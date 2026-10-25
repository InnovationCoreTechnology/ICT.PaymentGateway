using ICT.PaymentGateway.Domain.Common;
using ICT.PaymentGateway.Domain.Payments;

namespace ICT.PaymentGateway.Domain.UnitTests;

public class MoneyTests
{
    [Fact]
    public void Create_WithValidValues_NormalisesCurrency()
    {
        var money = Money.Create(10.50m, "usd");

        money.Amount.ShouldBe(10.50m);
        money.Currency.ShouldBe("USD");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositiveAmount_Throws(double amount) =>
        Should.Throw<DomainException>(() => Money.Create((decimal)amount, "USD"));

    [Fact]
    public void Create_WithTooManyDecimals_Throws() =>
        Should.Throw<DomainException>(() => Money.Create(1.00001m, "USD"));

    [Theory]
    [InlineData("")]
    [InlineData("US")]
    [InlineData("XXX")]
    public void Create_WithUnsupportedCurrency_Throws(string currency) =>
        Should.Throw<DomainException>(() => Money.Create(1m, currency));
}
