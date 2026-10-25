using ICT.PaymentGateway.API.Contracts;
using ICT.PaymentGateway.Application.Common.Messaging;
using ICT.PaymentGateway.Application.Payments;
using ICT.PaymentGateway.Application.Payments.Commands.CreatePayment;
using ICT.PaymentGateway.Application.Payments.Queries.GetPaymentById;
using Microsoft.AspNetCore.Mvc;

namespace ICT.PaymentGateway.API.Controllers;

[ApiController]
[Route("api/v1/payments")]
[Produces("application/json")]
public sealed class PaymentsController(IDispatcher dispatcher) : ControllerBase
{
    /// <summary>Creates a payment. Replaying the same Idempotency-Key and body returns the original payment.</summary>
    [HttpPost]
    [ProducesResponseType<PaymentDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromHeader(Name = ApiHeaders.IdempotencyKey)] string? idempotencyKey,
        [FromHeader(Name = ApiHeaders.MerchantId)] string? merchantHeader,
        [FromBody] CreatePaymentRequest request,
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
            new CreatePaymentCommand(
                merchantId,
                idempotencyKey,
                request.Amount,
                request.Currency,
                request.PaymentMethodToken,
                request.Description),
            cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : this.ToProblem(result.Error);
    }

    /// <summary>Gets a payment by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<PaymentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        [FromHeader(Name = ApiHeaders.MerchantId)] string? merchantHeader,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(merchantHeader, out var merchantId))
        {
            return this.MissingHeaderProblem($"The {ApiHeaders.MerchantId} header must be a valid GUID.");
        }

        var result = await dispatcher.SendAsync(new GetPaymentByIdQuery(merchantId, id), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error);
    }
}
