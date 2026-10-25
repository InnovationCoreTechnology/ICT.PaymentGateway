using ICT.PaymentGateway.Application.Common.Messaging;
using ICT.PaymentGateway.Application.Common.Results;

namespace ICT.PaymentGateway.Application.Subscriptions.Queries.GetCurrentSubscription;

public sealed record GetCurrentSubscriptionQuery(Guid MerchantId) : IQuery<Result<SubscriptionDto>>;
