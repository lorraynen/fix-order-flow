using BaseExchange.OrderFlow.OrderGenerator.Application.DTOs;
using BaseExchange.OrderFlow.OrderGenerator.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BaseExchange.OrderGenerator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IOrderService orderService,
        ILogger<OrdersController> logger)
    {
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Cria uma nova ordem
    /// </summary>
    /// <param name="request">Dados da ordem a ser criada</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resposta com status da ordem criada</returns>
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

        var result = await _orderService.CreateOrderAsync(request, cancellationToken);

        // ✅ Usar Result Pattern com pattern matching
        return result.Match(
            onSuccess: success => Ok(success.Data),
            onFailure: failure =>
            {
                // Determinar status code baseado na mensagem de erro
                var statusCode = DetermineStatusCode(failure.Errors);
                return StatusCode(statusCode, new
                {
                    errors = failure.Errors,
                    timestamp = DateTime.UtcNow
                });
            }
        );
    }

    /// <summary>
    /// Determina o status HTTP baseado na mensagem de erro
    /// </summary>
    private int DetermineStatusCode(IReadOnlyList<string> errors)
    {
        var errorMessage = string.Concat(errors).ToUpperInvariant();

        return errorMessage switch
        {
            var msg when msg.Contains("VALIDATION") => StatusCodes.Status400BadRequest,
            var msg when msg.Contains("SYMBOL") => StatusCodes.Status400BadRequest,
            var msg when msg.Contains("QUANTITY") => StatusCodes.Status400BadRequest,
            var msg when msg.Contains("PRICE") => StatusCodes.Status400BadRequest,
            var msg when msg.Contains("CONNECTION") => StatusCodes.Status503ServiceUnavailable,
            var msg when msg.Contains("FIX") => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}

