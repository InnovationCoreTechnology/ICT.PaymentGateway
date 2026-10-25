using ICT.PaymentGateway.Domain.Payments;

namespace ICT.PaymentGateway.Application.Payments;

public sealed record PaymentDto(
    Guid Id,
    decimal Amount,
    string Currency,
    string Status,
    string? Description,
    string? PspReference,
    string? FailureReason,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static PaymentDto From(Payment payment) =>
        new(
            payment.Id,
            payment.Amount.Amount,
            payment.Amount.Currency,
            payment.Status.ToString(),
            payment.Description,
            payment.PspReference,
            payment.FailureReason,
            payment.CreatedAt,
            payment.UpdatedAt);
}
