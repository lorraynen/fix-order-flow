using BaseExchange.OrderFlow.Domain.Entities;
using BaseExchange.OrderFlow.OrderAccumulator.Application.Services;
using BaseExchange.OrderFlow.Tests.TestConfiguration;
using FluentAssertions;
using Xunit;

namespace BaseExchange.OrderFlow.Tests.Unit.OrderAccumulator.Services;

/// <summary>
/// Testes para IExposureService (implementação real)
/// Implementa SOLID: Usa TestFactory para abstração
/// </summary>
public class ExposureServiceTests
{
    #region Single Order Tests

    [Fact]
    public void ProcessOrder_WithBuyOrder_Real_ShouldAddPositiveExposure()
    {
        // Arrange
        var exposureService = TestFactory.CreateExposureService();
        var order = TestFactory.CreateOrder(side: BaseExchange.OrderFlow.Domain.Enums.SideEnum.Buy);
        var expectedExposure = order.Price * order.Quantity;

        // Act
        exposureService.ProcessOrder(order);

        // Assert
        var exposure = exposureService.GetExposure(order.Symbol);
        exposure.Should().Be(expectedExposure);
    }

    [Fact]
    public void ProcessOrder_WithSellOrder_Real_ShouldAddNegativeExposure()
    {
        // Arrange
        var exposureService = TestFactory.CreateExposureService();
        var order = TestFactory.CreateOrder(side: BaseExchange.OrderFlow.Domain.Enums.SideEnum.Sell);
        var expectedExposure = -(order.Price * order.Quantity);

        // Act
        exposureService.ProcessOrder(order);

        // Assert
        var exposure = exposureService.GetExposure(order.Symbol);
        exposure.Should().Be(expectedExposure);
    }

    [Fact]
    public void ProcessOrder_WithMultipleBuyOrders_Real_ShouldAccumulateExposure()
    {
        // Arrange
        var exposureService = TestFactory.CreateExposureService();
        var order1 = TestFactory.CreateOrder(quantity: 100, price: 25.50m);
        var order2 = TestFactory.CreateOrder(quantity: 50, price: 30.00m);
        var expectedExposure = (100 * 25.50m) + (50 * 30.00m);

        // Act
        exposureService.ProcessOrder(order1);
        exposureService.ProcessOrder(order2);

        // Assert
        var exposure = exposureService.GetExposure(order1.Symbol);
        exposure.Should().Be(expectedExposure);
    }

    [Fact]
    public void ProcessOrder_WithBuyAndSellOrders_Real_ShouldCalculateNetExposure()
    {
        // Arrange
        var exposureService = TestFactory.CreateExposureService();
        var buyOrder = TestFactory.CreateOrder(
            side: BaseExchange.OrderFlow.Domain.Enums.SideEnum.Buy,
            quantity: 100,
            price: 25.50m);
        var sellOrder = TestFactory.CreateOrder(
            side: BaseExchange.OrderFlow.Domain.Enums.SideEnum.Sell,
            quantity: 50,
            price: 25.50m);
        var expectedExposure = (100 * 25.50m) - (50 * 25.50m);

        // Act
        exposureService.ProcessOrder(buyOrder);
        exposureService.ProcessOrder(sellOrder);

        // Assert
        var exposure = exposureService.GetExposure("PETR4");
        exposure.Should().Be(expectedExposure);
    }

    [Fact]
    public void ProcessOrder_WithNettingOrders_Real_ShouldResultInZeroExposure()
    {
        // Arrange
        var exposureService = TestFactory.CreateExposureService();
        var buyOrder = TestFactory.CreateOrder(
            side: BaseExchange.OrderFlow.Domain.Enums.SideEnum.Buy,
            quantity: 100,
            price: 30.00m);
        var sellOrder = TestFactory.CreateOrder(
            side: BaseExchange.OrderFlow.Domain.Enums.SideEnum.Sell,
            quantity: 100,
            price: 30.00m);

        // Act
        exposureService.ProcessOrder(buyOrder);
        exposureService.ProcessOrder(sellOrder);

        // Assert
        var exposure = exposureService.GetExposure("PETR4");
        exposure.Should().Be(0);
    }

    [Fact]
    public void ProcessOrder_WithMultipleSymbols_Real_ShouldTrackIndependently()
    {
        // Arrange
        var exposureService = TestFactory.CreateExposureService();
        var petr4Order = TestFactory.CreateOrder(symbol: "PETR4", quantity: 100);
        var vale3Order = TestFactory.CreateOrder(symbol: "VALE3", quantity: 200);

        // Act
        exposureService.ProcessOrder(petr4Order);
        exposureService.ProcessOrder(vale3Order);

        // Assert
        exposureService.GetExposure("PETR4").Should().Be(100 * petr4Order.Price);
        exposureService.GetExposure("VALE3").Should().Be(200 * vale3Order.Price);
    }

    [Fact]
    public void GetExposure_WithUnknownSymbol_Real_ShouldReturnZero()
    {
        // Arrange
        var exposureService = TestFactory.CreateExposureService();

        // Act
        var exposure = exposureService.GetExposure("UNKNOWN");

        // Assert
        exposure.Should().Be(0);
    }

    [Fact]
    public void GetExposure_WithNullSymbol_Real_ShouldThrowArgumentNullException()
    {
        // Arrange
        var exposureService = TestFactory.CreateExposureService();

        // Act & Assert
        var action = () => exposureService.GetExposure(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetAll_WithMultipleOrders_Real_ShouldReturnAllExposures()
    {
        // Arrange
        var exposureService = TestFactory.CreateExposureService();
        var petr4Order = TestFactory.CreateOrder(symbol: "PETR4");
        var vale3Order = TestFactory.CreateOrder(symbol: "VALE3");

        exposureService.ProcessOrder(petr4Order);
        exposureService.ProcessOrder(vale3Order);

        // Act
        var allExposures = exposureService.GetAll();

        // Assert
        allExposures.Should().HaveCount(2);
        allExposures.Should().ContainKeys("PETR4", "VALE3");
    }

    [Fact]
    public void GetAll_WithNoOrders_Real_ShouldReturnEmpty()
    {
        // Arrange
        var exposureService = TestFactory.CreateExposureService();

        // Act
        var allExposures = exposureService.GetAll();

        // Assert
        allExposures.Should().BeEmpty();
    }

    [Fact]
    public void ProcessOrder_WithConcurrentOrders_Real_ShouldHandleThreadSafely()
    {
        // Arrange
        var exposureService = TestFactory.CreateExposureService();
        const int numberOfOrders = 1000;
        var orders = Enumerable.Range(0, numberOfOrders)
            .Select(i => TestFactory.CreateOrder(quantity: 1, price: 10.00m))
            .ToList();

        // Act
        Parallel.ForEach(orders, order => exposureService.ProcessOrder(order));

        // Assert
        var exposure = exposureService.GetExposure("PETR4");
        var expectedExposure = numberOfOrders * 1 * 10.00m;
        exposure.Should().Be(expectedExposure);
    }

    [Fact]
    public void ProcessOrder_WithNullOrder_Real_ShouldThrowArgumentNullException()
    {
        // Arrange
        var exposureService = TestFactory.CreateExposureService();

        // Act & Assert
        var action = () => exposureService.ProcessOrder(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    #endregion
}