using BaseExchange.OrderFlow.OrderAccumulator.Application.Services;
using BaseExchange.OrderFlow.Tests.TestConfiguration;
using FluentAssertions;
using Xunit;

namespace BaseExchange.OrderFlow.Tests.Unit.OrderAccumulator.Services;

/// <summary>
/// Testes que usam SharedFixture com IExposureService
/// Implementa SOLID: Depende de interface injetada
/// </summary>
[Collection("Exposure Service Collection")]
public class ExposureServiceSharedTests
{
    private readonly SharedFixture _fixture;

    public ExposureServiceSharedTests(SharedFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void ProcessOrder_WithSharedService_ShouldWork()
    {
        // Arrange
        var order = TestFactory.CreateOrder();

        // Act
        _fixture.ExposureService.ProcessOrder(order);

        // Assert
        var exposure = _fixture.ExposureService.GetExposure(order.Symbol);
        exposure.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Multiple_Operations_WithSharedService_ShouldMaintainState()
    {
        // Arrange
        var order1 = TestFactory.CreateOrder(quantity: 100);
        var order2 = TestFactory.CreateOrder(quantity: 50);

        // Act
        _fixture.ExposureService.ProcessOrder(order1);
        _fixture.ExposureService.ProcessOrder(order2);

        // Assert
        var totalExposure = _fixture.ExposureService.GetExposure(order1.Symbol);
        totalExposure.Should().Be((100 + 50) * order1.Price);
    }
}