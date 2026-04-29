using BaseExchange.OrderFlow.Domain.Entities;
using BaseExchange.OrderFlow.OrderGenerator.Application.Exceptions;
using BaseExchange.OrderFlow.OrderGenerator.Application.Interfaces;
using Moq;

namespace BaseExchange.OrderFlow.Tests.TestConfiguration;

/// <summary>
/// Builder para criar mocks customizados de IFixOrderSender
/// </summary>
public class MockFixOrderSenderBuilder
{
    private readonly Mock<IFixOrderSender> _mock;

    public MockFixOrderSenderBuilder()
    {
        _mock = new Mock<IFixOrderSender>(MockBehavior.Strict);
    }

    public MockFixOrderSenderBuilder WithSuccessfulSend()
    {
        _mock
            .Setup(x => x.SendOrder(It.IsAny<Order>()));

        return this;
    }

    public MockFixOrderSenderBuilder WithSendFailure(string reason = "Send failed")
    {
        _mock
            .Setup(x => x.SendOrder(It.IsAny<Order>()))
            .Throws(OrderSendException.Simple(reason));

        return this;
    }

    /// <summary>
    /// Configura o mock para simular falha com exceção interna
    /// </summary>
    public MockFixOrderSenderBuilder WithSendFailureWithInnerException(
        string symbol,
        Exception innerException)
    {
        _mock
            .Setup(x => x.SendOrder(It.IsAny<Order>()))
            .Throws(OrderSendException.FromException(symbol, innerException));

        return this;
    }

    /// <summary>
    /// Configura o mock para simular timeout
    /// </summary>
    public MockFixOrderSenderBuilder WithSendTimeout()
    {
        _mock
            .Setup(x => x.SendOrder(It.IsAny<Order>()))
            .Throws(OrderSendException.FromException(
                "UNKNOWN",
                new TimeoutException("Send timeout")));

        return this;
    }

    /// <summary>
    /// Configura o mock para lançar exception genérica
    /// </summary>
    public MockFixOrderSenderBuilder WithGenericException(string message = "Generic error")
    {
        _mock
            .Setup(x => x.SendOrder(It.IsAny<Order>()))
            .Throws(new Exception(message));

        return this;
    }

    /// <summary>
    /// Retorna a instância do mock
    /// </summary>
    public IFixOrderSender Build()
    {
        return _mock.Object;
    }

    /// <summary>
    /// Retorna o mock para verificações adicionais
    /// </summary>
    public Mock<IFixOrderSender> BuildMock()
    {
        return _mock;
    }
}