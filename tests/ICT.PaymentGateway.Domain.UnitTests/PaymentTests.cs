using ICT.PaymentGateway.Domain.Common;
using ICT.PaymentGateway.Domain.Payments;

namespace ICT.PaymentGateway.Domain.UnitTests;

public class PaymentTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static Payment NewPayment() =>
        Payment.Create(Guid.NewGuid(), Money.Create(25m, "EUR"), "tok_123", "order 1", Now);

    [Fact]
    public void Create_StartsInCreatedStatus()
    {
        var payment = NewPayment();

        payment.Status.ShouldBe(PaymentStatus.Created);
        payment.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void Create_WithBlankToken_Throws() =>
        Should.Throw<DomainException>(() =>
            Payment.Create(Guid.NewGuid(), Money.Create(1m, "USD"), " ", null, Now));

    [Fact]
    public void HappyPath_MovesThroughToCaptured()
    {
        var payment = NewPayment();

        payment.MarkPending(Now);
        payment.Authorize("psp_1", Now);
        payment.Capture(Now);

        payment.Status.ShouldBe(PaymentStatus.Captured);
        payment.PspReference.ShouldBe("psp_1");
    }

    [Fact]
    public void Capture_FromCreated_Throws() =>
        Should.Throw<DomainException>(() => NewPayment().Capture(Now));

    [Fact]
    public void Fail_FromPending_RecordsReason()
    {
        var payment = NewPayment();
        payment.MarkPending(Now);

        payment.Fail("declined", Now);

        payment.Status.ShouldBe(PaymentStatus.Failed);
        payment.FailureReason.ShouldBe("declined");
    }

    [Fact]
    public void Cancel_AfterCapture_Throws()
    {
        var payment = NewPayment();
        payment.MarkPending(Now);
        payment.Authorize("psp_1", Now);
        payment.Capture(Now);

        Should.Throw<DomainException>(() => payment.Cancel(Now));
    }
}
