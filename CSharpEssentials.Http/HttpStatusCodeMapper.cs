using System.Net;
using CSharpEssentials.Errors;

namespace CSharpEssentials.Http;

public static class HttpStatusCodeMapper
{
#pragma warning disable IDE0072
    public static ErrorType ToErrorType(HttpStatusCode statusCode)
        => statusCode switch
        {
            HttpStatusCode.BadRequest => ErrorType.Validation,
            HttpStatusCode.Unauthorized => ErrorType.Unauthorized,
            HttpStatusCode.PaymentRequired => ErrorType.Failure,
            HttpStatusCode.Forbidden => ErrorType.Forbidden,
            HttpStatusCode.NotFound => ErrorType.NotFound,
            HttpStatusCode.MethodNotAllowed => ErrorType.Failure,
            HttpStatusCode.NotAcceptable => ErrorType.Failure,
            HttpStatusCode.ProxyAuthenticationRequired => ErrorType.Unauthorized,
            HttpStatusCode.RequestTimeout => ErrorType.Unexpected,
            HttpStatusCode.Conflict => ErrorType.Conflict,
            HttpStatusCode.Gone => ErrorType.NotFound,
            HttpStatusCode.LengthRequired => ErrorType.Validation,
            HttpStatusCode.PreconditionFailed => ErrorType.Conflict,
            HttpStatusCode.RequestEntityTooLarge => ErrorType.Validation,
            HttpStatusCode.RequestUriTooLong => ErrorType.Validation,
            HttpStatusCode.UnsupportedMediaType => ErrorType.Validation,
            HttpStatusCode.RequestedRangeNotSatisfiable => ErrorType.Validation,
            HttpStatusCode.ExpectationFailed => ErrorType.Validation,
            HttpStatusCode.UnprocessableEntity => ErrorType.Validation,
            HttpStatusCode.TooManyRequests => ErrorType.Conflict,
            >= HttpStatusCode.BadRequest and < HttpStatusCode.InternalServerError => ErrorType.Failure,
            >= HttpStatusCode.InternalServerError => ErrorType.Unexpected,
            _ => ErrorType.Failure
        };

    public static Error ToError(HttpStatusCode statusCode, string? description = null)
    {
        string code = $"Http.{(int)statusCode}";
        string msg = description ?? $"HTTP request failed with status code {(int)statusCode} ({statusCode}).";
        return statusCode switch
        {
            HttpStatusCode.BadRequest => Error.Validation(code, msg),
            HttpStatusCode.Unauthorized => Error.Unauthorized(code, msg),
            HttpStatusCode.PaymentRequired => Error.Failure(code, msg),
            HttpStatusCode.Forbidden => Error.Forbidden(code, msg),
            HttpStatusCode.NotFound => Error.NotFound(code, msg),
            HttpStatusCode.MethodNotAllowed => Error.Failure(code, msg),
            HttpStatusCode.NotAcceptable => Error.Failure(code, msg),
            HttpStatusCode.ProxyAuthenticationRequired => Error.Unauthorized(code, msg),
            HttpStatusCode.RequestTimeout => Error.Unexpected(code, msg),
            HttpStatusCode.Conflict => Error.Conflict(code, msg),
            HttpStatusCode.Gone => Error.NotFound(code, msg),
            HttpStatusCode.LengthRequired => Error.Validation(code, msg),
            HttpStatusCode.PreconditionFailed => Error.Conflict(code, msg),
            HttpStatusCode.RequestEntityTooLarge => Error.Validation(code, msg),
            HttpStatusCode.RequestUriTooLong => Error.Validation(code, msg),
            HttpStatusCode.UnsupportedMediaType => Error.Validation(code, msg),
            HttpStatusCode.RequestedRangeNotSatisfiable => Error.Validation(code, msg),
            HttpStatusCode.ExpectationFailed => Error.Validation(code, msg),
            HttpStatusCode.UnprocessableEntity => Error.Validation(code, msg),
            HttpStatusCode.TooManyRequests => Error.Conflict(code, msg),
            >= HttpStatusCode.InternalServerError => Error.Unexpected(code, msg),
            _ => Error.Failure(code, msg)
        };
    }
#pragma warning restore IDE0072
}
