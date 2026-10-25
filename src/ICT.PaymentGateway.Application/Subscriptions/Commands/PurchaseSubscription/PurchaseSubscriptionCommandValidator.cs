using FluentValidation;
using ICT.PaymentGateway.Application.Payments;

namespace ICT.PaymentGateway.Application.Subscriptions.Commands.PurchaseSubscription;

public sealed class PurchaseSubscriptionCommandValidator : AbstractValidator<PurchaseSubscriptionCommand>
{
    public PurchaseSubscriptionCommandValidator()
    {
        RuleFor(x => x.MerchantId).NotEmpty();
        RuleFor(x => x.IdempotencyKey).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Plan).IsInEnum();
        RuleFor(x => x.PaymentMethodToken)
            .NotEmpty()
            .MaximumLength(200)
            .Must(token => !CardNumberGuard.LooksLikeCardNumber(token))
            .WithMessage("'Payment Method Token' must be a token, not a card number.");
    }
}
