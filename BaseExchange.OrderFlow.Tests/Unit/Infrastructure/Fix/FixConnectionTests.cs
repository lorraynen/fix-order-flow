using BaseExchange.OrderFlow.OrderGenerator.Application.Exceptions;
using BaseExchange.OrderFlow.OrderGenerator.Application.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BaseExchange.OrderFlow.Tests.Unit.Infrastructure.Fix;

/// <summary>
/// Testes para interface IFixConnection
/// Não testa implementação específica, mas contrato da interface
/// </summary>
public class FixConnectionTests
{
    private readonly Mock<ILogger<MockFixConnection>> _mockLogger;

    public FixConnectionTests()
    {
        _mockLogger = new Mock<ILogger<MockFixConnection>>();
    }

    /// <summary>
    /// Mock simples de IFixConnection para testes
    /// </summary>
    private class MockFixConnection : IFixConnection
    {
        private bool _isConnected;
        private int _connectAttempt;
        private readonly int _connectAfterAttempts;

        public MockFixConnection(int connectAfterAttempts = 1)
        {
            _connectAfterAttempts = connectAfterAttempts;
            _connectAttempt = 0;
            _isConnected = false;
        }

        public bool IsConnected() => _isConnected;

        public async Task EnsureConnectedAsync(CancellationToken cancellationToken)
        {
            while (!_isConnected && _connectAttempt < _connectAfterAttempts)
            {
                await Task.Delay(10, cancellationToken);
                _connectAttempt++;
            }

            if (!_isConnected && _connectAttempt >= _connectAfterAttempts)
            {
                throw new FixConnectionException("Could not connect");
            }

            _isConnected = true;
        }
    }

    [Fact]
    public void IsConnected_WhenNotConnected_ShouldReturnFalse()
    {
        // Arrange
        var connection = new MockFixConnection();

        // Act
        var result = connection.IsConnected();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task EnsureConnectedAsync_WhenConnectionTimeout_ShouldThrowException()
    {
        // Arrange
        var connection = new MockFixConnection(connectAfterAttempts: 0);

        // Act & Assert
        await Assert.ThrowsAsync<FixConnectionException>(() =>
            connection.EnsureConnectedAsync(CancellationToken.None));
    }

    [Fact]
    public async Task EnsureConnectedAsync_WhenCancelled_ShouldThrowTaskCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.CancelAfter(5);

        var connection = new MockFixConnection(connectAfterAttempts: 1000);

        // Act & Assert
        // ✅ Task.Delay com CancellationToken lança TaskCanceledException (subclasse de OperationCanceledException)
        var exception = await Assert.ThrowsAsync<TaskCanceledException>(() =>
            connection.EnsureConnectedAsync(cts.Token));

        // Validar que é uma subclasse de OperationCanceledException
        exception.Should().BeOfType<TaskCanceledException>();
        exception.Should().BeAssignableTo<OperationCanceledException>();
    }
}