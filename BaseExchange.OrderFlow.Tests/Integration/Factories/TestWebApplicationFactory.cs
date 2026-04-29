using BaseExchange.OrderFlow.Domain.Entities;
using BaseExchange.OrderFlow.OrderGenerator.Application.Interfaces;
using BaseExchange.OrderGenerator.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace BaseExchange.OrderFlow.Tests.Integration.Factories;

/// <summary>
/// Factory para criar WebApplication com mocks para testes de integração
/// Implementa SOLID: Substitui dependências reais por mocks
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly Mock<IFixConnection>? _mockConnection;
    private readonly Mock<IFixOrderSender>? _mockSender;

    public TestWebApplicationFactory(
        Mock<IFixConnection>? mockConnection = null,
        Mock<IFixOrderSender>? mockSender = null)
    {
        _mockConnection = mockConnection;
        _mockSender = mockSender;
    }

    /// <summary>
    /// Configura o WebHost para testes, substituindo serviços reais por mocks
    /// </summary>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove o registro de IFixOrderSender real
            var fixOrderSenderDescriptor = services.FirstOrDefault(
                d => d.ServiceType == typeof(IFixOrderSender));
            
            if (fixOrderSenderDescriptor != null)
            {
                services.Remove(fixOrderSenderDescriptor);
            }

            // Remove o registro de IFixConnection real
            var fixConnectionDescriptor = services.FirstOrDefault(
                d => d.ServiceType == typeof(IFixConnection));
            
            if (fixConnectionDescriptor != null)
            {
                services.Remove(fixConnectionDescriptor);
            }

            // Registra mocks
            var mockConnection = _mockConnection ?? CreateDefaultMockConnection();
            var mockSender = _mockSender ?? CreateDefaultMockSender();

            services.AddSingleton(mockConnection.Object);
            services.AddSingleton(mockSender.Object);
        });

        base.ConfigureWebHost(builder);
    }

    /// <summary>
    /// Cria mock padrão para conexão (conectado com sucesso)
    /// </summary>
    private static Mock<IFixConnection> CreateDefaultMockConnection()
    {
        var mock = new Mock<IFixConnection>(MockBehavior.Strict);
        
        mock.Setup(x => x.IsConnected())
            .Returns(true);
        
        mock.Setup(x => x.EnsureConnectedAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return mock;
    }

    /// <summary>
    /// Cria mock padrão para envio de ordens (sucesso)
    /// </summary>
    private static Mock<IFixOrderSender> CreateDefaultMockSender()
    {
        var mock = new Mock<IFixOrderSender>(MockBehavior.Strict);
        
        mock.Setup(x => x.SendOrder(It.IsAny<Order>()));

        return mock;
    }

    /// <summary>
    /// Factory method para cenário de sucesso (padrão)
    /// </summary>
    public static TestWebApplicationFactory CreateWithSuccessfulFix()
    {
        return new TestWebApplicationFactory(
            CreateDefaultMockConnection(),
            CreateDefaultMockSender());
    }

    /// <summary>
    /// Factory method para cenário de FIX desconectado
    /// </summary>
    public static TestWebApplicationFactory CreateWithDisconnectedFix()
    {
        var mockConnection = new Mock<IFixConnection>(MockBehavior.Strict);
        mockConnection.Setup(x => x.IsConnected()).Returns(false);
        mockConnection.Setup(x => x.EnsureConnectedAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("FIX not connected"));

        var mockSender = new Mock<IFixOrderSender>(MockBehavior.Strict);
        
        return new TestWebApplicationFactory(mockConnection, mockSender);
    }

    /// <summary>
    /// Factory method para cenário de erro no envio
    /// </summary>
    public static TestWebApplicationFactory CreateWithSendFailure()
    {
        var mockConnection = new Mock<IFixConnection>(MockBehavior.Strict);
        mockConnection.Setup(x => x.IsConnected()).Returns(true);
        mockConnection.Setup(x => x.EnsureConnectedAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var mockSender = new Mock<IFixOrderSender>(MockBehavior.Strict);
        mockSender.Setup(x => x.SendOrder(It.IsAny<Order>()))
            .Throws(new Exception("FIX send failed"));

        return new TestWebApplicationFactory(mockConnection, mockSender);
    }
}