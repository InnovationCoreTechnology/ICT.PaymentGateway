using ICT.PaymentGateway.Application.Common.Messaging;
using ICT.PaymentGateway.Application.Common.Results;

namespace ICT.PaymentGateway.Application.Payments.Queries.GetPaymentById;

internal sealed class GetPaymentByIdQueryHandler(IPaymentRepository payments)
    : IRequestHandler<GetPaymentByIdQuery, Result<PaymentDto>>
{
    public async Task<Result<PaymentDto>> HandleAsync(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await payments.GetByIdAsync(request.PaymentId, request.MerchantId, cancellationToken);

        return payment is null
            ? Result<PaymentDto>.Failure(Error.NotFound("payment.not_found", "Payment not found."))
            : Result<PaymentDto>.Success(PaymentDto.From(payment));
    }
}
