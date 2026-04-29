using BaseExchange.OrderFlow.Domain.Entities;
using BaseExchange.OrderFlow.OrderGenerator.Application.Exceptions;
using BaseExchange.OrderFlow.OrderGenerator.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace BaseExchange.OrderFlow.Infrastructure.Fix;

/// <summary>
/// Implementa IFixOrderSender
/// Responsável APENAS por enviar ordens via FIX
/// Implementa SRP: Single Responsibility Principle
/// </summary>
public class FixOrderSender : IFixOrderSender
{
    private readonly FixClient _fixClient;
    private readonly ILogger<FixOrderSender> _logger;

    public FixOrderSender(FixClient fixClient, ILogger<FixOrderSender> logger)
    {
        _fixClient = fixClient ?? throw new ArgumentNullException(nameof(fixClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Envia uma ordem via FIX protocol
    /// </summary>
    public void SendOrder(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        try
        {
            _logger.LogInformation(
                "Sending order {Symbol} {Side} {Quantity}@{Price}",
                order.Symbol, order.Side, order.Quantity, order.Price);

            _fixClient.SendOrder(order);

            _logger.LogInformation(
                "Order sent successfully {Symbol}",
                order.Symbol);
        }
        catch (FixConnectionException ex)
        {
            _logger.LogError(ex, "FIX connection error while sending order {Symbol}", order.Symbol);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending order {Symbol}: {Error}", order.Symbol, ex.Message);
            throw new OrderSendException($"Failed to send order {order.Symbol}", ex);
        }
    }
}