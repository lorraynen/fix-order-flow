using BaseExchange.OrderFlow.Domain.Entities;
using BaseExchange.OrderFlow.Domain.Enums;
using BaseExchange.OrderFlow.Tests.TestConfiguration;
using FluentAssertions;
using Xunit;

namespace BaseExchange.OrderFlow.Tests.Unit.Domain.Entities;

/// <summary>
/// Testes para a entidade Order
/// Implementa SOLID: Usa Factory Pattern para criação de objetos
/// Não instancia classes diretamente em testes, usa abstrações
/// </summary>
public class OrderTests
{
    #region Valid Creation Tests

    [Fact]
    public void Order_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange & Act - Usar TestFactory.Method (static), não _factory.Method
        var order = TestFactory.CreateOrder();

        // Assert
        order.Should().NotBeNull();
        order.Symbol.Should().Be("PETR4");
        order.Side.Should().Be(SideEnum.Buy);
        order.Quantity.Should().Be(100);
        order.Price.Should().Be(25.50m);
    }

    [Theory]
    [InlineData("PETR4")]
    [InlineData("VALE3")]
    [InlineData("VIIA4")]
    public void Order_WithValidSymbols_ShouldCreateSuccessfully(string symbol)
    {
        // Arrange & Act
        var order = TestFactory.CreateOrder(symbol: symbol);

        // Assert
        order.Symbol.Should().Be(symbol);
    }

    [Theory]
    [InlineData(SideEnum.Buy)]
    [InlineData(SideEnum.Sell)]
    public void Order_WithValidSides_ShouldCreateSuccessfully(SideEnum side)
    {
        // Arrange & Act
        var order = TestFactory.CreateOrder(side: side);

        // Assert
        order.Side.Should().Be(side);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(99999)]
    public void Order_WithValidQuantities_ShouldCreateSuccessfully(int quantity)
    {
        // Arrange & Act
        var order = TestFactory.CreateOrder(quantity: quantity);

        // Assert
        order.Quantity.Should().Be(quantity);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(10.50)]
    [InlineData(999.99)]
    public void Order_WithValidPrices_ShouldCreateSuccessfully(decimal price)
    {
        // Arrange & Act
        var order = TestFactory.CreateOrder(price: price);

        // Assert
        order.Price.Should().Be(price);
    }

    #endregion

    #region Invalid Symbol Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Order_WithEmptySymbol_ShouldThrowArgumentException(string? symbol)
    {
        // Arrange & Act & Assert
        var action = () => TestFactory.CreateOrder(symbol: symbol ?? "");

        action.Should().Throw<ArgumentException>()
            .WithMessage("Symbol is required");
    }

    [Theory]
    [InlineData("INVALID")]
    [InlineData("XYZ123")]
    [InlineData("AAPL")]
    public void Order_WithInvalidSymbol_ShouldThrowArgumentException(string symbol)
    {
        // Arrange & Act & Assert
        var action = () => TestFactory.CreateOrder(symbol: symbol);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Invalid symbol");
    }

    [Fact]
    public void Order_WithCaseMismatchSymbol_ShouldWork()
    {
        // Arrange & Act
        var order = TestFactory.CreateOrder(symbol: "petr4");

        // Assert
        order.Symbol.Should().Be("petr4");
    }

    #endregion

    #region Invalid Quantity Tests

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Order_WithZeroOrNegativeQuantity_ShouldThrowArgumentException(int quantity)
    {
        // Arrange & Act & Assert
        var action = () => TestFactory.CreateOrder(quantity: quantity);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Quantity must be between 1 and 99999");
    }

    [Theory]
    [InlineData(100000)]
    [InlineData(100001)]
    [InlineData(1000000)]
    public void Order_WithQuantityExceedingMax_ShouldThrowArgumentException(int quantity)
    {
        // Arrange & Act & Assert
        var action = () => TestFactory.CreateOrder(quantity: quantity);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Quantity must be between 1 and 99999");
    }

    #endregion

    #region Invalid Price Tests

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void Order_WithZeroOrNegativePrice_ShouldThrowArgumentException(decimal price)
    {
        // Arrange & Act & Assert
        var action = () => TestFactory.CreateOrder(price: price);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Price must be between 0.01 and 999.99");
    }

    [Theory]
    [InlineData(1000)]
    [InlineData(1000.01)]
    [InlineData(9999.99)]
    public void Order_WithPriceExceedingMax_ShouldThrowArgumentException(decimal price)
    {
        // Arrange & Act & Assert
        var action = () => TestFactory.CreateOrder(price: price);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Price must be between 0.01 and 999.99");
    }

    [Theory]
    [InlineData(0.015)]
    [InlineData(10.123)]
    [InlineData(99.999)]
    [InlineData(50.555)]
    public void Order_WithPriceNotMultipleOfPointZeroOne_ShouldThrowArgumentException(decimal price)
    {
        // Arrange & Act & Assert
        var action = () => TestFactory.CreateOrder(price: price);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Price must be multiple of 0.01");
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(10.50)]
    [InlineData(100.99)]
    [InlineData(999.99)]
    public void Order_WithValidPriceMultiple_ShouldCreateSuccessfully(decimal price)
    {
        // Arrange & Act
        var order = TestFactory.CreateOrder(price: price);

        // Assert
        order.Price.Should().Be(price);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Order_WithMinimumQuantityAndPrice_ShouldCreateSuccessfully()
    {
        // Arrange & Act
        var order = TestFactory.CreateOrder(quantity: 1, price: 0.01m);

        // Assert
        order.Quantity.Should().Be(1);
        order.Price.Should().Be(0.01m);
    }

    [Fact]
    public void Order_WithMaximumQuantityAndPrice_ShouldCreateSuccessfully()
    {
        // Arrange & Act
        var order = TestFactory.CreateOrder(quantity: 99999, price: 999.99m);

        // Assert
        order.Quantity.Should().Be(99999);
        order.Price.Should().Be(999.99m);
    }

    [Fact]
    public void Order_WithLargeOrder_ShouldMaintainAccuracy()
    {
        // Arrange & Act
        var order = TestFactory.CreateOrder(quantity: 50000, price: 500.00m);

        // Assert
        var expectedValue = 50000 * 500.00m;
        (order.Quantity * order.Price).Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(0.01, 1)]
    [InlineData(0.01, 99999)]
    [InlineData(999.99, 1)]
    [InlineData(999.99, 99999)]
    public void Order_WithBoundaryValues_ShouldCreateSuccessfully(decimal price, int quantity)
    {
        // Arrange & Act
        var order = TestFactory.CreateOrder(price: price, quantity: quantity);

        // Assert
        order.Price.Should().Be(price);
        order.Quantity.Should().Be(quantity);
    }

    #endregion
}