using BaseExchange.OrderFlow.Domain.Enums;
using BaseExchange.OrderFlow.OrderGenerator.Application.DTOs;
using BaseExchange.OrderFlow.OrderGenerator.Application.Validators;
using BaseExchange.OrderFlow.Tests.TestConfiguration;
using FluentAssertions;
using Xunit;

namespace BaseExchange.OrderFlow.Tests.Unit.Application.Validators;

/// <summary>
/// Testes para CreateOrderValidator
/// Implementa SOLID: Usa TestFactory para abstração de criação de objetos
/// </summary>
public class CreateOrderValidatorTests
{
    private readonly CreateOrderValidator _validator = new();

    #region Valid Request Tests

    [Fact]
    public async Task ValidateAsync_WithValidRequest_ShouldSucceed()
    {
        // Arrange
        var request = TestFactory.CreateOrderRequest(
            symbol: "VALE3",
            side: SideEnum.Buy,
            quantity: 250,
            price: 45.30m);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("PETR4")]
    [InlineData("VALE3")]
    [InlineData("VIIA4")]
    public async Task ValidateAsync_WithValidSymbols_ShouldSucceed(string symbol)
    {
        // Arrange
        var request = TestFactory.CreateOrderRequest(symbol: symbol);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Symbol Validation Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ValidateAsync_WithEmptySymbol_ShouldFail(string? symbol)
    {
        // Arrange
        var request = TestFactory.CreateOrderRequest(symbol: symbol ?? "");

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Symbol");
    }

    [Theory]
    [InlineData("INVALID")]
    [InlineData("XYZ")]
    [InlineData("AAPL")]
    public async Task ValidateAsync_WithInvalidSymbol_ShouldFail(string symbol)
    {
        // Arrange
        var request = TestFactory.CreateOrderRequest(symbol: symbol);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.PropertyName == "Symbol" && 
            e.ErrorMessage.Contains("must be one of"));
    }

    [Fact]
    public async Task ValidateAsync_WithSymbolExceedingMaxLength_ShouldFail()
    {
        // Arrange
        var request = TestFactory.CreateOrderRequest(
            symbol: "PETR4TOOLONG");

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Symbol");
    }

    #endregion

    #region Quantity Validation Tests

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task ValidateAsync_WithZeroOrNegativeQuantity_ShouldFail(int quantity)
    {
        // Arrange
        var request = TestFactory.CreateOrderRequest(quantity: quantity);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Quantity");
    }

    [Theory]
    [InlineData(100000)]
    [InlineData(100001)]
    public async Task ValidateAsync_WithQuantityExceedingMax_ShouldFail(int quantity)
    {
        // Arrange
        var request = TestFactory.CreateOrderRequest(quantity: quantity);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Quantity");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(99999)]
    public async Task ValidateAsync_WithValidQuantity_ShouldSucceed(int quantity)
    {
        // Arrange
        var request = TestFactory.CreateOrderRequest(quantity: quantity);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Price Validation Tests

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public async Task ValidateAsync_WithZeroOrNegativePrice_ShouldFail(decimal price)
    {
        // Arrange
        var request = TestFactory.CreateOrderRequest(price: price);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }

    [Theory]
    [InlineData(1000)]
    [InlineData(1000.01)]
    [InlineData(9999.99)]
    public async Task ValidateAsync_WithPriceExceedingMax_ShouldFail(decimal price)
    {
        // Arrange
        var request = TestFactory.CreateOrderRequest(price: price);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }

    [Theory]
    [InlineData(0.015)]
    [InlineData(10.123)]
    [InlineData(99.999)]
    public async Task ValidateAsync_WithPriceNotMultipleOfPointZeroOne_ShouldFail(decimal price)
    {
        // Arrange
        var request = TestFactory.CreateOrderRequest(price: price);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.PropertyName == "Price" && 
            e.ErrorMessage.Contains("multiple of 0.01"));
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(10.50)]
    [InlineData(100.99)]
    [InlineData(999.99)]
    public async Task ValidateAsync_WithValidPrice_ShouldSucceed(decimal price)
    {
        // Arrange
        var request = TestFactory.CreateOrderRequest(price: price);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Multiple Errors Tests

    [Fact]
    public async Task ValidateAsync_WithMultipleErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var request = TestFactory.CreateOrderRequest(
            symbol: "INVALID",
            quantity: -1,
            price: -10);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    #endregion

    #region Side Validation Tests

    [Fact]
    public async Task ValidateAsync_WithValidBuySide_ShouldSucceed()
    {
        // Arrange
        var request = TestFactory.CreateOrderRequest(side: SideEnum.Buy);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WithValidSellSide_ShouldSucceed()
    {
        // Arrange
        var request = TestFactory.CreateOrderRequest(side: SideEnum.Sell);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion
}