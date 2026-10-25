using ICT.PaymentGateway.Domain.Common;

namespace ICT.PaymentGateway.Domain.Subscriptions;

/// <summary>One paid membership period for an account. Access is derived from the dates, so no status job is needed.</summary>
public sealed class Subscription
{
    private Subscription()
    {
    }

    public Guid Id { get; private set; }

    public Guid MerchantId { get; private set; }

    public SubscriptionPlan Plan { get; private set; }

    /// <summary>The captured payment that bought this period.</summary>
    public Guid PaymentId { get; private set; }

    public DateTimeOffset StartsAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    /// <summary>
    /// Starts a new period after a successful payment. Renewing the same plan keeps the time left on the
    /// current period; choosing a different plan starts a fresh period from now.
    /// </summary>
    public static Subscription Activate(
        Guid merchantId,
        SubscriptionPlan plan,
        Guid paymentId,
        TimeSpan duration,
        DateTimeOffset now,
        Subscription? current)
    {
        if (merchantId == Guid.Empty)
        {
            throw new DomainException("MerchantId is required.");
        }

        if (!Enum.IsDefined(plan))
        {
            throw new DomainException("Unknown subscription plan.");
        }

        if (duration <= TimeSpan.Zero)
        {
            throw new DomainException("Subscription duration must be positive.");
        }

        var carryOver = current is not null && current.Plan == plan && current.IsActiveAt(now)
            ? current.ExpiresAt - now
            : TimeSpan.Zero;

        return new Subscription
        {
            Id = Guid.NewGuid(),
            MerchantId = merchantId,
            Plan = plan,
            PaymentId = paymentId,
            StartsAt = now,
            ExpiresAt = now + carryOver + duration,
        };
    }

    public bool IsActiveAt(DateTimeOffset now) => now >= StartsAt && now < ExpiresAt;

    /// <summary>True when the subscription is active and its tier is at least <paramref name="required"/>.</summary>
    public bool Grants(SubscriptionPlan required, DateTimeOffset now) => IsActiveAt(now) && Plan >= required;
}
