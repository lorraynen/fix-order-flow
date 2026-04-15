namespace BaseExchange.OrderFlow.OrderGenerator.Application.Exceptions;

public class ValidationException : AppException
{
    public ValidationException(
       string message = "Validation failed",
       Exception? innerException = null)
       : base(message, innerException)
    {
    }

}
