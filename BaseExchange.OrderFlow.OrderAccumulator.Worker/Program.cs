using BaseExchange.OrderFlow.OrderAccumulator.Application.Services;
using BaseExchange.OrderFlow.OrderAccumulator.Worker.Fix;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddSingleton<ExposureService>();
builder.Services.AddSingleton<FixApplication>();

builder.Services.AddSingleton(new FixSettingsOptions
{
    ConfigPath = Path.Combine(AppContext.BaseDirectory, "Fix", "fix.cfg")
});

// Worker
builder.Services.AddHostedService<FixAcceptorService>();

var app = builder.Build();

// Endpoint
app.MapGet("/exposure", (ExposureService service) =>
{
    return service.GetAll()
        .Select(x => new
        {
            Symbol = x.Key,
            Exposure = x.Value
        });
});

app.Run();