namespace ICT.PaymentGateway.API.Contracts;

/// <param name="Amount">Amount in major units, e.g. 12.50.</param>
/// <param name="Currency">ISO 4217 code, e.g. USD.</param>
/// <param name="PaymentMethodToken">Token from the PSP or card vault. Raw card numbers are rejected.</param>
/// <param name="Description">Optional free text, up to 500 characters.</param>
public sealed record CreatePaymentRequest(
    decimal Amount,
    string Currency,
    string PaymentMethodToken,
    string? Description);
