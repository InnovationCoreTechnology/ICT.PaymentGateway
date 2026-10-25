namespace ICT.PaymentGateway.Application.Common.Results;

public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Unprocessable = 4,
    PaymentRequired = 5,
}
