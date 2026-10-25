using ICT.PaymentGateway.Domain.Subscriptions;

namespace ICT.PaymentGateway.API.Contracts;

/// <param name="Plan">Basic, Classic or Gold.</param>
/// <param name="PaymentMethodToken">Token from the PSP or card vault. Raw card numbers are rejected.</param>
public sealed record PurchaseSubscriptionRequest(SubscriptionPlan Plan, string PaymentMethodToken);
