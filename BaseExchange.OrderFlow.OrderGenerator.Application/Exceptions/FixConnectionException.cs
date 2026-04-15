namespace BaseExchange.OrderFlow.OrderGenerator.Application.Exceptions;

public class FixConnectionException : AppException
{
    public FixConnectionException(
        string message = "FIX connection not established",
        Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
