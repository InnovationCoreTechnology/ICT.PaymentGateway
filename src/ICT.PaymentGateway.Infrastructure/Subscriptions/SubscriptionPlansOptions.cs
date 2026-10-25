using ICT.PaymentGateway.Domain.Subscriptions;

namespace ICT.PaymentGateway.Infrastructure.Subscriptions;

/// <summary>Plan prices and features, bound from the Subscriptions section of the configuration.</summary>
public sealed class SubscriptionPlansOptions
{
    public const string SectionName = "Subscriptions";

    public List<PlanOptions> Plans { get; init; } = [];
}

public sealed class PlanOptions
{
    public SubscriptionPlan Plan { get; init; }

    public decimal Price { get; init; }

    public string Currency { get; init; } = string.Empty;

    public int DurationDays { get; init; } = 30;

    public Dictionary<string, string> Features { get; init; } = [];
}
