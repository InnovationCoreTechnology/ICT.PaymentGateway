using ICT.PaymentGateway.API.Contracts;
using ICT.PaymentGateway.Application.Common.Messaging;
using ICT.PaymentGateway.Application.Subscriptions.Queries.GetCurrentSubscription;
using ICT.PaymentGateway.Domain.Subscriptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace ICT.PaymentGateway.API.Subscriptions;

/// <summary>
/// Locks an endpoint until the calling merchant has an active subscription of at least the given plan.
/// Returns 403 Problem Details otherwise.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequiresPlanAttribute(SubscriptionPlan plan) : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var http = context.HttpContext;

        if (!Guid.TryParse(http.Request.Headers[ApiHeaders.MerchantId], out var merchantId))
        {
            context.Result = Problem(http, StatusCodes.Status400BadRequest, "Missing header", $"The {ApiHeaders.MerchantId} header must be a valid GUID.");
            return;
        }

        var dispatcher = http.RequestServices.GetRequiredService<IDispatcher>();
        var current = await dispatcher.SendAsync(new GetCurrentSubscriptionQuery(merchantId), http.RequestAborted);

        if (!current.IsSuccess || current.Value.Plan < plan)
        {
            context.Result = Problem(
                http,
                StatusCodes.Status403Forbidden,
                "Subscription required",
                $"This feature needs an active {plan} plan or higher.");
            return;
        }

        await next();
    }

    private static ObjectResult Problem(HttpContext http, int status, string title, string detail)
    {
        var factory = http.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        var problem = factory.CreateProblemDetails(http, status, title, detail: detail, instance: http.Request.Path);
        return new ObjectResult(problem) { StatusCode = status, ContentTypes = { "application/problem+json" } };
    }
}
