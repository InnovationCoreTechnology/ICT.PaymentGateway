using ICT.PaymentGateway.Application.Common.Messaging;
using ICT.PaymentGateway.Application.Common.Results;
using ICT.PaymentGateway.Domain.Subscriptions;

namespace ICT.PaymentGateway.Application.Subscriptions.Commands.PurchaseSubscription;

public sealed record PurchaseSubscriptionCommand(
    Guid MerchantId,
    string IdempotencyKey,
    SubscriptionPlan Plan,
    string PaymentMethodToken) : ICommand<Result<SubscriptionDto>>;
