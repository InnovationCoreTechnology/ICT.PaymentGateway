using ICT.PaymentGateway.Application.Common.Messaging;
using ICT.PaymentGateway.Application.Common.Results;

namespace ICT.PaymentGateway.Application.Payments.Commands.CreatePayment;

public sealed record CreatePaymentCommand(
    Guid MerchantId,
    string IdempotencyKey,
    decimal Amount,
    string Currency,
    string PaymentMethodToken,
    string? Description) : ICommand<Result<PaymentDto>>;
