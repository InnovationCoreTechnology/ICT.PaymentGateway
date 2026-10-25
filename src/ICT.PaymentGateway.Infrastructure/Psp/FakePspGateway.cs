using ICT.PaymentGateway.Application.Payments;

namespace ICT.PaymentGateway.Infrastructure.Psp;

/// <summary>Stand-in PSP for development and tests. Tokens starting with tok_decline are declined; all others are approved.</summary>
internal sealed class FakePspGateway : IPspGateway
{
    public Task<PspAuthorizationResult> AuthorizeAsync(PspAuthorizationRequest request, CancellationToken cancellationToken)
    {
        var result = request.PaymentMethodToken.StartsWith("tok_decline", StringComparison.OrdinalIgnoreCase)
            ? new PspAuthorizationResult(false, null, "Card declined.")
            : new PspAuthorizationResult(true, $"fake_{Guid.NewGuid():N}", null);

        return Task.FromResult(result);
    }
}
