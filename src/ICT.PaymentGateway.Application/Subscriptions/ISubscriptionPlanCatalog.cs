using ICT.PaymentGateway.Domain.Subscriptions;

namespace ICT.PaymentGateway.Application.Subscriptions;

public interface ISubscriptionPlanCatalog
{
    IReadOnlyList<PlanDefinition> GetAll();

    PlanDefinition? Find(SubscriptionPlan plan);
}
