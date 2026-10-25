using ICT.PaymentGateway.API.Subscriptions;
using ICT.PaymentGateway.Domain.Subscriptions;
using Microsoft.AspNetCore.Mvc;

namespace ICT.PaymentGateway.API.Controllers;

/// <summary>Example endpoints, one per tier, showing how a feature is unlocked by a subscription. Replace with real features.</summary>
[ApiController]
[Route("api/v1/membership")]
[Produces("application/json")]
public sealed class MembershipFeaturesController : ControllerBase
{
    [HttpGet("basic")]
    [RequiresPlan(SubscriptionPlan.Basic)]
    [ProducesResponseType<FeatureResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public IActionResult BasicFeature() => Ok(new FeatureResponse(SubscriptionPlan.Basic, "Basic feature unlocked."));

    [HttpGet("classic")]
    [RequiresPlan(SubscriptionPlan.Classic)]
    [ProducesResponseType<FeatureResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public IActionResult ClassicFeature() => Ok(new FeatureResponse(SubscriptionPlan.Classic, "Classic feature unlocked."));

    [HttpGet("gold")]
    [RequiresPlan(SubscriptionPlan.Gold)]
    [ProducesResponseType<FeatureResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public IActionResult GoldFeature() => Ok(new FeatureResponse(SubscriptionPlan.Gold, "Gold feature unlocked."));

    public sealed record FeatureResponse(SubscriptionPlan RequiredPlan, string Message);
}
