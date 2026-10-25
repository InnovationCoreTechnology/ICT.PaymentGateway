using ICT.PaymentGateway.Application.Subscriptions;
using ICT.PaymentGateway.Domain.Payments;
using ICT.PaymentGateway.Domain.Subscriptions;
using Microsoft.Extensions.Options;

namespace ICT.PaymentGateway.Infrastructure.Subscriptions;

internal sealed class SubscriptionPlanCatalog : ISubscriptionPlanCatalog
{
    private readonly Dictionary<SubscriptionPlan, PlanDefinition> _plans;

    public SubscriptionPlanCatalog(IOptions<SubscriptionPlansOptions> options)
    {
        _plans = options.Value.Plans.ToDictionary(
            p => p.Plan,
            p => new PlanDefinition(p.Plan, Money.Create(p.Price, p.Currency), p.DurationDays, p.Features));
    }

    public IReadOnlyList<PlanDefinition> GetAll() => [.. _plans.Values];

    public PlanDefinition? Find(SubscriptionPlan plan) => _plans.GetValueOrDefault(plan);

    /// <summary>Returns an error message when the configured plans are unusable, otherwise null. Checked at start-up.</summary>
    public static string? Validate(SubscriptionPlansOptions options)
    {
        foreach (var plan in Enum.GetValues<SubscriptionPlan>())
        {
            var configured = options.Plans.Where(p => p.Plan == plan).ToList();
            if (configured.Count != 1)
            {
                return $"Subscriptions:Plans must contain exactly one entry for {plan}.";
            }

            var entry = configured[0];
            if (entry.Price <= 0 || decimal.Round(entry.Price, 4) != entry.Price)
            {
                return $"Subscriptions:Plans {plan} must have a positive price with at most 4 decimals.";
            }

            if (entry.DurationDays <= 0)
            {
                return $"Subscriptions:Plans {plan} must have DurationDays greater than zero.";
            }

            if (string.IsNullOrWhiteSpace(entry.Currency) || entry.Currency.Trim().Length != 3)
            {
                return $"Subscriptions:Plans {plan} must have a 3-letter currency code.";
            }
        }

        return null;
    }
}
