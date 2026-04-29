using BaseExchange.OrderFlow.Domain.Enums;
using BaseExchange.OrderFlow.OrderGenerator.Application.DTOs;

namespace BaseExchange.OrderFlow.Tests.Fixtures;

/// <summary>
/// Fixture para criar DTOs de teste
/// Builder pattern para evitar duplicação
/// </summary>
public class CreateOrderRequestDTOFixture
{
    private string _symbol = "PETR4";
    private SideEnum _side = SideEnum.Buy;
    private int _quantity = 100;
    private decimal _price = 25.50m;

    public CreateOrderRequestDTOFixture WithSymbol(string symbol)
    {
        _symbol = symbol;
        return this;
    }

    public CreateOrderRequestDTOFixture WithSide(SideEnum side)
    {
        _side = side;
        return this;
    }

    public CreateOrderRequestDTOFixture WithQuantity(int quantity)
    {
        _quantity = quantity;
        return this;
    }

    public CreateOrderRequestDTOFixture WithPrice(decimal price)
    {
        _price = price;
        return this;
    }

    public CreateOrderRequestDTO Build()
    {
        return new CreateOrderRequestDTO(_symbol, _side, _quantity, _price);
    }

    public static CreateOrderRequestDTOFixture CreateDefault() => new();

    public static CreateOrderRequestDTOFixture CreateValid()
        => new CreateOrderRequestDTOFixture()
            .WithSymbol("VALE3")
            .WithSide(SideEnum.Buy)
            .WithQuantity(250)
            .WithPrice(45.30m);
}