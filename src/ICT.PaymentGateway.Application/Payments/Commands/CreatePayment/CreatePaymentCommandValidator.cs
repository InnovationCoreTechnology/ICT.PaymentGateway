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
            .Must(token => !LooksLikeCardNumber(token))
            .WithMessage("'Payment Method Token' must be a token, not a card number.");
        RuleFor(x => x.Description).MaximumLength(500);
    }

    // PCI guard: a token made only of 13-19 digits (spaces and dashes allowed) is probably a card number.
    private static bool LooksLikeCardNumber(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        var digits = 0;
        foreach (var c in value)
        {
            if (char.IsAsciiDigit(c))
            {
                digits++;
            }
            else if (c is not (' ' or '-'))
            {
                return false;
            }
        }

        return digits is >= 13 and <= 19;
    }
}
