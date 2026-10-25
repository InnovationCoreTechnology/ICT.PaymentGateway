using ICT.PaymentGateway.Application.Common.Messaging;
using ICT.PaymentGateway.Application.Common.Results;

namespace ICT.PaymentGateway.Application.Subscriptions.Queries.GetCurrentSubscription;

internal sealed class GetCurrentSubscriptionQueryHandler(
    ISubscriptionRepository subscriptions,
    ISubscriptionPlanCatalog catalog,
    TimeProvider clock) : IRequestHandler<GetCurrentSubscriptionQuery, Result<SubscriptionDto>>
{
    public async Task<Result<SubscriptionDto>> HandleAsync(GetCurrentSubscriptionQuery request, CancellationToken cancellationToken)
    {
        var active = await subscriptions.GetActiveAsync(request.MerchantId, clock.GetUtcNow(), cancellationToken);

        return active is null
            ? Result<SubscriptionDto>.Failure(Error.NotFound("subscription.none", "No active subscription."))
            : Result<SubscriptionDto>.Success(SubscriptionDto.From(active, catalog));
    }
}
