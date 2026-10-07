using CSharpEssentials.Errors;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Errors of conditional requests.
/// </summary>
public static class ConditionalRequestErrors
{
    /// <summary>The code of <see cref="PreconditionFailed"/>.</summary>
    public const string PreconditionFailedCode = "Http.PreconditionFailed";

    /// <summary>
    /// The <c>If-Match</c> precondition did not match the current resource. An <see cref="ErrorType.Conflict"/> error that
    /// <see cref="DefaultErrorStatusCodeMapper"/> maps to <c>412 Precondition Failed</c> (by its <see cref="Error.Code"/>), so
    /// <c>ToProblemResult()</c>, <c>ToActionResult()</c> and <see cref="ResultEndpointFilter"/> return 412. Elsewhere it is a 409: a registered
    /// <see cref="IResultErrorMapper"/> decides the response of <see cref="ResultEndpointFilter"/> itself, and a custom
    /// <see cref="IErrorStatusCodeMapper"/> that does not derive from <see cref="DefaultErrorStatusCodeMapper"/> must map
    /// <see cref="PreconditionFailedCode"/> to 412 itself.
    /// </summary>
    public static Error PreconditionFailed { get; } = Error.Conflict(
        PreconditionFailedCode,
        "The resource has changed since it was read. Fetch it again and retry with its current ETag.");

    internal static bool IsPreconditionFailed(Error error) =>
        error.Type == ErrorType.Conflict && string.Equals(error.Code, PreconditionFailedCode, StringComparison.Ordinal);
}
