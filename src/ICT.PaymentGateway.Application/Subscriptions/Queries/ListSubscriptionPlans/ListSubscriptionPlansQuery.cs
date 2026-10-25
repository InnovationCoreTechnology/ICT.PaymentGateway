using ICT.PaymentGateway.Application.Common.Messaging;

namespace ICT.PaymentGateway.Application.Subscriptions.Queries.ListSubscriptionPlans;

public sealed record ListSubscriptionPlansQuery : IQuery<IReadOnlyList<PlanDto>>;
