namespace ICT.PaymentGateway.Domain.Payments;

/// <summary>Remembers which payment an Idempotency-Key produced, and the hash of the request that used it.</summary>
public sealed class IdempotencyRecord
{
    private IdempotencyRecord()
    {
        Key = string.Empty;
        RequestHash = string.Empty;
    }

    public Guid Id { get; private set; }

    public Guid MerchantId { get; private set; }

    public string Key { get; private set; }

    public string RequestHash { get; private set; }

    public Guid PaymentId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static IdempotencyRecord Create(Guid merchantId, string key, string requestHash, Guid paymentId, DateTimeOffset now) =>
        new()
        {
            Id = Guid.NewGuid(),
            MerchantId = merchantId,
            Key = key,
            RequestHash = requestHash,
            PaymentId = paymentId,
            CreatedAt = now,
        };
}
