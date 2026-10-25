using ICT.PaymentGateway.Application.Payments;
using ICT.PaymentGateway.Domain.Payments;
using Microsoft.EntityFrameworkCore;

namespace ICT.PaymentGateway.Infrastructure.Persistence;

internal sealed class IdempotencyStore(PaymentGatewayDbContext db) : IIdempotencyStore
{
    public Task<IdempotencyRecord?> FindAsync(Guid merchantId, string key, CancellationToken cancellationToken) =>
        db.IdempotencyRecords.AsNoTracking().FirstOrDefaultAsync(r => r.MerchantId == merchantId && r.Key == key, cancellationToken);

    public async Task AddAsync(IdempotencyRecord record, CancellationToken cancellationToken) =>
        await db.IdempotencyRecords.AddAsync(record, cancellationToken);
}
