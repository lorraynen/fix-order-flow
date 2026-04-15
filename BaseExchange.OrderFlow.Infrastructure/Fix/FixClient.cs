using BaseExchange.OrderFlow.Domain.Entities;
using BaseExchange.OrderFlow.Domain.Enums;
using BaseExchange.OrderFlow.OrderGenerator.Application.Exceptions;
using BaseExchange.OrderFlow.OrderGenerator.Application.Interfaces;
using Microsoft.Extensions.Logging;
using QuickFix;
using QuickFix.Fields;
using QuickFix.Logger;
using QuickFix.Store;
using QuickFix.Transport;

namespace BaseExchange.OrderFlow.Infrastructure.Fix;

public class FixClient : IFixOrderSender, IFixConnection
{
    private readonly ILogger<FixClient> _logger;
    private readonly FixApplication _app;
    private readonly FixSettingsOptions _options;

    private SocketInitiator? _initiator;
    private bool _started = false;

    private const int MaxRetries = 10;
    private const int RetryDelayMs = 500;

    public FixClient(
        FixApplication app,
        FixSettingsOptions options,
        ILogger<FixClient> logger)
    {
        _app = app ?? throw new ArgumentNullException(nameof(app));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        Start();
    }

    public async Task EnsureConnectedAsync(CancellationToken cancellationToken)
    {
        int retries = 0;

        while (!IsConnected() && retries < MaxRetries)
        {
            _logger.LogWarning("Waiting FIX connection... attempt {Retry}", retries + 1);

            await Task.Delay(RetryDelayMs, cancellationToken);
            retries++;
        }

        if (!IsConnected())
        {
            throw new FixConnectionException("FIX not connected after retries");
        }
    }

    public void SendOrder(Order order)
    {
        if (_app.CurrentSession == null)
            throw new FixConnectionException("FIX not connected");

        var message = new QuickFix.FIX44.NewOrderSingle(
            new ClOrdID(Guid.NewGuid().ToString()),
            new Symbol(order.Symbol),
            new Side(order.Side == SideEnum.Buy ? Side.BUY : Side.SELL),
            new TransactTime(DateTime.UtcNow),
            new OrdType(OrdType.LIMIT)
        );

        message.Set(new OrderQty(order.Quantity));
        message.Set(new Price(order.Price));

        Session.SendToTarget(message, _app.CurrentSession);

        _logger.LogInformation(
            "Order sent {Symbol} {Side} {Quantity} @ {Price}",
            order.Symbol,
            order.Side,
            order.Quantity,
            order.Price);
    }

    public bool IsConnected()
    {
        return _app.CurrentSession != null;
    }

    private void Start()
    {
        if (_started)
            return;

        try
        {
            _logger.LogInformation("Starting FIX client...");

            var path = _options.ConfigPath;

            if (!File.Exists(path))
                throw new FileNotFoundException("fix.cfg not found", path);

            var settings = new SessionSettings(path);

            var storeFactory = new FileStoreFactory(settings);
            var logFactory = new ScreenLogFactory(settings);

            _initiator = new SocketInitiator(_app, storeFactory, settings, logFactory);

            _initiator.Start();

            _started = true;

            _logger.LogInformation("FIX client started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting FIX client");
            throw;
        }
    }
}
