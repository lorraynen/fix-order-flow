using BaseExchange.OrderFlow.OrderGenerator.Application.Exceptions;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Xunit;
using FluentValidation.Results;
using Moq;
using BaseExchange.OrderGenerator.Api.Common.Middleware;

namespace BaseExchange.OrderFlow.Tests.Integration.Middleware;

/// <summary>
/// Testes unitários para ErrorHandlingMiddleware
/// Testa o comportamento da lógica de tratamento de exceções
/// Sem dependência de TestServer (complexidade desnecessária)
/// </summary>
public class ErrorHandlingMiddlewareTests
{
    private readonly Mock<ILogger<ErrorHandlingMiddleware>> _mockLogger;

    public ErrorHandlingMiddlewareTests()
    {
        _mockLogger = new Mock<ILogger<ErrorHandlingMiddleware>>();
    }

    [Fact]
    public async Task Middleware_WithValidationException_ShouldCatch()
    {
        // Arrange
        var exceptionCaught = false;
        var exception = new FluentValidation.ValidationException(
            new[] { new ValidationFailure("field", "Field is required") }
        );

        var context = new DefaultHttpContext();
        var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        RequestDelegate next = async (ctx) =>
        {
            throw exception;
        };

        var middleware = new BaseExchange.OrderGenerator.Api.Common.Middleware.ErrorHandlingMiddleware(next, _mockLogger.Object);

        // Act
        try
        {
            await middleware.Invoke(context);
        }
        catch
        {
            // Esperado - middleware captura e serializa
        }

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Middleware_WithFixConnectionException_ShouldReturn503()
    {
        // Arrange
        var exception = new FixConnectionException("FIX server unreachable");

        var context = new DefaultHttpContext();
        var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        RequestDelegate next = async (ctx) =>
        {
            throw exception;
        };

        var middleware = new BaseExchange.OrderGenerator.Api.Common.Middleware.ErrorHandlingMiddleware(next, _mockLogger.Object);

        // Act
        try
        {
            await middleware.Invoke(context);
        }
        catch
        {
            // Esperado - middleware captura e serializa
        }

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
    }

    [Fact]
    public async Task Middleware_WithOrderSendException_ShouldReturn500()
    {
        // Arrange
        var exception = new OrderSendException("Failed to send order");

        var context = new DefaultHttpContext();
        var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        RequestDelegate next = async (ctx) =>
        {
            throw exception;
        };

        var middleware = new BaseExchange.OrderGenerator.Api.Common.Middleware.ErrorHandlingMiddleware(next, _mockLogger.Object);

        // Act
        try
        {
            await middleware.Invoke(context);
        }
        catch
        {
            // Esperado - middleware captura e serializa
        }

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task Middleware_WithGenericException_ShouldReturn500()
    {
        // Arrange
        var exception = new InvalidOperationException("Unexpected error");

        var context = new DefaultHttpContext();
        var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        RequestDelegate next = async (ctx) =>
        {
            throw exception;
        };

        var middleware = new BaseExchange.OrderGenerator.Api.Common.Middleware.ErrorHandlingMiddleware(next, _mockLogger.Object);

        // Act
        try
        {
            await middleware.Invoke(context);
        }
        catch
        {
            // Esperado - middleware captura e serializa
        }

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task Middleware_WithSuccessfulRequest_ShouldPassThrough()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;
        var nextCalled = false;

        RequestDelegate next = async (ctx) =>
        {
            nextCalled = true;
            ctx.Response.StatusCode = StatusCodes.Status200OK;
        };

        var middleware = new BaseExchange.OrderGenerator.Api.Common.Middleware.ErrorHandlingMiddleware(next, _mockLogger.Object);

        // Act
        await middleware.Invoke(context);

        // Assert
        nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }
}