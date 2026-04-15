
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuickFix;
using QuickFix.Logger;
using QuickFix.Store;

namespace BaseExchange.OrderFlow.OrderAccumulator.Worker.Fix;
public class FixAcceptorService : BackgroundService
{
    private readonly FixApplication _fixApp;
    private readonly ILogger<FixAcceptorService> _logger;

    private ThreadedSocketAcceptor? _acceptor;
    private readonly FixSettingsOptions _options;

    public FixAcceptorService(
        FixApplication fixApp,
        FixSettingsOptions options,
        ILogger<FixAcceptorService> logger)
    {
        _fixApp = fixApp ?? throw new ArgumentNullException(nameof(fixApp));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Starting FIX Acceptor...");

            var path = _options.ConfigPath;

            var settings = new SessionSettings(path);
            var storeFactory = new FileStoreFactory(settings);
            var logFactory = new FileLogFactory(settings);

            _acceptor = new ThreadedSocketAcceptor(
                _fixApp,
                storeFactory,
                settings,
                logFactory);

            _acceptor.Start();

            _logger.LogInformation("FIX Acceptor started successfully");

            // Mantém o serviço vivo até cancelamento
            stoppingToken.Register(() =>
            {
                _logger.LogInformation("Shutdown signal received");
                _acceptor?.Stop();
            });

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting FIX Acceptor");
            throw;
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping FIX Acceptor...");

        _acceptor?.Stop();

        return base.StopAsync(cancellationToken);
    }
}
