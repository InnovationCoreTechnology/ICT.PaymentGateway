using ICT.PaymentGateway.Application.Common.Messaging;

namespace ICT.PaymentGateway.Application.Subscriptions.Queries.ListSubscriptionPlans;

internal sealed class ListSubscriptionPlansQueryHandler(ISubscriptionPlanCatalog catalog)
    : IRequestHandler<ListSubscriptionPlansQuery, IReadOnlyList<PlanDto>>
{
    public Task<IReadOnlyList<PlanDto>> HandleAsync(ListSubscriptionPlansQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<PlanDto> plans = catalog.GetAll()
            .OrderBy(p => p.Plan)
            .Select(PlanDto.From)
            .ToList();

        return Task.FromResult(plans);
    }
}
