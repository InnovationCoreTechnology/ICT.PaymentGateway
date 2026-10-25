using ICT.PaymentGateway.Application.Subscriptions.Commands.PurchaseSubscription;
using ICT.PaymentGateway.Domain.Subscriptions;

namespace ICT.PaymentGateway.Application.UnitTests;

public class PurchaseSubscriptionCommandValidatorTests
{
    private readonly PurchaseSubscriptionCommandValidator _validator = new();

    private static PurchaseSubscriptionCommand Command(
        SubscriptionPlan plan = SubscriptionPlan.Basic,
        string token = "tok_abc",
        string key = "key-1") =>
        new(Guid.NewGuid(), key, plan, token);

    [Fact]
    public void Validate_WithValidCommand_Passes() =>
        _validator.Validate(Command()).IsValid.ShouldBeTrue();

    [Fact]
    public void Validate_WithUnknownPlan_Fails() =>
        _validator.Validate(Command(plan: (SubscriptionPlan)0)).IsValid.ShouldBeFalse();

    [Fact]
    public void Validate_WhenTokenLooksLikeCardNumber_Fails() =>
        _validator.Validate(Command(token: "4111 1111 1111 1111")).IsValid.ShouldBeFalse();

    [Fact]
    public void Validate_WithBlankKey_Fails() =>
        _validator.Validate(Command(key: " ")).IsValid.ShouldBeFalse();
}
