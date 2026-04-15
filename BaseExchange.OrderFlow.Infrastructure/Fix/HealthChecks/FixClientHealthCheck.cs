using BaseExchange.OrderFlow.OrderGenerator.Application.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace BaseExchange.OrderFlow.Infrastructure.Fix.HealthChecks;

public class FixClientHealthCheck : IHealthCheck
{
    private readonly IFixConnection _connection;
    private readonly ILogger<FixClientHealthCheck> _logger;

    public FixClientHealthCheck(
        IFixConnection connection,
        ILogger<FixClientHealthCheck> logger)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (_connection.IsConnected())
            {
                _logger.LogDebug("FIX health check OK");
                return Task.FromResult(
                    HealthCheckResult.Healthy("FIX connected"));
            }

            _logger.LogWarning("FIX health check failed: not connected");

            return Task.FromResult(
                HealthCheckResult.Unhealthy("FIX not connected"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FIX health check error");

            return Task.FromResult(
                HealthCheckResult.Unhealthy("FIX health check failed", ex));
        }
    }
}