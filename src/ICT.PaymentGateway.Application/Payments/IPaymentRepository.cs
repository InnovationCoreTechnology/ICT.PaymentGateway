using ICT.PaymentGateway.Domain.Payments;

namespace ICT.PaymentGateway.Application.Payments;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment, CancellationToken cancellationToken);

    Task<Payment?> GetByIdAsync(Guid id, Guid merchantId, CancellationToken cancellationToken);
}
