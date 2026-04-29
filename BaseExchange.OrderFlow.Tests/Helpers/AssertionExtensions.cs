using BaseExchange.OrderFlow.OrderGenerator.Application.DTOs;
using FluentAssertions;
using FluentAssertions.Primitives;

namespace BaseExchange.OrderFlow.Tests.Helpers;

/// <summary>
/// Extensões de assertion customizadas
/// Reduz código duplicado em testes
/// </summary>
public static class AssertionExtensions
{
    public static void ShouldBeValidOrder(this OrderResponseDto order)
    {
        order.Should().NotBeNull();
        order.Symbol.Should().NotBeNullOrWhiteSpace();
        order.Side.Should().NotBeNull();
        order.Quantity.Should().BeGreaterThan(0);
        order.Price.Should().BeGreaterThan(0);
        order.Status.Should().NotBeNullOrWhiteSpace();
        order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
    }

    public static void ShouldHaveError(this FluentValidation.Results.ValidationResult result, string propertyName)
    {
        result.Errors.Should().ContainSingle(e => e.PropertyName == propertyName);
    }

    public static void ShouldHaveErrors(this FluentValidation.Results.ValidationResult result, params string[] propertyNames)
    {
        var errorProperties = result.Errors.Select(e => e.PropertyName).Distinct();
        errorProperties.Should().Contain(propertyNames);
    }
}