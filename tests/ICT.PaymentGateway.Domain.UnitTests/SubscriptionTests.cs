using ICT.PaymentGateway.Domain.Common;
using ICT.PaymentGateway.Domain.Subscriptions;

namespace ICT.PaymentGateway.Domain.UnitTests;

public class SubscriptionTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan Month = TimeSpan.FromDays(30);
    private static readonly Guid Merchant = Guid.NewGuid();

    private static Subscription Activate(SubscriptionPlan plan, DateTimeOffset now, Subscription? current = null) =>
        Subscription.Activate(Merchant, plan, Guid.NewGuid(), Month, now, current);

    [Fact]
    public void Activate_WithNoCurrentSubscription_StartsNowForOneTerm()
    {
        var subscription = Activate(SubscriptionPlan.Gold, Now);

        subscription.StartsAt.ShouldBe(Now);
        subscription.ExpiresAt.ShouldBe(Now + Month);
    }

    [Fact]
    public void Activate_RenewingSamePlanWhileActive_KeepsRemainingTime()
    {
        var first = Activate(SubscriptionPlan.Classic, Now);
        var tenDaysLater = Now.AddDays(10);

        var renewed = Activate(SubscriptionPlan.Classic, tenDaysLater, first);

        renewed.ExpiresAt.ShouldBe(Now + Month + Month);
    }

    [Fact]
    public void Activate_ChangingPlan_StartsFreshTermFromNow()
    {
        var first = Activate(SubscriptionPlan.Basic, Now);
        var tenDaysLater = Now.AddDays(10);

        var upgraded = Activate(SubscriptionPlan.Gold, tenDaysLater, first);

        upgraded.ExpiresAt.ShouldBe(tenDaysLater + Month);
    }

    [Fact]
    public void Activate_RenewingAfterExpiry_StartsFreshTermFromNow()
    {
        var first = Activate(SubscriptionPlan.Basic, Now);
        var later = Now.AddDays(45);

        var renewed = Activate(SubscriptionPlan.Basic, later, first);

        renewed.ExpiresAt.ShouldBe(later + Month);
    }

    [Fact]
    public void IsActiveAt_IsFalseOnceExpired()
    {
        var subscription = Activate(SubscriptionPlan.Basic, Now);

        subscription.IsActiveAt(Now.AddDays(29)).ShouldBeTrue();
        subscription.IsActiveAt(Now + Month).ShouldBeFalse();
    }

    [Theory]
    [InlineData(SubscriptionPlan.Basic, SubscriptionPlan.Basic, true)]
    [InlineData(SubscriptionPlan.Basic, SubscriptionPlan.Classic, false)]
    [InlineData(SubscriptionPlan.Classic, SubscriptionPlan.Basic, true)]
    [InlineData(SubscriptionPlan.Gold, SubscriptionPlan.Classic, true)]
    [InlineData(SubscriptionPlan.Classic, SubscriptionPlan.Gold, false)]
    public void Grants_ComparesTiers(SubscriptionPlan owned, SubscriptionPlan required, bool expected) =>
        Activate(owned, Now).Grants(required, Now.AddDays(1)).ShouldBe(expected);

    [Fact]
    public void Grants_IsFalseAfterExpiryEvenForGold() =>
        Activate(SubscriptionPlan.Gold, Now).Grants(SubscriptionPlan.Basic, Now + Month).ShouldBeFalse();

    [Fact]
    public void Activate_WithNonPositiveDuration_Throws() =>
        Should.Throw<DomainException>(() =>
            Subscription.Activate(Merchant, SubscriptionPlan.Basic, Guid.NewGuid(), TimeSpan.Zero, Now, null));

    [Fact]
    public void Activate_WithEmptyMerchant_Throws() =>
        Should.Throw<DomainException>(() =>
            Subscription.Activate(Guid.Empty, SubscriptionPlan.Basic, Guid.NewGuid(), Month, Now, null));
}
