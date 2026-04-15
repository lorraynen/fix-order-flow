using BaseExchange.OrderFlow.OrderGenerator.Application.DTOs;
using FluentValidation;

namespace BaseExchange.OrderFlow.OrderGenerator.Application.Validators;

public class CreateOrderValidator : AbstractValidator<CreateOrderRequestDTO>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("Symbol is required")
            .MaximumLength(10).WithMessage("Symbol must not exceed 10 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero");
    }
}