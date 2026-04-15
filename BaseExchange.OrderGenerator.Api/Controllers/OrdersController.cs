using BaseExchange.OrderFlow.Api.Client;
using BaseExchange.OrderFlow.OrderGenerator.Application.DTOs;
using BaseExchange.OrderFlow.OrderGenerator.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace BaseExchange.OrderGenerator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;
    private readonly ExposureApiClient _exposureClient;

    public OrdersController(
        IOrderService orderService,
        ILogger<OrdersController> logger,
         ExposureApiClient exposureClient)
    {
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _exposureClient = exposureClient ?? throw new ArgumentNullException(nameof(exposureClient));
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderRequestDTO request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "HTTP POST /orders {Symbol} {Side} {Quantity} @ {Price}",
            request.Symbol,
            request.Side,
            request.Quantity,
            request.Price);

        var response = await _orderService.CreateOrderAsync(request, cancellationToken);

        return Ok(response);
    }

}

