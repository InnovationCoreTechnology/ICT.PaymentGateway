using ICT.PaymentGateway.Domain.Subscriptions;

namespace ICT.PaymentGateway.Application.Subscriptions;

public interface ISubscriptionRepository
{
    Task AddAsync(Subscription subscription, CancellationToken cancellationToken);

    /// <summary>The newest subscription of the merchant that is active at <paramref name="now"/>, or null when locked.</summary>
    Task<Subscription?> GetActiveAsync(Guid merchantId, DateTimeOffset now, CancellationToken cancellationToken);

    Task<Subscription?> GetByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken);
}
