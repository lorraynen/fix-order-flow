using BaseExchange.OrderFlow.Domain.Entities;
using BaseExchange.OrderFlow.Domain.Enums;
using BaseExchange.OrderFlow.OrderGenerator.Application.Exceptions;
using Microsoft.Extensions.Logging;
using QuickFix;
using QuickFix.Fields;
using QuickFix.Logger;
using QuickFix.Store;
using QuickFix.Transport;

namespace BaseExchange.OrderFlow.Infrastructure.Fix;

/// <summary>
/// Coordena a conexão e envio de ordens FIX
/// Não implementa interfaces - é uma classe interna de infraestrutura
/// Usada por FixConnection e FixOrderSender
/// </summary>
public class FixClient
{
    private readonly ILogger<FixClient> _logger;
    private readonly FixApplication _app;
    private readonly FixSettingsOptions _options;

    private SocketInitiator? _initiator;
    private bool _started = false;

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

    /// <summary>
    /// Verifica se há uma sessão FIX ativa
    /// </summary>
    public bool IsConnected()
    {
        return _app.CurrentSession != null;
    }

    /// <summary>
    /// Envia uma ordem ao servidor FIX
    /// Método interno - não é público para exterior
    /// </summary>
    public void SendOrder(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

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
            "Order sent {Symbol} {Side} {Quantity}@{Price}",
            order.Symbol, order.Side, order.Quantity, order.Price);
    }

    /// <summary>
    /// Inicia a conexão FIX
    /// </summary>
    private void Start()
    {
        if (_started)
            return;

        try
        {
            _logger.LogInformation("Starting FIX client...");

            var path = _options.ConfigPath;

            if (!File.Exists(path))
            {
                _logger.LogError("FIX config file not found: {Path}", path);
                throw new FileNotFoundException("fix.cfg not found", path);
            }

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

    /// <summary>
    /// Para a conexão FIX
    /// </summary>
    public void Stop()
    {
        try
        {
            _initiator?.Stop();
            _started = false;
            _logger.LogInformation("FIX client stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping FIX client");
        }
    }

    /// <summary>
    /// Implementa IDisposable para limpeza de recursos
    /// </summary>
    public void Dispose()
    {
        Stop();
        _initiator?.Dispose();
    }
}
