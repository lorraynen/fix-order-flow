using BaseExchange.OrderFlow.Domain.Entities;
using BaseExchange.OrderFlow.Domain.Enums;

namespace BaseExchange.OrderFlow.Tests.Fixtures;

/// <summary>
/// Fixture para criar dados de teste para Order
/// Implementa Builder Pattern para evitar duplicação
/// </summary>
public class OrderFixture
{
    private string _symbol = "PETR4";
    private SideEnum _side = SideEnum.Buy;
    private int _quantity = 100;
    private decimal _price = 25.50m;

    public OrderFixture WithSymbol(string symbol)
    {
        _symbol = symbol;
        return this;
    }

    public OrderFixture WithSide(SideEnum side)
    {
        _side = side;
        return this;
    }

    public OrderFixture WithQuantity(int quantity)
    {
        _quantity = quantity;
        return this;
    }

    public OrderFixture WithPrice(decimal price)
    {
        _price = price;
        return this;
    }

    public Order Build()
    {
        return new Order(_symbol, _side, _quantity, _price);
    }

    public static OrderFixture CreateDefault() => new();

    public static OrderFixture CreateBuyOrder() 
        => new OrderFixture().WithSide(SideEnum.Buy).WithQuantity(100).WithPrice(25.50m);

    public static OrderFixture CreateSellOrder() 
        => new OrderFixture().WithSide(SideEnum.Sell).WithQuantity(50).WithPrice(30.75m);

    public static OrderFixture CreateLargeOrder() 
        => new OrderFixture().WithQuantity(50000).WithPrice(500.00m);

    public static OrderFixture CreateSmallOrder() 
        => new OrderFixture().WithQuantity(1).WithPrice(0.01m);
}