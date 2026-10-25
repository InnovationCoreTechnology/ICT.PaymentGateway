using ICT.PaymentGateway.API.Contracts;
using ICT.PaymentGateway.Application.Common.Messaging;
using ICT.PaymentGateway.Application.Subscriptions;
using ICT.PaymentGateway.Application.Subscriptions.Commands.PurchaseSubscription;
using ICT.PaymentGateway.Application.Subscriptions.Queries.GetCurrentSubscription;
using ICT.PaymentGateway.Application.Subscriptions.Queries.ListSubscriptionPlans;
using Microsoft.AspNetCore.Mvc;

namespace ICT.PaymentGateway.API.Controllers;

[ApiController]
[Produces("application/json")]
public sealed class SubscriptionsController(IDispatcher dispatcher) : ControllerBase
{
    /// <summary>Lists the Basic, Classic and Gold plans with price and features.</summary>
    [HttpGet("api/v1/subscription-plans")]
    [ProducesResponseType<IReadOnlyList<PlanDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListPlans(CancellationToken cancellationToken) =>
        Ok(await dispatcher.SendAsync(new ListSubscriptionPlansQuery(), cancellationToken));

    /// <summary>
    /// Pays for a plan and unlocks the account with it. Replaying the same Idempotency-Key and body returns the original result.
    /// </summary>
    [HttpPost("api/v1/subscriptions")]
    [ProducesResponseType<SubscriptionDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status402PaymentRequired)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Purchase(
        [FromHeader(Name = ApiHeaders.IdempotencyKey)] string? idempotencyKey,
        [FromHeader(Name = ApiHeaders.MerchantId)] string? merchantHeader,
        [FromBody] PurchaseSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return this.MissingHeaderProblem($"The {ApiHeaders.IdempotencyKey} header is required.");
        }

        if (!Guid.TryParse(merchantHeader, out var merchantId))
        {
            return this.MissingHeaderProblem($"The {ApiHeaders.MerchantId} header must be a valid GUID.");
        }

        var result = await dispatcher.SendAsync(
            new PurchaseSubscriptionCommand(merchantId, idempotencyKey, request.Plan, request.PaymentMethodToken),
            cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetCurrent), result.Value)
            : this.ToProblem(result.Error);
    }

    /// <summary>Gets the active plan, expiry and unlocked features. Returns 404 while the account is locked.</summary>
    [HttpGet("api/v1/subscriptions/current")]
    [ProducesResponseType<SubscriptionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrent(
        [FromHeader(Name = ApiHeaders.MerchantId)] string? merchantHeader,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(merchantHeader, out var merchantId))
        {
            return this.MissingHeaderProblem($"The {ApiHeaders.MerchantId} header must be a valid GUID.");
        }

        var result = await dispatcher.SendAsync(new GetCurrentSubscriptionQuery(merchantId), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error);
    }
}
