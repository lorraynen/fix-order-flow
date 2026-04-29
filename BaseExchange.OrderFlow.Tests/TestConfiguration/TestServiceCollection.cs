using BaseExchange.OrderFlow.OrderGenerator.Application.Interfaces;
using BaseExchange.OrderFlow.OrderGenerator.Application.Services;
using BaseExchange.OrderFlow.OrderGenerator.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace BaseExchange.OrderFlow.Tests.TestConfiguration;

/// <summary>
/// Configuração de DI para testes
/// Permite registrar mocks e serviços reais conforme necessário
/// </summary>
public static class TestServiceCollection
{
    public static IServiceCollection AddTestServices(
        this IServiceCollection services,
        TestServiceOptions? options = null)
    {
        options ??= new TestServiceOptions();

        // Register validators
        services.AddValidatorsFromAssemblyContaining<CreateOrderValidator>();

        // Register mocks or real implementations
        if (options.UseRealFixConnection && options.UseRealFixOrderSender)
        {
            // For integration tests with real FIX
            // This would be configured separately
        }
        else
        {
            // Register mocks for unit tests
            var mockConnection = new Mock<IFixConnection>();
            var mockSender = new Mock<IFixOrderSender>();

            services.AddSingleton(mockConnection.Object);
            services.AddSingleton(mockSender.Object);
        }

        // Register application services
        services.AddScoped<IOrderService, OrderService>();

        // Add logging
        services.AddLogging(config =>
        {
            config.AddConsole();
        });

        return services;
    }
}

/// <summary>
/// Options for test service configuration
/// </summary>
public class TestServiceOptions
{
    public bool UseRealFixConnection { get; set; } = false;
    public bool UseRealFixOrderSender { get; set; } = false;
}