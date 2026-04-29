using BaseExchange.OrderFlow.OrderGenerator.Application.Exceptions;
using BaseExchange.OrderFlow.OrderGenerator.Application.Interfaces;
using Moq;

namespace BaseExchange.OrderFlow.Tests.TestConfiguration;

/// <summary>
/// Builder para criar mocks customizados de IFixConnection
/// Facilita reutilização de cenários complexos
/// </summary>
public class MockFixConnectionBuilder
{
    private readonly Mock<IFixConnection> _mock;

    public MockFixConnectionBuilder()
    {
        _mock = new Mock<IFixConnection>(MockBehavior.Strict);
    }

    public MockFixConnectionBuilder WithSuccessfulConnection()
    {
        _mock
            .Setup(x => x.IsConnected())
            .Returns(true);

        _mock
            .Setup(x => x.EnsureConnectedAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return this;
    }

    public MockFixConnectionBuilder WithFailedConnection(string reason = "Connection failed")
    {
        _mock
            .Setup(x => x.IsConnected())
            .Returns(false);

        _mock
            .Setup(x => x.EnsureConnectedAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FixConnectionException(reason));

        return this;
    }

    public MockFixConnectionBuilder WithConnectionTimeout()
    {
        _mock
            .Setup(x => x.IsConnected())
            .Returns(false);

        _mock
            .Setup(x => x.EnsureConnectedAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("Connection timeout"));

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