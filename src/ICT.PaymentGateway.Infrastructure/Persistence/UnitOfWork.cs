using ICT.PaymentGateway.Application.Common;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ICT.PaymentGateway.Infrastructure.Persistence;

internal sealed class UnitOfWork(PaymentGatewayDbContext db) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new IdempotencyConflictException("A request with this Idempotency-Key is already being processed.");
        }
    }
}
