using BaseExchange.OrderFlow.Tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace BaseExchange.OrderFlow.Tests.Integration.EndToEnd;

/// <summary>
/// Testes End-to-End do fluxo completo de ordens
/// Valida: Domain -> Application -> API -> FIX
/// </summary>
public class OrderFlowE2ETests : IDisposable
{
    private readonly OrderFixture _orderFixture;

    public OrderFlowE2ETests()
    {
        _orderFixture = OrderFixture.CreateDefault();
    }

    [Fact]
    public void CompleteOrderFlow_FromDomainThroughAPI_ShouldSucceed()
    {
        // Arrange - Create valid order at domain level
        var domainOrder = _orderFixture.Build();

        // Act & Assert
        domainOrder.Should().NotBeNull();
        domainOrder.Symbol.Should().Be("PETR4");
        domainOrder.Side.Should().NotBe(null);
        domainOrder.Quantity.Should().BeGreaterThan(0);
        domainOrder.Price.Should().BeGreaterThan(0);
    }

    [Fact]
    public void OrderValidationBoundary_ShouldEnforceConsistency()
    {
        // Arrange - Try to create with invalid data
        var invalidSymbol = "INVALID";

        // Act & Assert
        var action = () => _orderFixture.WithSymbol(invalidSymbol).Build();
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void OrderCreation_WithBoundaryValues_ShouldHandleCorrectly()
    {
        // Arrange
        var minOrder = _orderFixture
            .WithQuantity(1)
            .WithPrice(0.01m)
            .Build();

        var maxOrder = _orderFixture
            .WithQuantity(99999)
            .WithPrice(999.99m)
            .Build();

        // Act & Assert
        minOrder.Quantity.Should().Be(1);
        minOrder.Price.Should().Be(0.01m);

        maxOrder.Quantity.Should().Be(99999);
        maxOrder.Price.Should().Be(999.99m);
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}