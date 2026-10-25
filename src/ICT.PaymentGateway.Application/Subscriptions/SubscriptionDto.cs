using ICT.PaymentGateway.Domain.Subscriptions;

namespace ICT.PaymentGateway.Application.Subscriptions;

public sealed record SubscriptionDto(
    Guid Id,
    SubscriptionPlan Plan,
    DateTimeOffset StartsAt,
    DateTimeOffset ExpiresAt,
    Guid PaymentId,
    IReadOnlyDictionary<string, string> Features)
{
    public static SubscriptionDto From(Subscription subscription, ISubscriptionPlanCatalog catalog) =>
        new(
            subscription.Id,
            subscription.Plan,
            subscription.StartsAt,
            subscription.ExpiresAt,
            subscription.PaymentId,
            catalog.Find(subscription.Plan)?.Features ?? new Dictionary<string, string>());
}
