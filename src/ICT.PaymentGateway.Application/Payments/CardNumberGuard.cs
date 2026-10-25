namespace ICT.PaymentGateway.Application.Payments;

/// <summary>PCI guard: a token made only of 13-19 digits (spaces and dashes allowed) is probably a card number.</summary>
internal static class CardNumberGuard
{
    public static bool LooksLikeCardNumber(string? value)
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
