namespace BaseExchange.OrderFlow.OrderGenerator.Application.Interfaces
{
    public interface IFixConnection
    {
        bool IsConnected();
        Task EnsureConnectedAsync(CancellationToken cancellationToken);
    }
}
