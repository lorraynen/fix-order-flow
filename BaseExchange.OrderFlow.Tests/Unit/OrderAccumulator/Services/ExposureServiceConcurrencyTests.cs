using BaseExchange.OrderFlow.OrderAccumulator.Application.Services;
using BaseExchange.OrderFlow.Tests.TestConfiguration;
using FluentAssertions;
using Xunit;

namespace BaseExchange.OrderFlow.Tests.Unit.OrderAccumulator.Services;

/// <summary>
/// Testes de concorrência para ExposureService
/// Valida thread-safety com múltiplas threads simultâneas
/// </summary>
public class ExposureServiceConcurrencyTests
{
    [Fact]
    public async Task ProcessOrder_WithConcurrentOrdersSameSymbol_ShouldMaintainAccuracy()
    {
        // Arrange
        var service = new ExposureService();
        const int orderCount = 1000;
        const string symbol = "PETR4";
        const decimal pricePerOrder = 25.50m;
        const int quantityPerOrder = 100;

        var orders = Enumerable.Range(0, orderCount)
            .Select(i => TestFactory.CreateOrder(
                symbol: symbol,
                quantity: quantityPerOrder,
                price: pricePerOrder))
            .ToList();

        var expectedExposure = decimal.Round(
            orderCount * pricePerOrder * quantityPerOrder, 2);

        // Act
        await Task.WhenAll(
            orders.Select(o => Task.Run(() => service.ProcessOrder(o)))
        );

        // Assert
        var actualExposure = service.GetExposure(symbol);
        actualExposure.Should().Be(expectedExposure);
    }

    [Fact]
    public async Task ProcessOrder_WithConcurrentOrdersDifferentSymbols_ShouldIsolateProperly()
    {
        // Arrange
        var service = new ExposureService();
        var symbols = new[] { "PETR4", "VALE3", "VIIA4" };
        const int ordersPerSymbol = 100;

        var allOrders = symbols
            .SelectMany(symbol =>
                Enumerable.Range(0, ordersPerSymbol)
                    .Select(_ => TestFactory.CreateOrder(symbol: symbol)))
            .ToList();

        // Act
        await Task.WhenAll(
            allOrders.Select(o => Task.Run(() => service.ProcessOrder(o)))
        );

        // Assert
        foreach (var symbol in symbols)
        {
            var exposure = service.GetExposure(symbol);
            exposure.Should().BeGreaterThan(0);
        }

        var allExposures = service.GetAll();
        allExposures.Should().HaveCount(3);
    }

    [Fact]
    public async Task ProcessOrder_WithBuyAndSellConcurrent_ShouldCalculateNetExposureCorrectly()
    {
        // Arrange
        var service = new ExposureService();
        const int iterations = 500;

        // Criar 500 compras e 500 vendas
        var buyOrders = Enumerable.Range(0, iterations)
            .Select(_ => TestFactory.CreateOrder(
                side: BaseExchange.OrderFlow.Domain.Enums.SideEnum.Buy,
                quantity: 100,
                price: 25.50m))
            .ToList();

        var sellOrders = Enumerable.Range(0, iterations)
            .Select(_ => TestFactory.CreateOrder(
                side: BaseExchange.OrderFlow.Domain.Enums.SideEnum.Sell,
                quantity: 100,
                price: 25.50m))
            .ToList();

        var allOrders = buyOrders.Concat(sellOrders).ToList();
        var expectedNetExposure = 0m; // Todas as compras serão compensadas pelas vendas

        // Act
        await Task.WhenAll(
            allOrders.Select(o => Task.Run(() => service.ProcessOrder(o)))
        );

        // Assert
        var netExposure = service.GetExposure("PETR4");
        netExposure.Should().Be(expectedNetExposure);
    }

    [Fact]
    public async Task ProcessOrder_RapidSequential_ShouldHandleAllOrders()
    {
        // Arrange
        var service = new ExposureService();
        const int orderCount = 10000;

        var orders = Enumerable.Range(0, orderCount)
            .Select(i => TestFactory.CreateOrder(
                symbol: i % 3 == 0 ? "PETR4" : i % 3 == 1 ? "VALE3" : "VIIA4",
                quantity: 10 + i % 100))
            .ToList();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await Task.WhenAll(
            orders.Select(o => Task.Run(() => service.ProcessOrder(o)))
        );
        stopwatch.Stop();

        // Assert
        var allExposures = service.GetAll();
        allExposures.Should().HaveCount(3);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // Deve processar em menos de 5s
    }
}