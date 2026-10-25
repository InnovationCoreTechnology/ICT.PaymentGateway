namespace ICT.PaymentGateway.Application.Payments;

public sealed record PspAuthorizationResult(bool Approved, string? PspReference, string? FailureReason);
