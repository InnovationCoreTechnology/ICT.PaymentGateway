namespace ICT.PaymentGateway.Domain.Payments;

public enum PaymentStatus
{
    Created = 0,
    Pending = 1,
    Authorized = 2,
    Captured = 3,
    Failed = 4,
    Cancelled = 5,
}
