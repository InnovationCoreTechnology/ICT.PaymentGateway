using System.Security.Cryptography;
using System.Text;
using ICT.PaymentGateway.Application.Common;
using ICT.PaymentGateway.Application.Common.Messaging;
using ICT.PaymentGateway.Application.Common.Results;
using ICT.PaymentGateway.Application.Payments;
using ICT.PaymentGateway.Domain.Payments;
using ICT.PaymentGateway.Domain.Subscriptions;

namespace ICT.PaymentGateway.Application.Subscriptions.Commands.PurchaseSubscription;

/// <summary>Charges the plan price and, only when the payment is captured, unlocks the account with that plan.</summary>
internal sealed class PurchaseSubscriptionCommandHandler(
    ISubscriptionPlanCatalog catalog,
    ISubscriptionRepository subscriptions,
    IPaymentRepository payments,
    IIdempotencyStore idempotencyStore,
    IPspGateway psp,
    IUnitOfWork unitOfWork,
    TimeProvider clock) : IRequestHandler<PurchaseSubscriptionCommand, Result<SubscriptionDto>>
{
    public async Task<Result<SubscriptionDto>> HandleAsync(PurchaseSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var requestHash = ComputeHash(request);

        var existing = await idempotencyStore.FindAsync(request.MerchantId, request.IdempotencyKey, cancellationToken);
        if (existing is not null)
        {
            return await ReplayAsync(existing, requestHash, request.MerchantId, cancellationToken);
        }

        var definition = catalog.Find(request.Plan);
        if (definition is null)
        {
            return Result<SubscriptionDto>.Failure(Error.NotFound("subscription.plan_not_found", "The requested plan is not available."));
        }

        var now = clock.GetUtcNow();
        var payment = Payment.Create(request.MerchantId, definition.Price, request.PaymentMethodToken, $"Subscription:{request.Plan}", now);
        payment.MarkPending(now);

        var outcome = await psp.AuthorizeAsync(
            new PspAuthorizationRequest(payment.Id, payment.Amount, payment.PaymentMethodToken),
            cancellationToken);

        Subscription? subscription = null;
        if (outcome.Approved && !string.IsNullOrEmpty(outcome.PspReference))
        {
            payment.Authorize(outcome.PspReference, now);
            payment.Capture(now);

            var current = await subscriptions.GetActiveAsync(request.MerchantId, now, cancellationToken);
            subscription = Subscription.Activate(
                request.MerchantId,
                request.Plan,
                payment.Id,
                TimeSpan.FromDays(definition.DurationDays),
                now,
                current);
            await subscriptions.AddAsync(subscription, cancellationToken);
        }
        else
        {
            payment.Fail(outcome.FailureReason ?? "Declined by payment provider.", now);
        }

        // The payment and idempotency record are saved even when declined, so replaying the key returns the same 402.
        await payments.AddAsync(payment, cancellationToken);
        await idempotencyStore.AddAsync(
            IdempotencyRecord.Create(request.MerchantId, request.IdempotencyKey, requestHash, payment.Id, now),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return subscription is null
            ? Result<SubscriptionDto>.Failure(DeclinedError(payment.FailureReason))
            : Result<SubscriptionDto>.Success(SubscriptionDto.From(subscription, catalog));
    }

    private async Task<Result<SubscriptionDto>> ReplayAsync(
        IdempotencyRecord record,
        string requestHash,
        Guid merchantId,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(record.RequestHash, requestHash, StringComparison.Ordinal))
        {
            return Result<SubscriptionDto>.Failure(Error.Unprocessable(
                "idempotency.key_reused",
                "This Idempotency-Key was already used with a different request body."));
        }

        var original = await subscriptions.GetByPaymentIdAsync(record.PaymentId, cancellationToken);
        if (original is not null)
        {
            return Result<SubscriptionDto>.Success(SubscriptionDto.From(original, catalog));
        }

        // No subscription for this payment means the original attempt was declined.
        var payment = await payments.GetByIdAsync(record.PaymentId, merchantId, cancellationToken);
        return Result<SubscriptionDto>.Failure(DeclinedError(payment?.FailureReason));
    }

    private static Error DeclinedError(string? reason) =>
        Error.PaymentRequired("subscription.payment_declined", reason ?? "The payment was declined.");

    private static string ComputeHash(PurchaseSubscriptionCommand request)
    {
        var canonical = string.Join("|", "subscription", request.Plan, request.PaymentMethodToken);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }
}
