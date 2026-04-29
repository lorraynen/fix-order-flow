using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace BaseExchange.OrderFlow.Infrastructure.Resilience;

/// <summary>
/// Define políticas de resiliência para operações FIX
/// Implementa padrões: Retry, Circuit Breaker, Timeout
/// </summary>
public static class ResiliencePolicy
{
    private const int MaxRetries = 3;
    private const int InitialDelayMs = 100;

    /// <summary>
    /// Política de retry com backoff exponencial
    /// 1ª tentativa: 100ms
    /// 2ª tentativa: 200ms
    /// 3ª tentativa: 400ms
    /// </summary>
    public static IAsyncPolicy<T> GetExponentialRetryPolicy<T>() =>
        Policy.TimeoutAsync<T>(TimeSpan.FromSeconds(10))
            .WrapAsync(
                Policy<T>
                    .Handle<HttpRequestException>()
                    .OrResult(r => r == null)
                    .WaitAndRetryAsync(
                        retryCount: MaxRetries,
                        sleepDurationProvider: retryAttempt =>
                            TimeSpan.FromMilliseconds(InitialDelayMs * Math.Pow(2, retryAttempt - 1))
                    )
            );

    /// <summary>
    /// Política de Circuit Breaker
    /// Abre o circuito após 3 falhas consecutivas
    /// Mantém aberto por 30 segundos
    /// </summary>
    public static IAsyncPolicy<T> GetCircuitBreakerPolicy<T>() =>
        Policy<T>
            .Handle<HttpRequestException>()
            .OrResult(r => r == null)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, timespan, context) =>
                {
                    // Log será adicionado via middleware
                },
                onReset: context =>
                {
                    // Log será adicionado via middleware
                }
            );

    /// <summary>
    /// Política combinada: Retry + Circuit Breaker + Timeout
    /// Tenta com backoff exponencial, depois abre circuito se falhar
    /// </summary>
    public static IAsyncPolicy<T> GetCombinedPolicy<T>() =>
        Policy.WrapAsync(
            GetExponentialRetryPolicy<T>(),
            GetCircuitBreakerPolicy<T>()
        );

    /// <summary>
    /// Política de retry simples para operações rápidas
    /// </summary>
    public static IAsyncPolicy GetSimpleRetryPolicy() =>
        Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: MaxRetries,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromMilliseconds(InitialDelayMs * Math.Pow(2, retryAttempt - 1))
            );
}