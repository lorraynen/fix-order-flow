using BaseExchange.OrderFlow.Domain.Enums;

namespace BaseExchange.OrderFlow.OrderGenerator.Application.DTOs;

public record CreateOrderRequestDTO(
    string Symbol,
    SideEnum Side,
    int Quantity,
    decimal Price);
