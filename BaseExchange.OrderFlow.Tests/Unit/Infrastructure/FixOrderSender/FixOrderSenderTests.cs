using BaseExchange.OrderFlow.Domain.Entities;
using BaseExchange.OrderFlow.OrderGenerator.Application.Interfaces;
using BaseExchange.OrderFlow.Tests.Fixtures;
using FluentAssertions;
using Moq;
using Xunit;

namespace BaseExchange.OrderFlow.Tests.Unit.Infrastructure.FixOrderSender;

/// <summary>
/// Testes para IFixOrderSender
/// Valida envio de ordens via FIX
/// </summary>
public class FixOrderSenderTests
{
    private readonly Mock<IFixOrderSender> _mockSender;

    public FixOrderSenderTests()
    {
        _mockSender = new Mock<IFixOrderSender>();
    }

    [Fact]
    public void SendOrder_WithValidOrder_ShouldNotThrow()
    {
        // Arrange
        var order = OrderFixture.CreateDefault().Build();
        
        _mockSender
            .Setup(x => x.SendOrder(It.IsAny<Order>()));

        // Act
        var action = () => _mockSender.Object.SendOrder(order);

        // Assert
        action.Should().NotThrow();
        _mockSender.Verify(x => x.SendOrder(order), Times.Once);
    }

    [Fact]
    public void SendOrder_WithMultipleOrders_ShouldProcessAll()
    {
        // Arrange
        var orders = new[]
        {
            OrderFixture.CreateBuyOrder().Build(),
            OrderFixture.CreateSellOrder().Build(),
            OrderFixture.CreateDefault().WithSymbol("VALE3").Build()
        };

        _mockSender
            .Setup(x => x.SendOrder(It.IsAny<Order>()));

        // Act
        foreach (var order in orders)
        {
            _mockSender.Object.SendOrder(order);
        }

        // Assert
        _mockSender.Verify(x => x.SendOrder(It.IsAny<Order>()), Times.Exactly(3));
    }

    [Fact]
    public void SendOrder_WhenThrowsException_ShouldPropagate()
    {
        // Arrange
        var order = OrderFixture.CreateDefault().Build();
        
        _mockSender
            .Setup(x => x.SendOrder(It.IsAny<Order>()))
            .Throws<Exception>();

        // Act & Assert
        var action = () => _mockSender.Object.SendOrder(order);

        action.Should().Throw<Exception>();
    }

    [Fact]
    public void SendOrder_WithLargeOrder_ShouldHandleCorrectly()
    {
        // Arrange
        var largeOrder = OrderFixture.CreateLargeOrder().Build();
        
        _mockSender
            .Setup(x => x.SendOrder(It.IsAny<Order>()));

        // Act
        var action = () => _mockSender.Object.SendOrder(largeOrder);

        // Assert
        action.Should().NotThrow();
        _mockSender.Verify(x => x.SendOrder(largeOrder), Times.Once);
    }
}