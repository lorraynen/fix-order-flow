using BaseExchange.OrderFlow.Domain.Entities;
using BaseExchange.OrderFlow.OrderGenerator.Application.Common;
using BaseExchange.OrderFlow.OrderGenerator.Application.DTOs;
using BaseExchange.OrderFlow.OrderGenerator.Application.Interfaces;
using BaseExchange.OrderFlow.OrderGenerator.Application.Exceptions;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace BaseExchange.OrderFlow.OrderGenerator.Application.Services;

public interface IOrderService
{
    Task<Result<OrderResponseDto>> CreateOrderAsync(
        CreateOrderRequestDTO request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Service para gerenciar criação de ordens
/// Implementa lógica de negócio: validação + criação + envio
/// Usa Result Pattern para tratamento de erros
/// </summary>
public class OrderService : IOrderService
{
    private readonly IFixOrderSender _sender;
    private readonly IFixConnection _connection;
    private readonly IValidator<CreateOrderRequestDTO> _validator;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IFixOrderSender sender,
        IFixConnection connection,
        IValidator<CreateOrderRequestDTO> validator,
        ILogger<OrderService> logger)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<OrderResponseDto>> CreateOrderAsync(
        CreateOrderRequestDTO request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation(
            "Creating order {Symbol} {Side} {Quantity} @ {Price}",
            request.Symbol,
            request.Side,
            request.Quantity,
            request.Price);

        // ✅ VALIDAÇÃO com FluentValidation
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("Validation failed: {@Errors}", errors);
            return Result<OrderResponseDto>.AsFailure(errors);
        }

        // ✅ Aguardar conexão FIX
        try
        {
            await _connection.EnsureConnectedAsync(cancellationToken);
        }
        catch (FixConnectionException ex)
        {
            _logger.LogError(ex, "FIX connection failed");
            return Result<OrderResponseDto>.AsFailure(ex.Message);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Connection attempt was cancelled");
            return Result<OrderResponseDto>.AsFailure("Connection attempt was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during connection");
            return Result<OrderResponseDto>.AsFailure($"Connection error: {ex.Message}");
        }

        // ✅ Criar ordem (validação de domínio automática no construtor)
        Order order;
        try
        {
            order = new Order(
                request.Symbol,
                request.Side,
                request.Quantity,
                request.Price
            );
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Order domain validation failed");
            return Result<OrderResponseDto>.AsFailure(ex.Message);
        }

        // ✅ Enviar ordem
        try
        {
            _sender.SendOrder(order);
        }
        catch (OrderSendException ex)
        {
            _logger.LogError(ex, "FIX send failed {@Order}", order);
            return Result<OrderResponseDto>.AsFailure(ex.Message);
        }
        catch (FixConnectionException ex)
        {
            _logger.LogError(ex, "FIX connection error while sending order");
            return Result<OrderResponseDto>.AsFailure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending order {@Order}", order);
            return Result<OrderResponseDto>.AsFailure($"Send error: {ex.Message}");
        }

        _logger.LogInformation(
            "Order sent {Symbol} {Side} {Quantity} @ {Price}",
            order.Symbol,
            order.Side,
            order.Quantity,
            order.Price);

        var response = new OrderResponseDto(
            order.Symbol,
            order.Side,
            order.Quantity,
            order.Price,
            "sent",
            DateTime.UtcNow
        );

        return Result<OrderResponseDto>.AsSuccess(response);
    }
}