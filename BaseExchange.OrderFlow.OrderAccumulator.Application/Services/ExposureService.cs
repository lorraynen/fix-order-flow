using BaseExchange.OrderFlow.Domain.Entities;
using BaseExchange.OrderFlow.Domain.Enums;
using System.Collections.Concurrent;

namespace BaseExchange.OrderFlow.OrderAccumulator.Application.Services;

/// <summary>
/// Implementação de rastreamento de exposição
/// Mantém dicionário thread-safe de exposições por símbolo
/// </summary>
public class ExposureService : IExposureService
{
    private readonly ConcurrentDictionary<string, decimal> _exposure = new();

    /// <summary>
    /// Processa uma ordem e atualiza exposição
    /// </summary>
    public void ProcessOrder(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        var symbol = order.Symbol.ToUpperInvariant();
        var value = decimal.Round(order.Price * order.Quantity, 2);

        var signedValue = order.Side == SideEnum.Buy ? value : -value;

        _exposure.AddOrUpdate(
            symbol,
            signedValue,
            (_, current) => current + signedValue
        );
    }

    /// <summary>
    /// Retorna exposição para um símbolo
    /// </summary>
    public decimal GetExposure(string symbol)
    {
        ArgumentNullException.ThrowIfNull(symbol);

        return _exposure.TryGetValue(symbol.ToUpperInvariant(), out var value)
            ? value
            : 0;
    }

    /// <summary>
    /// Retorna todas as exposições
    /// </summary>
    public IReadOnlyDictionary<string, decimal> GetAll()
    {
        return _exposure.ToDictionary(x => x.Key, x => x.Value);
    }
}