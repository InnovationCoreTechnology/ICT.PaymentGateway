using ICT.PaymentGateway.Application.Common;
using ICT.PaymentGateway.Application.Common.Results;
using ICT.PaymentGateway.Application.Payments;
using ICT.PaymentGateway.Application.Payments.Commands.CreatePayment;
using ICT.PaymentGateway.Domain.Payments;
using NSubstitute;

namespace ICT.PaymentGateway.Application.UnitTests;

public class CreatePaymentCommandHandlerTests
{
    private static readonly Guid MerchantId = Guid.NewGuid();

    private readonly IPaymentRepository _payments = Substitute.For<IPaymentRepository>();
    private readonly IIdempotencyStore _idempotency = Substitute.For<IIdempotencyStore>();
    private readonly IPspGateway _psp = Substitute.For<IPspGateway>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreatePaymentCommandHandler _handler;

    public CreatePaymentCommandHandlerTests()
    {
        _handler = new CreatePaymentCommandHandler(_payments, _idempotency, _psp, _unitOfWork, TimeProvider.System);
    }

    private static CreatePaymentCommand Command(decimal amount = 10m, string key = "key-1") =>
        new(MerchantId, key, amount, "USD", "tok_abc", "order");

    [Fact]
    public async Task Handle_WhenPspApproves_ReturnsAuthorizedPaymentAndSaves()
    {
        _psp.AuthorizeAsync(Arg.Any<PspAuthorizationRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PspAuthorizationResult(true, "psp_1", null));

        var result = await _handler.HandleAsync(Command(), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Status.ShouldBe("Authorized");
        result.Value.PspReference.ShouldBe("psp_1");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _idempotency.Received(1).AddAsync(Arg.Any<IdempotencyRecord>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPspDeclines_ReturnsFailedPayment()
    {
        _psp.AuthorizeAsync(Arg.Any<PspAuthorizationRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PspAuthorizationResult(false, null, "Card declined."));

        var result = await _handler.HandleAsync(Command(), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Status.ShouldBe("Failed");
        result.Value.FailureReason.ShouldBe("Card declined.");
    }

    [Fact]
    public async Task Handle_WhenAmountInvalid_ReturnsValidationErrorWithoutCallingPsp()
    {
        var result = await _handler.HandleAsync(Command(amount: 1.00001m), CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error.Type.ShouldBe(ErrorType.Validation);
        await _psp.DidNotReceiveWithAnyArgs().AuthorizeAsync(default!, default);
    }

    [Fact]
    public async Task Handle_WhenKeyReusedWithSameBody_ReturnsOriginalPaymentWithoutCallingPsp()
    {
        var original = Payment.Create(MerchantId, Money.Create(10m, "USD"), "tok_abc", "order", DateTimeOffset.UtcNow);
        string? capturedHash = null;
        _psp.AuthorizeAsync(Arg.Any<PspAuthorizationRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PspAuthorizationResult(true, "psp_1", null));
        await _idempotency.AddAsync(
            Arg.Do<IdempotencyRecord>(r => capturedHash = r.RequestHash),
            Arg.Any<CancellationToken>());
        await _handler.HandleAsync(Command(), CancellationToken.None);

        _idempotency.FindAsync(MerchantId, "key-1", Arg.Any<CancellationToken>())
            .Returns(IdempotencyRecord.Create(MerchantId, "key-1", capturedHash!, original.Id, DateTimeOffset.UtcNow));
        _payments.GetByIdAsync(original.Id, MerchantId, Arg.Any<CancellationToken>()).Returns(original);
        _psp.ClearReceivedCalls();

        var result = await _handler.HandleAsync(Command(), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(original.Id);
        await _psp.DidNotReceiveWithAnyArgs().AuthorizeAsync(default!, default);
    }

    [Fact]
    public async Task Handle_WhenKeyReusedWithDifferentBody_ReturnsUnprocessable()
    {
        _idempotency.FindAsync(MerchantId, "key-1", Arg.Any<CancellationToken>())
            .Returns(IdempotencyRecord.Create(MerchantId, "key-1", "DIFFERENT", Guid.NewGuid(), DateTimeOffset.UtcNow));

        var result = await _handler.HandleAsync(Command(), CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error.Type.ShouldBe(ErrorType.Unprocessable);
    }
}
