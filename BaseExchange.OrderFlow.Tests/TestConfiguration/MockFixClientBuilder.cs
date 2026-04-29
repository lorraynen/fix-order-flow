using BaseExchange.OrderFlow.Domain.Entities;
using BaseExchange.OrderFlow.OrderGenerator.Application.Interfaces;
using Moq;

namespace BaseExchange.OrderFlow.Tests.TestConfiguration;

/// <summary>
/// Builder para criar mocks customizados do IFixClient
/// Facilita reutilização de mocks complexos
/// </summary>
public class MockFixClientBuilder
{
    private readonly Mock<IFixConnection> _mock;
    private readonly Mock<IFixOrderSender> _mockFixOrder;

    public MockFixClientBuilder()
    {
        _mock = new Mock<IFixConnection>(MockBehavior.Strict);
    }

    public MockFixClientBuilder WithSuccessfulConnection()
    {
        _mock
            .Setup(x => x.EnsureConnectedAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockFixOrder
            .Setup(x => x.SendOrder(It.IsAny<Order>()));

        return this;
    }

    public MockFixClientBuilder WithFailedConnection(string reason = "Connection failed")
    {
        _mock
            .Setup(x => x.EnsureConnectedAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(reason));

        return this;
    }

    public MockFixClientBuilder WithSendOrderFailure(string reason = "Send failed")
    {
        _mockFixOrder
            .Setup(x => x.SendOrder(It.IsAny<Order>()))
            .Throws(new Exception(reason));

        return this;
    }

    public IFixConnection Build()
    {
        return _mock.Object;
    }

    public Mock<IFixConnection> BuildMock()
    {
        return _mock;
    }
}