using ICT.PaymentGateway.Domain.Subscriptions;

namespace ICT.PaymentGateway.Application.Subscriptions;

public sealed record PlanDto(
    SubscriptionPlan Plan,
    decimal Price,
    string Currency,
    int DurationDays,
    IReadOnlyDictionary<string, string> Features)
{
    public static PlanDto From(PlanDefinition definition) =>
        new(definition.Plan, definition.Price.Amount, definition.Price.Currency, definition.DurationDays, definition.Features);
}
