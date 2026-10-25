using ICT.PaymentGateway.Domain.Common;

namespace ICT.PaymentGateway.Domain.Payments;

/// <summary>An amount of money: a decimal plus an ISO 4217 currency code.</summary>
public sealed record Money
{
    private static readonly HashSet<string> SupportedCurrencies = new(StringComparer.Ordinal)
    {
        "AED", "AUD", "BRL", "CAD", "CHF", "CNY", "EUR", "GBP", "HKD", "INR",
        "JPY", "MXN", "NZD", "SAR", "SEK", "SGD", "USD", "ZAR",
    };

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; }

    public string Currency { get; }

    public static Money Create(decimal amount, string currency)
    {
        if (amount <= 0)
        {
            throw new DomainException("Amount must be greater than zero.");
        }

        if (decimal.Round(amount, 4) != amount)
        {
            throw new DomainException("Amount cannot have more than 4 decimal places.");
        }

        var code = currency?.Trim().ToUpperInvariant();
        if (code is null || !SupportedCurrencies.Contains(code))
        {
            throw new DomainException($"Currency '{currency}' is not a supported ISO 4217 code.");
        }

        return new Money(amount, code);
    }
}
