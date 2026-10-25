using ICT.PaymentGateway.Application.Common.Messaging;
using ICT.PaymentGateway.Application.Common.Results;

namespace ICT.PaymentGateway.Application.Payments.Queries.GetPaymentById;

public sealed record GetPaymentByIdQuery(Guid MerchantId, Guid PaymentId) : IQuery<Result<PaymentDto>>;
