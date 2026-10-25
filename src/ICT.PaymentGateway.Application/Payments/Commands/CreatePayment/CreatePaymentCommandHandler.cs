using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using ICT.PaymentGateway.Application.Common;
using ICT.PaymentGateway.Application.Common.Messaging;
using ICT.PaymentGateway.Application.Common.Results;
using ICT.PaymentGateway.Domain.Common;
using ICT.PaymentGateway.Domain.Payments;

namespace ICT.PaymentGateway.Application.Payments.Commands.CreatePayment;

internal sealed class CreatePaymentCommandHandler(
    IPaymentRepository payments,
    IIdempotencyStore idempotencyStore,
    IPspGateway psp,
    IUnitOfWork unitOfWork,
    TimeProvider clock) : IRequestHandler<CreatePaymentCommand, Result<PaymentDto>>
{
    public async Task<Result<PaymentDto>> HandleAsync(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        var requestHash = ComputeHash(request);

        var existing = await idempotencyStore.FindAsync(request.MerchantId, request.IdempotencyKey, cancellationToken);
        if (existing is not null)
        {
            return await ReplayAsync(existing, requestHash, request.MerchantId, cancellationToken);
        }

        Money amount;
        try
        {
            amount = Money.Create(request.Amount, request.Currency);
        }
        catch (DomainException ex)
        {
            return Result<PaymentDto>.Failure(Error.Validation("payment.invalid_amount", ex.Message));
        }

        var now = clock.GetUtcNow();
        var payment = Payment.Create(request.MerchantId, amount, request.PaymentMethodToken, request.Description, now);
        payment.MarkPending(now);

        var outcome = await psp.AuthorizeAsync(
            new PspAuthorizationRequest(payment.Id, payment.Amount, payment.PaymentMethodToken),
            cancellationToken);

        if (outcome.Approved && !string.IsNullOrEmpty(outcome.PspReference))
        {
            payment.Authorize(outcome.PspReference, clock.GetUtcNow());
        }
        else
        {
            payment.Fail(outcome.FailureReason ?? "Declined by payment provider.", clock.GetUtcNow());
        }

        await payments.AddAsync(payment, cancellationToken);
        await idempotencyStore.AddAsync(
            IdempotencyRecord.Create(request.MerchantId, request.IdempotencyKey, requestHash, payment.Id, now),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PaymentDto>.Success(PaymentDto.From(payment));
    }

    private async Task<Result<PaymentDto>> ReplayAsync(
        IdempotencyRecord record,
        string requestHash,
        Guid merchantId,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(record.RequestHash, requestHash, StringComparison.Ordinal))
        {
            return Result<PaymentDto>.Failure(Error.Unprocessable(
                "idempotency.key_reused",
                "This Idempotency-Key was already used with a different request body."));
        }

        var original = await payments.GetByIdAsync(record.PaymentId, merchantId, cancellationToken);
        return original is null
            ? Result<PaymentDto>.Failure(Error.NotFound("payment.not_found", "The original payment could not be found."))
            : Result<PaymentDto>.Success(PaymentDto.From(original));
    }

    private static string ComputeHash(CreatePaymentCommand request)
    {
        var canonical = string.Join(
            "|",
            request.Amount.ToString("0.####", CultureInfo.InvariantCulture),
            request.Currency.Trim().ToUpperInvariant(),
            request.PaymentMethodToken,
            request.Description ?? string.Empty);

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }
}
