using FluentValidation;

namespace ICT.PaymentGateway.Application.Payments.Commands.CreatePayment;

public sealed class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.MerchantId).NotEmpty();
        RuleFor(x => x.IdempotencyKey).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.PaymentMethodToken)
            .NotEmpty()
            .MaximumLength(200)
            .Must(token => !CardNumberGuard.LooksLikeCardNumber(token))
            .WithMessage("'Payment Method Token' must be a token, not a card number.");
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
