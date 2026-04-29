using BaseExchange.OrderFlow.Domain.Entities;
using BaseExchange.OrderFlow.OrderGenerator.Application.Common;
using BaseExchange.OrderFlow.OrderGenerator.Application.DTOs;
using BaseExchange.OrderFlow.OrderGenerator.Application.Exceptions;
using BaseExchange.OrderFlow.OrderGenerator.Application.Interfaces;
using BaseExchange.OrderFlow.OrderGenerator.Application.Services;
using BaseExchange.OrderFlow.OrderGenerator.Application.Validators;
using BaseExchange.OrderFlow.Tests.TestConfiguration;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BaseExchange.OrderFlow.Tests.Unit.Application.Services;

/// <summary>
/// Testes para IOrderService com Result Pattern
/// Implementa SOLID: Usa TestFactory para abstrair dependências
/// </summary>
public class OrderServiceTests
{
    #region Happy Path Tests

    [Fact]
    public async Task CreateOrderAsync_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var mockConnection = TestFactory.CreateMockFixConnection();
        var mockSender = TestFactory.CreateMockFixOrderSender();
        var mockLogger = TestFactory.CreateMockLogger<OrderService>();
        var validator = new CreateOrderValidator();

        var service = new OrderService(
            mockSender.Object,
            mockConnection.Object,
            validator,
            mockLogger.Object);

        var request = TestFactory.CreateOrderRequest();

        // Act
        var result = await service.CreateOrderAsync(request);

        // Assert
        result.Should().BeOfType<Result<OrderResponseDto>.Success>();
        
        var successResult = result as Result<OrderResponseDto>.Success;
        successResult!.Data.Should().NotBeNull();
        successResult.Data.Symbol.Should().Be(request.Symbol);
        successResult.Data.Status.Should().Be("sent");

        mockConnection.Verify(x => x.EnsureConnectedAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockSender.Verify(x => x.SendOrder(It.IsAny<Order>()), Times.Once);
    }

    #endregion

    #region Validation Error Tests

