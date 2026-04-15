using BaseExchange.OrderFlow.Infrastructure.Fix;
using BaseExchange.OrderFlow.Infrastructure.Fix.HealthChecks;
using BaseExchange.OrderFlow.OrderGenerator.Application.Interfaces;
using BaseExchange.OrderFlow.OrderGenerator.Application.Services;
using BaseExchange.OrderFlow.OrderGenerator.Application.Validators;
using BaseExchange.OrderFlow.Api.Client;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;

namespace BaseExchange.OrderGenerator.Api.Extensions;

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
                Description = "API for sending orders via FIX"
            });

            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            if (File.Exists(xmlPath))
            {
                config.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }

    public static IServiceCollection AddFixServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<FixApplication>();

        var configFile = configuration["FixClient:ConfigFile"] ?? "fix.cfg";

        services.AddSingleton(new FixSettingsOptions
        {
            ConfigPath = Path.Combine(AppContext.BaseDirectory, configFile)
        });

        // Single instance shared across both contracts (sender + connection state).
        services.AddSingleton<FixClient>();
        services.AddSingleton<IFixOrderSender>(sp => sp.GetRequiredService<FixClient>());
        services.AddSingleton<IFixConnection>(sp => sp.GetRequiredService<FixClient>());
        services.AddHttpClient<ExposureApiClient>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:5000");
        });

        services.AddHealthChecks().AddCheck<FixClientHealthCheck>("fix");

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IOrderService, OrderService>();
        return services;
    }

    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateOrderValidator>();
        return services;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        return services;
    }
}
