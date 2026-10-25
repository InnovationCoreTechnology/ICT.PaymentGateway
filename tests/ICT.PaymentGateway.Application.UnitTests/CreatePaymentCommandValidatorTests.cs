using ICT.PaymentGateway.Application.Payments.Commands.CreatePayment;

namespace ICT.PaymentGateway.Application.UnitTests;

public class CreatePaymentCommandValidatorTests
{
    private readonly CreatePaymentCommandValidator _validator = new();

    private static CreatePaymentCommand Command(string token = "tok_abc", decimal amount = 10m, string currency = "USD") =>
        new(Guid.NewGuid(), "key-1", amount, currency, token, null);

    [Fact]
    public void Validate_WithValidCommand_Passes() =>
        _validator.Validate(Command()).IsValid.ShouldBeTrue();

    [Theory]
    [InlineData("4111111111111111")]
    [InlineData("4111 1111 1111 1111")]
    [InlineData("4111-1111-1111-1111")]
    public void Validate_WhenTokenLooksLikeCardNumber_Fails(string token) =>
        _validator.Validate(Command(token: token)).IsValid.ShouldBeFalse();

    [Fact]
    public void Validate_WithZeroAmount_Fails() =>
        _validator.Validate(Command(amount: 0m)).IsValid.ShouldBeFalse();

    [Fact]
    public void Validate_WithBadCurrencyLength_Fails() =>
        _validator.Validate(Command(currency: "US")).IsValid.ShouldBeFalse();
}
