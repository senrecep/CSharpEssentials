using System.Diagnostics.CodeAnalysis;
using CSharpEssentials.Exceptions;
using Microsoft.AspNetCore.Http;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// The fallback exception mapping used by <see cref="GlobalExceptionHandler"/>:
/// <list type="bullet">
/// <item><see cref="OperationCanceledException"/> while <see cref="HttpContext.RequestAborted"/> is canceled → 499 Client Closed Request
/// (any other cancellation, such as a timeout, is a server error and becomes a 500)</item>
/// <item><see cref="BadHttpRequestException"/> → its status code with a fixed detail per status (the framework message is never written)</item>
/// <item><see cref="EnhancedValidationException"/> → its errors</item>
/// <item><see cref="DomainException"/> → its error</item>
/// <item>anything else → 500 without exception details</item>
/// </list>
/// </summary>
public sealed class DefaultExceptionProblemMapper : IExceptionProblemMapper
{
    /// <summary>
    /// 499, the de-facto status for a request the client abandoned.
    /// </summary>
    public const int ClientClosedRequest = 499;

    /// <summary>
    /// A shared instance.
    /// </summary>
    public static DefaultExceptionProblemMapper Instance { get; } = new();

    /// <inheritdoc />
    public bool TryMap(HttpContext httpContext, Exception exception, [NotNullWhen(true)] out ExceptionProblem? problem)
    {
        problem = exception switch
        {
            OperationCanceledException when httpContext.RequestAborted.IsCancellationRequested =>
                new ExceptionProblem(ClientClosedRequest, "Client Closed Request"),
            BadHttpRequestException badRequest => new ExceptionProblem(badRequest.StatusCode, Detail: GetBadRequestDetail(badRequest.StatusCode)),
            EnhancedValidationException validation => new ExceptionProblem(Errors: validation.Errors),
            DomainException domain => new ExceptionProblem(Errors: [domain.Error]),
            _ => new ExceptionProblem(StatusCodes.Status500InternalServerError, "An unexpected error occurred."),
        };
        return true;
    }

    private static string GetBadRequestDetail(int statusCode) => statusCode switch
    {
        StatusCodes.Status408RequestTimeout => "The request timed out.",
        StatusCodes.Status413PayloadTooLarge => "The request body is too large.",
        StatusCodes.Status415UnsupportedMediaType => "The request content type is not supported.",
        StatusCodes.Status431RequestHeaderFieldsTooLarge => "The request headers are too large.",
        _ => "The request is invalid.",
    };
}
