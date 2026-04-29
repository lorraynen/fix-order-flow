using BaseExchange.OrderFlow.OrderGenerator.Application.Exceptions;
using BaseExchange.OrderGenerator.Api.Response;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace BaseExchange.OrderGenerator.Api.Common.Middleware;

/// <summary>
/// Middleware global de tratamento de erros
/// Implementa padrão de centralização de tratamento de exceções
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processa a requisição e trata exceções
    /// </summary>
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (FluentValidation.ValidationException ex)
        {
            // ✅ Usar namespace completo para evitar ambiguidade
            _logger.LogWarning(ex, "Validation error occurred");
            var errors = ex.Errors.Select(e => e.ErrorMessage);
            await HandleException(context, 400, "Validation failed", errors, _logger);
        }
        catch (FixConnectionException ex)
        {
            _logger.LogWarning(ex, "FIX connection error occurred");
            await HandleException(context, 503, ex.Message, ex.Errors, _logger);
        }
        catch (OrderSendException ex)
        {
            _logger.LogError(ex, "Order send error");
            await HandleException(context, 500, ex.Message, ex.Errors, _logger);
        }
        catch (AppException ex)
        {
            _logger.LogWarning(ex, "Application error occurred");
            await HandleException(context, 400, ex.Message, ex.Errors, _logger);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {ErrorMessage}", ex.Message);
            await HandleException(context, 500, "Internal server error", new[] { ex.Message }, _logger);
        }
    }

    private static async Task HandleException(
        HttpContext context,
        int statusCode,
        string message,
        IEnumerable<string> errors,
        ILogger<ErrorHandlingMiddleware> logger)
    {
        try
        {
            if (context.Response.HasStarted)
            {
                logger.LogWarning("Response has already started, cannot modify status code");
                return;
            }

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var response = ApiResponse<object>.ErrorResponse(message, errors);
            await context.Response.WriteAsJsonAsync(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error writing error response");
        }
    }
}
