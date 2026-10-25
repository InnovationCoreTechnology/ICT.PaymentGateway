using ICT.PaymentGateway.Domain.Payments;

namespace ICT.PaymentGateway.Application.Payments;

/// <summary>What the PSP needs to authorise a payment. Carries a token, never card data.</summary>
public sealed record PspAuthorizationRequest(Guid PaymentId, Money Amount, string PaymentMethodToken);
