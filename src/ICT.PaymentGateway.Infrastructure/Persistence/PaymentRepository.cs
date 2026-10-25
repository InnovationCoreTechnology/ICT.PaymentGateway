using ICT.PaymentGateway.Application.Payments;
using ICT.PaymentGateway.Domain.Payments;
using Microsoft.EntityFrameworkCore;

namespace ICT.PaymentGateway.Infrastructure.Persistence;

internal sealed class PaymentRepository(PaymentGatewayDbContext db) : IPaymentRepository
{
    public async Task AddAsync(Payment payment, CancellationToken cancellationToken) =>
        await db.Payments.AddAsync(payment, cancellationToken);

    public Task<Payment?> GetByIdAsync(Guid id, Guid merchantId, CancellationToken cancellationToken) =>
        db.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id && p.MerchantId == merchantId, cancellationToken);
}
