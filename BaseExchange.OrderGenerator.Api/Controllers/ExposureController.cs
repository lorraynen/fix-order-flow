using BaseExchange.OrderFlow.Api.Client;
using BaseExchange.OrderFlow.OrderGenerator.Application.DTOs;
using BaseExchange.OrderFlow.OrderGenerator.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace BaseExchange.OrderGenerator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ExposureController : ControllerBase
{
    private readonly ILogger<OrdersController> _logger;
    private readonly ExposureApiClient _exposureClient;

    public ExposureController(
        ILogger<OrdersController> logger,
         ExposureApiClient exposureClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _exposureClient = exposureClient ?? throw new ArgumentNullException(nameof(exposureClient));
    }

   
    [HttpGet]
    public async Task<IActionResult> GetExposure()
    {
        var result = await _exposureClient.GetExposure();
        return Ok(result);
    }
}

