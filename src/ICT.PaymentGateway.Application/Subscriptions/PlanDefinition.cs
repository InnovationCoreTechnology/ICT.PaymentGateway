using ICT.PaymentGateway.Domain.Payments;
using ICT.PaymentGateway.Domain.Subscriptions;

namespace ICT.PaymentGateway.Application.Subscriptions;

/// <summary>What a plan costs, how long one payment lasts, and what it unlocks.</summary>
public sealed record PlanDefinition(
    SubscriptionPlan Plan,
    Money Price,
    int DurationDays,
    IReadOnlyDictionary<string, string> Features);
