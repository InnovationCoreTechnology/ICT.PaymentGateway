using ICT.PaymentGateway.Application.Subscriptions;
using ICT.PaymentGateway.Domain.Subscriptions;
using ICT.PaymentGateway.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ICT.PaymentGateway.Infrastructure.Subscriptions;

internal sealed class SubscriptionRepository(PaymentGatewayDbContext db) : ISubscriptionRepository
{
    public async Task AddAsync(Subscription subscription, CancellationToken cancellationToken) =>
        await db.Subscriptions.AddAsync(subscription, cancellationToken);

    public Task<Subscription?> GetActiveAsync(Guid merchantId, DateTimeOffset now, CancellationToken cancellationToken) =>
        db.Subscriptions.AsNoTracking()
            .Where(s => s.MerchantId == merchantId && s.StartsAt <= now && s.ExpiresAt > now)
            .OrderByDescending(s => s.StartsAt)
            .ThenByDescending(s => s.ExpiresAt)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<Subscription?> GetByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken) =>
        db.Subscriptions.AsNoTracking().FirstOrDefaultAsync(s => s.PaymentId == paymentId, cancellationToken);
}
