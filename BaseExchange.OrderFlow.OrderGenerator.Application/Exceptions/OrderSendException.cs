namespace BaseExchange.OrderFlow.OrderGenerator.Application.Exceptions;

public class OrderSendException : AppException
{
    public OrderSendException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}