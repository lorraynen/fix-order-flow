using BaseExchange.OrderFlow.Domain.Entities;

namespace BaseExchange.OrderFlow.OrderGenerator.Application.Interfaces;

public interface IFixOrderSender
{
    void SendOrder(Order order);
}