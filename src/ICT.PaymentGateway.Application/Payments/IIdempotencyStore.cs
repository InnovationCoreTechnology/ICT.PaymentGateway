using ICT.PaymentGateway.Domain.Payments;

namespace ICT.PaymentGateway.Application.Payments;

public interface IIdempotencyStore
{
    Task<IdempotencyRecord?> FindAsync(Guid merchantId, string key, CancellationToken cancellationToken);

    Task AddAsync(IdempotencyRecord record, CancellationToken cancellationToken);
}
