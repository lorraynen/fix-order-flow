using QuickFix;
using QuickFix.Fields;
using BaseExchange.OrderFlow.Domain.Enums;
using BaseExchange.OrderFlow.Domain.Entities;
using BaseExchange.OrderFlow.OrderAccumulator.Application.Services;
using Microsoft.Extensions.Logging;

namespace BaseExchange.OrderFlow.OrderAccumulator.Worker.Fix;
public class FixApplication : IApplication
{
    private readonly ExposureService _exposureService;
    private readonly ILogger<FixApplication> _logger;

    public FixApplication(
        ExposureService exposureService,
        ILogger<FixApplication> logger)
    {
        _exposureService = exposureService;
        _logger = logger;
    }

    public void OnCreate(SessionID sessionID) { }

    public void OnLogon(SessionID sessionID)
    {
        _logger.LogInformation("FIX connected: {Session}", sessionID);
    }

    public void OnLogout(SessionID sessionID)
    {
        _logger.LogWarning("FIX logout");
    }

    public void ToAdmin(Message message, SessionID sessionID) { }

    public void FromAdmin(Message message, SessionID sessionID) { }

    public void ToApp(Message message, SessionID sessionID) { }

    public void FromApp(Message message, SessionID sessionID)
    {
        try
        {
            if (!message.Header.IsSetField(Tags.MsgType))
                return;

            var msgType = message.Header.GetString(Tags.MsgType);

            if (msgType != MsgType.ORDER_SINGLE)
                return;

            var order = MapToOrder(message);

            _exposureService.ProcessOrder(order);

            var exposure = _exposureService.GetExposure(order.Symbol);

            _logger.LogInformation(
                "Exposure updated: {Symbol} = {Exposure}",
                order.Symbol,
                exposure);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing FIX message");
        }
    }

    private static Order MapToOrder(Message message)
    {
        var symbol = message.GetString(Tags.Symbol);
        var side = message.GetChar(Tags.Side);
        var quantity = message.GetInt(Tags.OrderQty);
        var price = message.GetDecimal(Tags.Price);

        return new Order(
            symbol,
            side == Side.BUY ? SideEnum.Buy : SideEnum.Sell,
            quantity,
            price
        );
    }
}