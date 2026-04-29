namespace BaseExchange.OrderFlow.OrderGenerator.Application.Exceptions;

/// <summary>
/// Exception thrown when order sending fails via FIX protocol
/// </summary>
public class OrderSendException : AppException
{
    /// <summary>
    /// Initializes a new instance of OrderSendException
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="innerException">Inner exception that caused the error</param>
    /// <param name="errors">Optional collection of error details</param>
    public OrderSendException(
        string message,
        Exception? innerException = null,
        IEnumerable<string>? errors = null)
        : base(message, innerException, errors)
    {
    }

    /// <summary>
    /// Creates a new OrderSendException with a specific message
    /// </summary>
    public static OrderSendException FromException(string symbol, Exception ex)
    {
        var message = $"Failed to send order for symbol {symbol}";
        return new OrderSendException(message, ex);
    }

    /// <summary>
    /// Creates a new OrderSendException without inner exception
    /// </summary>
    public static OrderSendException Simple(string message)
    {
        return new OrderSendException(message);
    }
}