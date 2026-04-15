using BaseExchange.OrderFlow.Domain.Enums;

namespace BaseExchange.OrderFlow.Domain.Entities;

public class Order
{
    private static readonly HashSet<string> AllowedSymbols = new(StringComparer.OrdinalIgnoreCase)
                                                             { "PETR4", "VALE3", "VIIA4" };

    public string Symbol { get; }
    public SideEnum Side { get; }
    public int Quantity { get; }
    public decimal Price { get; }

    public Order(string symbol, SideEnum side, int quantity, decimal price)
    {
        Validate(symbol, side, quantity, price);

        Symbol = symbol;
        Side = side;
        Quantity = quantity;
        Price = price;
    }
    private static void Validate(string symbol, SideEnum side, int quantity, decimal price)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol is required");

        if (!AllowedSymbols.Contains(symbol))
            throw new ArgumentException("Invalid symbol");

        if (!Enum.IsDefined(typeof(SideEnum), side))
            throw new ArgumentException("Invalid side");

        if (quantity <= 0 || quantity >= 100000)
            throw new ArgumentException("Quantity must be between 1 and 99999");

        if (price < 0.01m || price >= 1000)
            throw new ArgumentException("Price must be between 0.01 and 999.99");

        if (price % 0.01m != 0)
            throw new ArgumentException("Price must be multiple of 0.01");
    }
   
}