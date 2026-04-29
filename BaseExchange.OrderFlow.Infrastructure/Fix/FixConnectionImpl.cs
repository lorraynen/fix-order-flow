using BaseExchange.OrderFlow.OrderGenerator.Application.Exceptions;
using BaseExchange.OrderFlow.OrderGenerator.Application.Interfaces;
using BaseExchange.OrderFlow.Infrastructure.Resilience;
using Microsoft.Extensions.Logging;
using Polly;

namespace BaseExchange.OrderFlow.Infrastructure.Fix;

/// <summary>
/// Implementa IFixConnection com políticas de resiliência (Polly)
/// Separa do namespace para evitar conflito de nomes
/// </summary>
public class FixConnectionImpl : IFixConnection
{
    private readonly FixClient _fixClient;
    private readonly ILogger<FixConnectionImpl> _logger;
    private readonly IAsyncPolicy _retryPolicy;
    private const int MaxRetries = 10;
    private const int RetryDelayMs = 500;

    public FixConnectionImpl(FixClient fixClient, ILogger<FixConnectionImpl> logger)
    {
        _fixClient = fixClient ?? throw new ArgumentNullException(nameof(fixClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _retryPolicy = ResiliencePolicy.GetSimpleRetryPolicy();
    }

    /// <summary>
    /// Verifica se está conectado ao FIX
    /// </summary>
    public bool IsConnected()
    {
        return _fixClient.IsConnected();
    }

    /// <summary>
    /// Aguarda conexão com retry automático usando Polly
    /// Implementa exponential backoff e circuit breaker
    /// </summary>
    public async Task EnsureConnectedAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _retryPolicy.ExecuteAsync(async (ct) =>
            {
                int retries = 0;

                while (!IsConnected() && retries < MaxRetries)
                {
                    _logger.LogWarning(
                        "Waiting FIX connection... attempt {Retry}/{MaxRetries}",
                        retries + 1,
                        MaxRetries);

                    await Task.Delay(RetryDelayMs, ct);
                    retries++;
                }

                if (!IsConnected())
                {
                    var timeoutMs = MaxRetries * RetryDelayMs;
                    _logger.LogError(
                        "FIX connection not established after {Timeout}ms",
                        timeoutMs);

                    throw new FixConnectionException(
                        $"FIX not connected after {timeoutMs}ms");
                }

                _logger.LogInformation("FIX connection established");
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("FIX connection attempt was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FIX connection failed after retries");
            throw;
        }
    }
}