using Microsoft.Extensions.Logging;
using QuickFix;

namespace BaseExchange.OrderFlow.Infrastructure.Fix;

public class FixApplication : IApplication
{
    private readonly ILogger<FixApplication> _logger;

    public SessionID? CurrentSession { get; private set; }

    public FixApplication(ILogger<FixApplication> logger)
    {
        _logger = logger;
    }

    public void OnCreate(SessionID sessionID) { }

    public void OnLogon(SessionID sessionID)
    {
        CurrentSession = sessionID;
        _logger.LogInformation("FIX logon: {Session}", sessionID);
    }

    public void OnLogout(SessionID sessionID)
    {
        _logger.LogWarning("FIX logout: {Session}", sessionID);
        CurrentSession = null;
    }

    public void ToAdmin(Message message, SessionID sessionID)
    {
        _logger.LogDebug("FIX ToAdmin: {Message}", message);
    }

    public void FromAdmin(Message message, SessionID sessionID) { }

    public void ToApp(Message message, SessionID sessionID) { }

    public void FromApp(Message message, SessionID sessionID)
    {
        _logger.LogInformation("FIX message received: {MsgType}",
            message.Header.GetString(35));
    }
}