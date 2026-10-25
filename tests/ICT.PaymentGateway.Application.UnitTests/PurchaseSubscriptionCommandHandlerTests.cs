using ICT.PaymentGateway.Application.Common;
using ICT.PaymentGateway.Application.Common.Results;
using ICT.PaymentGateway.Application.Payments;
using ICT.PaymentGateway.Application.Subscriptions;
using ICT.PaymentGateway.Application.Subscriptions.Commands.PurchaseSubscription;
using ICT.PaymentGateway.Domain.Payments;
using ICT.PaymentGateway.Domain.Subscriptions;
using NSubstitute;

namespace ICT.PaymentGateway.Application.UnitTests;

public class PurchaseSubscriptionCommandHandlerTests
{
    private static readonly Guid MerchantId = Guid.NewGuid();

    private readonly ISubscriptionPlanCatalog _catalog = Substitute.For<ISubscriptionPlanCatalog>();
    private readonly ISubscriptionRepository _subscriptions = Substitute.For<ISubscriptionRepository>();
    private readonly IPaymentRepository _payments = Substitute.For<IPaymentRepository>();
    private readonly IIdempotencyStore _idempotency = Substitute.For<IIdempotencyStore>();
    private readonly IPspGateway _psp = Substitute.For<IPspGateway>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly PurchaseSubscriptionCommandHandler _handler;

    public PurchaseSubscriptionCommandHandlerTests()
    {
        foreach (var (plan, price) in new[] { (SubscriptionPlan.Basic, 9m), (SubscriptionPlan.Classic, 19m), (SubscriptionPlan.Gold, 39m) })
        {
            _catalog.Find(plan).Returns(new PlanDefinition(
                plan,
                Money.Create(price, "USD"),
                30,
                new Dictionary<string, string> { ["Support"] = plan.ToString() }));
        }

        _handler = new PurchaseSubscriptionCommandHandler(
            _catalog, _subscriptions, _payments, _idempotency, _psp, _unitOfWork, TimeProvider.System);
    }

    private static PurchaseSubscriptionCommand Command(SubscriptionPlan plan = SubscriptionPlan.Gold, string token = "tok_abc") =>
        new(MerchantId, "key-1", plan, token);

    private void PspApproves() =>
        _psp.AuthorizeAsync(Arg.Any<PspAuthorizationRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PspAuthorizationResult(true, "psp_1", null));

    [Fact]
    public async Task Handle_WhenPaymentSucceeds_UnlocksAccountWithPlan()
    {
        PspApproves();

        var result = await _handler.HandleAsync(Command(SubscriptionPlan.Gold), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Plan.ShouldBe(SubscriptionPlan.Gold);
        result.Value.ExpiresAt.ShouldBeGreaterThan(DateTimeOffset.UtcNow.AddDays(29));
        await _subscriptions.Received(1).AddAsync(Arg.Any<Subscription>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ChargesThePlanPriceAndCapturesThePayment()
    {
        PspApproves();
        Payment? saved = null;
        await _payments.AddAsync(Arg.Do<Payment>(p => saved = p), Arg.Any<CancellationToken>());

        await _handler.HandleAsync(Command(SubscriptionPlan.Classic), CancellationToken.None);

        saved.ShouldNotBeNull();
        saved.Amount.Amount.ShouldBe(19m);
        saved.Status.ShouldBe(PaymentStatus.Captured);
    }

    [Fact]
    public async Task Handle_WhenPaymentDeclined_ReturnsPaymentRequiredAndCreatesNoSubscription()
    {
        _psp.AuthorizeAsync(Arg.Any<PspAuthorizationRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PspAuthorizationResult(false, null, "Card declined."));

        var result = await _handler.HandleAsync(Command(), CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error.Type.ShouldBe(ErrorType.PaymentRequired);
        result.Error.Message.ShouldBe("Card declined.");
        await _subscriptions.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRenewingActiveSamePlan_KeepsRemainingTime()
    {
        PspApproves();
        var current = Subscription.Activate(
            MerchantId, SubscriptionPlan.Gold, Guid.NewGuid(), TimeSpan.FromDays(30), DateTimeOffset.UtcNow.AddDays(-10), null);
        _subscriptions.GetActiveAsync(MerchantId, Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns(current);

        var result = await _handler.HandleAsync(Command(SubscriptionPlan.Gold), CancellationToken.None);

        result.Value.ExpiresAt.ShouldBeGreaterThan(DateTimeOffset.UtcNow.AddDays(49));
    }

    [Fact]
    public async Task Handle_WhenKeyReusedWithDifferentBody_ReturnsUnprocessableWithoutCharging()
    {
        _idempotency.FindAsync(MerchantId, "key-1", Arg.Any<CancellationToken>())
            .Returns(IdempotencyRecord.Create(MerchantId, "key-1", "DIFFERENT", Guid.NewGuid(), DateTimeOffset.UtcNow));

        var result = await _handler.HandleAsync(Command(), CancellationToken.None);

        result.Error.Type.ShouldBe(ErrorType.Unprocessable);
        await _psp.DidNotReceiveWithAnyArgs().AuthorizeAsync(default!, default);
    }

    [Fact]
    public async Task Handle_WhenKeyReusedWithSameBody_ReturnsOriginalSubscriptionWithoutCharging()
    {
        PspApproves();
        string? hash = null;
        await _idempotency.AddAsync(Arg.Do<IdempotencyRecord>(r => hash = r.RequestHash), Arg.Any<CancellationToken>());
        await _handler.HandleAsync(Command(), CancellationToken.None);

        var original = Subscription.Activate(
            MerchantId, SubscriptionPlan.Gold, Guid.NewGuid(), TimeSpan.FromDays(30), DateTimeOffset.UtcNow, null);
        _idempotency.FindAsync(MerchantId, "key-1", Arg.Any<CancellationToken>())
            .Returns(IdempotencyRecord.Create(MerchantId, "key-1", hash!, original.PaymentId, DateTimeOffset.UtcNow));
        _subscriptions.GetByPaymentIdAsync(original.PaymentId, Arg.Any<CancellationToken>()).Returns(original);
        _psp.ClearReceivedCalls();

        var result = await _handler.HandleAsync(Command(), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(original.Id);
        await _psp.DidNotReceiveWithAnyArgs().AuthorizeAsync(default!, default);
    }

    [Fact]
    public async Task Handle_WhenReplayingADeclinedPurchase_ReturnsPaymentRequiredAgain()
    {
        _psp.AuthorizeAsync(Arg.Any<PspAuthorizationRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PspAuthorizationResult(false, null, "Card declined."));
        IdempotencyRecord? record = null;
        await _idempotency.AddAsync(Arg.Do<IdempotencyRecord>(r => record = r), Arg.Any<CancellationToken>());
        Payment? declined = null;
        await _payments.AddAsync(Arg.Do<Payment>(p => declined = p), Arg.Any<CancellationToken>());
        await _handler.HandleAsync(Command(), CancellationToken.None);

        _idempotency.FindAsync(MerchantId, "key-1", Arg.Any<CancellationToken>()).Returns(record);
        _payments.GetByIdAsync(declined!.Id, MerchantId, Arg.Any<CancellationToken>()).Returns(declined);

        var result = await _handler.HandleAsync(Command(), CancellationToken.None);

        result.Error.Type.ShouldBe(ErrorType.PaymentRequired);
        result.Error.Message.ShouldBe("Card declined.");
    }
}
