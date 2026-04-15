using BaseExchange.OrderFlow.Domain.Entities;
using BaseExchange.OrderFlow.OrderGenerator.Application.DTOs;
using BaseExchange.OrderFlow.OrderGenerator.Application.Interfaces;
using BaseExchange.OrderFlow.OrderGenerator.Application.Exceptions;
using Microsoft.Extensions.Logging;

namespace BaseExchange.OrderFlow.OrderGenerator.Application.Services;

public interface IOrderService
{
    Task<OrderResponseDto> CreateOrderAsync(
        CreateOrderRequestDTO request,
        CancellationToken cancellationToken = default);
}

public class OrderService : IOrderService
{
    private readonly IFixOrderSender _sender;
    private readonly IFixConnection _connection;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IFixOrderSender sender,
        IFixConnection connection,
        ILogger<OrderService> logger)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<OrderResponseDto> CreateOrderAsync(
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

        await _connection.EnsureConnectedAsync(cancellationToken);

        var order = new Order(
            request.Symbol,
            request.Side,
            request.Quantity,
            request.Price
        );

        try
        {
            _sender.SendOrder(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "FIX send failed {@Order} {@Context}",
                order,
                new
                {
                    ErrorType = ex.GetType().Name,
                    Timestamp = DateTime.UtcNow
                });

            throw new OrderSendException(
                $"Failed to send order {order.Symbol}",
                ex);
        }

        _logger.LogInformation(
            "Order sent {Symbol} {Side} {Quantity} @ {Price}",
            order.Symbol,
            order.Side,
            order.Quantity,
            order.Price);

        return new OrderResponseDto(
            order.Symbol,
            order.Side,
            order.Quantity,
            order.Price,
            "sent",
            DateTime.UtcNow
        );
    }
}