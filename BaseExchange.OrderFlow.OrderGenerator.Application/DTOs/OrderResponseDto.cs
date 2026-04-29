using BaseExchange.OrderFlow.Domain.Enums;

namespace BaseExchange.OrderFlow.OrderGenerator.Application.DTOs;

public record OrderResponseDto(
    string Symbol,
    SideEnum? Side,
    int Quantity,
    decimal Price,
    string Status,
    DateTime CreatedAt
);