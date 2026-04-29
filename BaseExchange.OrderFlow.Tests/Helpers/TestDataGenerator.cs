using BaseExchange.OrderFlow.Domain.Entities;
using BaseExchange.OrderFlow.Domain.Enums;

namespace BaseExchange.OrderFlow.Tests.Helpers;

/// <summary>
/// Gerador de dados de teste
/// Cria cenários complexos para testes
/// </summary>
public static class TestDataGenerator
{
    private static readonly Random _random = new();
    private static readonly string[] Symbols = { "PETR4", "VALE3", "VIIA4" };

    public static Order GenerateRandomOrder()
    {
        var symbol = Symbols[_random.Next(Symbols.Length)];
        var side = (SideEnum)_random.Next(0, 2);
        var quantity = _random.Next(1, 100000);
        var price = (decimal)_random.NextDouble() * 999.99m;

        // Normalize price to 0.01 increment
        price = Math.Round(price / 0.01m) * 0.01m;

        return new Order(symbol, side, quantity, Math.Max(price, 0.01m));
    }

    public static List<Order> GenerateOrderBatch(int count)
    {
        return Enumerable.Range(0, count)
            .Select(_ => GenerateRandomOrder())
            .ToList();
    }

    public static List<Order> GenerateOrdersForSymbol(string symbol, int count)
    {
        return Enumerable.Range(0, count)
            .Select(_ =>
            {
                var side = (SideEnum)_random.Next(0, 2);
                var quantity = _random.Next(1, 100000);
                var price = (decimal)_random.NextDouble() * 999.99m;
                price = Math.Round(price / 0.01m) * 0.01m;
                return new Order(symbol, side, quantity, Math.Max(price, 0.01m));
            })
            .ToList();
    }
}