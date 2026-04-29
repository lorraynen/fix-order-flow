using BaseExchange.OrderFlow.Domain.Enums;
using BaseExchange.OrderFlow.OrderGenerator.Application.DTOs;
using FluentValidation;

namespace BaseExchange.OrderFlow.OrderGenerator.Application.Validators;

/// <summary>
/// Validator para CreateOrderRequestDTO
/// Implementa regras de validação de negócio
/// </summary>
public class CreateOrderValidator : AbstractValidator<CreateOrderRequestDTO>
{
    // Símbolos permitidos (deve estar sincronizado com Order.cs)
    private static readonly string[] AllowedSymbols = { "PETR4", "VALE3", "VIIA4" };

    public CreateOrderValidator()
    {
        // Validar Symbol
        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("Symbol is required")
            .MaximumLength(10).WithMessage("Symbol must not exceed 10 characters")
            .Must(BeValidSymbol).WithMessage("Symbol must be one of: PETR4, VALE3, VIIA4");

        // Validar Quantity
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero")
            .LessThanOrEqualTo(99999).WithMessage("Quantity must not exceed 99999");

        // Validar Price
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero")
            .LessThan(1000).WithMessage("Price must be less than 1000")
            .Custom((price, context) =>
            {
                // Price deve ser múltiplo de 0.01
                if (price % 0.01m != 0)
                {
                    context.AddFailure(
                        context.PropertyName,
                        "Price must be multiple of 0.01");
                }
            });

        // Validar Side
        RuleFor(x => x.Side)
            .IsInEnum().WithMessage("Side must be a valid enum value");
    }

    /// <summary>
    /// Valida se o símbolo está na lista de permitidos
    /// </summary>
    private bool BeValidSymbol(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            return false;

        return AllowedSymbols.Contains(symbol.ToUpperInvariant());
    }
}