using ICT.PaymentGateway.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace ICT.PaymentGateway.API.Controllers;

internal static class ResultProblemExtensions
{
    public static ObjectResult ToProblem(this ControllerBase controller, Error error)
    {
        var status = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.PaymentRequired => StatusCodes.Status402PaymentRequired,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unprocessable => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status500InternalServerError,
        };

        return controller.Problem(statusCode: status, title: error.Code, detail: error.Message, instance: controller.Request.Path);
    }

    public static ObjectResult MissingHeaderProblem(this ControllerBase controller, string detail) =>
        controller.Problem(statusCode: StatusCodes.Status400BadRequest, title: "Missing header", detail: detail, instance: controller.Request.Path);
}