    [Fact]
    public async Task CreateOrderAsync_WithInvalidSymbol_ShouldReturnFailure()
    {
        // Arrange
        var mockConnection = TestFactory.CreateMockFixConnection();
        var mockSender = TestFactory.CreateMockFixOrderSender();
        var mockLogger = TestFactory.CreateMockLogger<OrderService>();
        var validator = new CreateOrderValidator();

        var service = new OrderService(
            mockSender.Object,
            mockConnection.Object,
            validator,
            mockLogger.Object);

        var request = TestFactory.CreateOrderRequest(symbol: "INVALID");

        // Act
        var result = await service.CreateOrderAsync(request);

        // Assert
        result.Should().BeOfType<Result<OrderResponseDto>.Failure>();
        
        var failureResult = result as Result<OrderResponseDto>.Failure;
        failureResult!.Errors.Should().NotBeEmpty();
        failureResult.Errors.First().Should().Contain("Symbol");

        mockConnection.Verify(x => x.EnsureConnectedAsync(It.IsAny<CancellationToken>()), Times.Never);
        mockSender.Verify(x => x.SendOrder(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrderAsync_WithZeroQuantity_ShouldReturnFailure()
    {
        // Arrange
        var mockConnection = TestFactory.CreateMockFixConnection();
        var mockSender = TestFactory.CreateMockFixOrderSender();
        var mockLogger = TestFactory.CreateMockLogger<OrderService>();
        var validator = new CreateOrderValidator();

        var service = new OrderService(
            mockSender.Object,
            mockConnection.Object,
            validator,
            mockLogger.Object);

        var request = TestFactory.CreateOrderRequest(quantity: 0);

        // Act
        var result = await service.CreateOrderAsync(request);

        // Assert
        result.Should().BeOfType<Result<OrderResponseDto>.Failure>();
        
        var failureResult = result as Result<OrderResponseDto>.Failure;
        failureResult!.Errors.Should().NotBeEmpty();
        failureResult.Errors.First().Should().Contain("Quantity");
    }

    [Fact]
    public async Task CreateOrderAsync_WithNegativePrice_ShouldReturnFailure()
    {
        // Arrange
        var mockConnection = TestFactory.CreateMockFixConnection();
        var mockSender = TestFactory.CreateMockFixOrderSender();
        var mockLogger = TestFactory.CreateMockLogger<OrderService>();
        var validator = new CreateOrderValidator();

        var service = new OrderService(
            mockSender.Object,
            mockConnection.Object,
            validator,
            mockLogger.Object);

        var request = TestFactory.CreateOrderRequest(price: -10.50m);

        // Act
        var result = await service.CreateOrderAsync(request);

        // Assert
        result.Should().BeOfType<Result<OrderResponseDto>.Failure>();
    }

    #endregion

    #region FIX Connection Error Tests

    [Fact]
    public async Task CreateOrderAsync_WhenFixNotConnected_ShouldReturnFailure()
    {
        // Arrange
        var mockConnection = TestFactory.CreateMockFixConnection(shouldThrowOnEnsure: true);
        var mockSender = TestFactory.CreateMockFixOrderSender();
        var mockLogger = TestFactory.CreateMockLogger<OrderService>();
        var validator = new CreateOrderValidator();

        var service = new OrderService(
            mockSender.Object,
            mockConnection.Object,
            validator,
            mockLogger.Object);

        var request = TestFactory.CreateOrderRequest();

        // Act
        var result = await service.CreateOrderAsync(request);

        // Assert
        result.Should().BeOfType<Result<OrderResponseDto>.Failure>();
        mockSender.Verify(x => x.SendOrder(It.IsAny<Order>()), Times.Never);
    }

    #endregion

    #region FIX Send Error Tests

    [Fact]
    public async Task CreateOrderAsync_WhenFixSendFails_ShouldReturnFailure()
    {
        // Arrange
        var mockConnection = TestFactory.CreateMockFixConnection();
        var mockSender = TestFactory.CreateMockFixOrderSender(shouldThrow: true);
        var mockLogger = TestFactory.CreateMockLogger<OrderService>();
        var validator = new CreateOrderValidator();

        var service = new OrderService(
            mockSender.Object,
            mockConnection.Object,
            validator,
            mockLogger.Object);

        var request = TestFactory.CreateOrderRequest();

        // Act
        var result = await service.CreateOrderAsync(request);

        // Assert
        result.Should().BeOfType<Result<OrderResponseDto>.Failure>();
        
        var failureResult = result as Result<OrderResponseDto>.Failure;
        failureResult!.Errors.Should().NotBeEmpty();
    }

    #endregion

    #region Pattern Matching Tests

    [Fact]
    public async Task CreateOrderAsync_ResultPatternMatching_ShouldHandleSuccessCorrectly()
    {
        // Arrange
        var mockConnection = TestFactory.CreateMockFixConnection();
        var mockSender = TestFactory.CreateMockFixOrderSender();
        var mockLogger = TestFactory.CreateMockLogger<OrderService>();
        var validator = new CreateOrderValidator();

        var service = new OrderService(
            mockSender.Object,
            mockConnection.Object,
            validator,
            mockLogger.Object);

        var request = TestFactory.CreateOrderRequest();

        // Act
        var result = await service.CreateOrderAsync(request);

        var matched = result.Match(
            onSuccess: success => success.Data.Symbol,
            onFailure: failure => "FAILED"
        );

        // Assert
        matched.Should().Be("PETR4");
    }

    [Fact]
    public async Task CreateOrderAsync_ResultPatternMatching_ShouldHandleFailureCorrectly()
    {
        // Arrange
        var mockConnection = TestFactory.CreateMockFixConnection();
        var mockSender = TestFactory.CreateMockFixOrderSender();
        var mockLogger = TestFactory.CreateMockLogger<OrderService>();
        var validator = new CreateOrderValidator();

        var service = new OrderService(
            mockSender.Object,
            mockConnection.Object,
            validator,
            mockLogger.Object);

        var request = TestFactory.CreateOrderRequest(symbol: "INVALID");

        // Act
        var result = await service.CreateOrderAsync(request);

        var matched = result.Match(
            onSuccess: success => "SUCCESS",
            onFailure: failure => string.Join(",", failure.Errors)
        );

        // Assert
        matched.Should().Contain("Symbol");
    }

    #endregion
}