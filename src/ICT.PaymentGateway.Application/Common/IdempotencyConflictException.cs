namespace ICT.PaymentGateway.Application.Common;

/// <summary>Another request with the same Idempotency-Key was saved first.</summary>
public sealed class IdempotencyConflictException(string message) : Exception(message);
