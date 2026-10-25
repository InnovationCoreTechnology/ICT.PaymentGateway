namespace ICT.PaymentGateway.Application.Payments;

public interface IPspGateway
{
    Task<PspAuthorizationResult> AuthorizeAsync(PspAuthorizationRequest request, CancellationToken cancellationToken);
}
