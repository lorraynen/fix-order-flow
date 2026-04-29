using BaseExchange.OrderFlow.Domain.Entities;
using BaseExchange.OrderFlow.Domain.Enums;
using BaseExchange.OrderFlow.OrderGenerator.Application.DTOs;
using BaseExchange.OrderFlow.OrderGenerator.Application.Interfaces;
using BaseExchange.OrderFlow.OrderGenerator.Application.Services;
using BaseExchange.OrderFlow.OrderAccumulator.Application.Services;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace BaseExchange.OrderFlow.Tests.TestConfiguration;

/// <summary>
/// Factory Pattern para criar objetos de teste
/// Implementa SOLID: Centraliza criação, facilita manutenção
/// </summary>
public class TestFactory
{
    /// <summary>
    /// Cria uma instância de Order para testes
    /// Depende de abstrações (interfaces), não de detalhes
    /// </summary>
    public static Order CreateOrder(
        string symbol = "PETR4",
        SideEnum side = SideEnum.Buy,
        int quantity = 100,
        decimal price = 25.50m)
    {
        return new Order(symbol, side, quantity, price);
    }

    /// <summary>
    /// Cria um mock de IFixConnection para testes
    /// </summary>
    public static Mock<IFixConnection> CreateMockFixConnection(
        bool isConnected = true,
        bool shouldThrowOnEnsure = false)
    {
        var mock = new Mock<IFixConnection>(MockBehavior.Strict);

        mock.Setup(x => x.IsConnected())
            .Returns(isConnected);

        if (shouldThrowOnEnsure)
        {
            mock.Setup(x => x.EnsureConnectedAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Connection failed"));
        }
        else
        {
            mock.Setup(x => x.EnsureConnectedAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        return mock;
    }

    /// <summary>
    /// Cria um mock de IFixOrderSender para testes
    /// </summary>
    public static Mock<IFixOrderSender> CreateMockFixOrderSender(
        bool shouldThrow = false,
        string? errorMessage = null)
    {
        var mock = new Mock<IFixOrderSender>(MockBehavior.Strict);

        if (shouldThrow)
        {
            mock.Setup(x => x.SendOrder(It.IsAny<Order>()))
                .Throws(new Exception(errorMessage ?? "Send failed"));
        }
        else
        {
            mock.Setup(x => x.SendOrder(It.IsAny<Order>()));
        }

        return mock;
    }

    /// <summary>
    /// Cria um mock de ILogger para testes
    /// </summary>
    public static Mock<ILogger<T>> CreateMockLogger<T>() where T : class
    {
        return new Mock<ILogger<T>>();
    }

    /// <summary>
    /// Cria um CreateOrderRequestDTO
    /// </summary>
    public static CreateOrderRequestDTO CreateOrderRequest(
        string symbol = "PETR4",
        SideEnum side = SideEnum.Buy,
        int quantity = 100,
        decimal price = 25.50m)
    {
        return new CreateOrderRequestDTO(symbol, side, quantity, price);
    }

    /// <summary>
    /// Cria instância de IExposureService
    /// </summary>
    public static IExposureService CreateExposureService()
    {
        return new ExposureService();
    }
}