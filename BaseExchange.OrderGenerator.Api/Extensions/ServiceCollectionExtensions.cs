using BaseExchange.OrderFlow.Infrastructure.Fix;
using BaseExchange.OrderFlow.Infrastructure.Fix.HealthChecks;
using BaseExchange.OrderFlow.OrderGenerator.Application.Interfaces;
using BaseExchange.OrderFlow.OrderGenerator.Application.Services;
using BaseExchange.OrderFlow.OrderGenerator.Application.Validators;
using BaseExchange.OrderFlow.Api.Client;
using BaseExchange.OrderFlow.Infrastructure.Resilience;
using BaseExchange.OrderFlow.OrderAccumulator.Application.Services;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;

namespace BaseExchange.OrderGenerator.Api.Extensions;

/// <summary>
/// Extension methods para IServiceCollection
/// Implementa SOLID: Centraliza registro de serviços
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddHealthChecks();

        services.AddSwaggerGen(config =>
        {
            config.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Order Generator API",
                Version = "v1",
                Description = "REST API for FIX order generation"
            });
        });

        return services;
    }

    public static IServiceCollection AddFixServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. Configuração do FIX
        var configFile = configuration["FixClient:ConfigFile"] ?? "fix.cfg";
        services.AddSingleton(new FixSettingsOptions
        {
            ConfigPath = Path.Combine(AppContext.BaseDirectory, configFile)
        });

        // 2. FixApplication - necessário para FixClient
        services.AddSingleton<FixApplication>();

        // 3. Cliente FIX (coordenador interno)
        services.AddSingleton<FixClient>();

        // 4. Serviços FIX
        services.AddSingleton<IFixConnection, FixConnectionImpl>();
        services.AddSingleton<IFixOrderSender, FixOrderSender>();

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IExposureService, ExposureService>();

        return services;
    }

    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining(typeof(CreateOrderValidator));
        return services;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        return services;
    }
}
