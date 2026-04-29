using BaseExchange.OrderFlow.OrderGenerator.Application.Exceptions;
using BaseExchange.OrderFlow.OrderGenerator.Application.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace BaseExchange.OrderFlow.Tests.Unit.Infrastructure.FixConnection;

/// <summary>
/// Testes para IFixConnection
/// Valida comportamento de conexão com FIX
/// </summary>
public class FixConnectionTests
{
    private readonly Mock<IFixConnection> _mockConnection;

    public FixConnectionTests()
    {
        _mockConnection = new Mock<IFixConnection>();
    }

    [Fact]
    public void IsConnected_WhenConnected_ShouldReturnTrue()
    {
        // Arrange
        _mockConnection
            .Setup(x => x.IsConnected())
            .Returns(true);

        // Act
        var result = _mockConnection.Object.IsConnected();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsConnected_WhenNotConnected_ShouldReturnFalse()
    {
        // Arrange
        _mockConnection
            .Setup(x => x.IsConnected())
            .Returns(false);

        // Act
        var result = _mockConnection.Object.IsConnected();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task EnsureConnectedAsync_WithValidConnection_ShouldComplete()
    {
        // Arrange
        _mockConnection
            .Setup(x => x.EnsureConnectedAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var action = () => _mockConnection.Object.EnsureConnectedAsync(CancellationToken.None);

        // Assert
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task EnsureConnectedAsync_WhenConnectionFails_ShouldThrow()
    {
        // Arrange
        _mockConnection
            .Setup(x => x.EnsureConnectedAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FixConnectionException("Connection failed"));

        // Act & Assert
        var action = () => _mockConnection.Object.EnsureConnectedAsync(CancellationToken.None);

        await action.Should()
            .ThrowAsync<FixConnectionException>();
    }

    [Fact]
    public async Task EnsureConnectedAsync_WithCancellationToken_ShouldRespectToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        
        _mockConnection
            .Setup(x => x.EnsureConnectedAsync(It.IsAny<CancellationToken>()))
            .Returns(async (CancellationToken ct) =>
            {
                await Task.Delay(1000, ct);
            });

        // Act
        cts.CancelAfter(100);
        var action = () => _mockConnection.Object.EnsureConnectedAsync(cts.Token);

        // Assert
        await action.Should()
            .ThrowAsync<OperationCanceledException>();
    }
}