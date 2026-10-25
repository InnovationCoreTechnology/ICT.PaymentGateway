namespace ICT.PaymentGateway.API.Contracts;

public static class ApiHeaders
{
    /// <summary>Temporary merchant identification until merchant authentication is built (roadmap 4.1).</summary>
    public const string MerchantId = "X-Merchant-Id";

    public const string IdempotencyKey = "Idempotency-Key";
}
