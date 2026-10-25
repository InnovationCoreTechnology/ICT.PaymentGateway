using ICT.PaymentGateway.Domain.Common;

namespace ICT.PaymentGateway.Domain.Payments;

public sealed class Payment
{
    private Payment()
    {
        PaymentMethodToken = string.Empty;
        Amount = null!;
    }

    public Guid Id { get; private set; }

    public Guid MerchantId { get; private set; }

    public Money Amount { get; private set; }

    /// <summary>Opaque token from the PSP or vault. Never a card number.</summary>
    public string PaymentMethodToken { get; private set; }

    public string? Description { get; private set; }

    public PaymentStatus Status { get; private set; }

    public string? PspReference { get; private set; }

    public string? FailureReason { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static Payment Create(
        Guid merchantId,
        Money amount,
        string paymentMethodToken,
        string? description,
        DateTimeOffset now)
    {
        if (merchantId == Guid.Empty)
        {
            throw new DomainException("MerchantId is required.");
        }

        if (string.IsNullOrWhiteSpace(paymentMethodToken))
        {
            throw new DomainException("Payment method token is required.");
        }

        return new Payment
        {
            Id = Guid.NewGuid(),
            MerchantId = merchantId,
            Amount = amount,
            PaymentMethodToken = paymentMethodToken,
            Description = description,
            Status = PaymentStatus.Created,
            CreatedAt = now,
            UpdatedAt = now,
        };
    }

    public void MarkPending(DateTimeOffset now) =>
        Transition(PaymentStatus.Pending, [PaymentStatus.Created], now);

    public void Authorize(string pspReference, DateTimeOffset now)
    {
        Transition(PaymentStatus.Authorized, [PaymentStatus.Pending], now);
        PspReference = pspReference;
    }

    public void Capture(DateTimeOffset now) =>
        Transition(PaymentStatus.Captured, [PaymentStatus.Authorized], now);

    public void Fail(string reason, DateTimeOffset now)
    {
        Transition(PaymentStatus.Failed, [PaymentStatus.Created, PaymentStatus.Pending], now);
        FailureReason = reason;
    }

    public void Cancel(DateTimeOffset now) =>
        Transition(PaymentStatus.Cancelled, [PaymentStatus.Created, PaymentStatus.Pending, PaymentStatus.Authorized], now);

    private void Transition(PaymentStatus target, PaymentStatus[] allowedFrom, DateTimeOffset now)
    {
        if (!allowedFrom.Contains(Status))
        {
            throw new DomainException($"Cannot move a payment from {Status} to {target}.");
        }

        Status = target;
        UpdatedAt = now;
    }
}
