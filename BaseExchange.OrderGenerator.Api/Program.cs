using BaseExchange.OrderFlow.OrderGenerator.Application.Interfaces;
using BaseExchange.OrderFlow.OrderGenerator.Application.Validators;
using BaseExchange.OrderGenerator.Api.Common.Middleware;
using BaseExchange.OrderGenerator.Api.Extensions;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace BaseExchange.OrderGenerator.Api;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigureLogging(builder);

            RegisterServices(builder.Services, builder.Configuration);

            var app = builder.Build();

            ConfigureRequestPipeline(app);

            InitializeServices(app);

            Log.Information("Application starting");

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static void ConfigureLogging(WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "OrderGeneratorAPI")
            .WriteTo.Console()
            .WriteTo.File(
                "logs/log-.txt",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .MinimumLevel.Debug()
            .CreateLogger();

        builder.Host.UseSerilog();
    }

    private static void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddApiServices();
        services.AddFixServices(configuration);
        services.AddApplicationServices();
        services.AddValidators();
        services.AddCorsPolicy();
    }

    private static void ConfigureRequestPipeline(WebApplication app)
    {
        app.UseMiddleware<ErrorHandlingMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Generator API v1");
                c.RoutePrefix = string.Empty;
            });
        }

        app.UseHttpsRedirection();

        //app.UseCors("AllowAll");

        app.UseAuthorization();

        app.MapControllers();
        app.MapHealthChecks("/health");
    }

    private static void InitializeServices(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            scope.ServiceProvider.GetRequiredService<IFixOrderSender>();
            logger.LogInformation("FIX client initialized");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize FIX client");
        }
    }
}
