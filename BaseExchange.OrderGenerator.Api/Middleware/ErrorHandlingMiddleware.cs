using BaseExchange.OrderFlow.OrderGenerator.Application.Exceptions;
using BaseExchange.OrderGenerator.Api.Response;

namespace BaseExchange.OrderGenerator.Api.Common.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error occurred");
            await HandleException(context, 400, ex.Message, ex.Errors);
        }
        catch (FixConnectionException ex)
        {
            _logger.LogWarning(ex, "FIX connection error occurred");
            await HandleException(context, 503, ex.Message, new[] { ex.Message });
        }
        catch (OrderSendException ex)
        {
            _logger.LogError(ex, "Order send error");

            await HandleException(context, 500, ex.Message, ex.Errors);
        }
        catch (AppException ex)
        {
            _logger.LogWarning(ex, "Application error occurred");
            await HandleException(context, 400, ex.Message, ex.Errors);
        }
        catch (ArgumentException ex)
        {
            // Domain guards currently throw ArgumentException; treat as 400 instead of 500.
            _logger.LogWarning(ex, "Bad request");
            await HandleException(context, 400, ex.Message, new[] { ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleException(context, 500, "Internal server error", new[] { ex.Message });
        }
    }

    private static async Task HandleException(
        HttpContext context,
        int statusCode,
        string message,
        IEnumerable<string> errors)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = ApiResponse<object>.ErrorResponse(message, errors);
        await context.Response.WriteAsJsonAsync(response);
    }
}
