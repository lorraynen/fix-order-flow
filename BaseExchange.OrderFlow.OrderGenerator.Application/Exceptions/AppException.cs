namespace BaseExchange.OrderFlow.OrderGenerator.Application.Exceptions;

public class AppException : Exception
{
    public IReadOnlyCollection<string> Errors { get; }

    public AppException(
        string message,
        Exception? innerException = null,
        IEnumerable<string>? errors = null)
        : base(message, innerException)
    {
        Errors = errors?.ToList() ?? new List<string>();
    }
}
