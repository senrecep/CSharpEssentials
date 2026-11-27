using CSharpEssentials.Errors;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Decides the HTTP status, the primary error and the title of a problem built from <see cref="Error"/> values.
/// Register your own implementation (for example to send specific error codes as 401/403) with
/// <c>services.AddErrorStatusCodeMapper&lt;T&gt;()</c>, or derive from <see cref="DefaultErrorStatusCodeMapper"/>.
/// </summary>
public interface IErrorStatusCodeMapper
{
    /// <summary>
    /// The HTTP status code for a single error.
    /// </summary>
    int GetStatusCode(Error error);

    /// <summary>
    /// The error that defines the response status, <c>title</c> and <c>detail</c>. <paramref name="errors"/> is never empty.
    /// </summary>
    /// <remarks>
    /// The default picks the error with the highest status, the first one on a tie. With the default statuses that order is
    /// 5xx, then 422 (<see cref="ErrorType.Domain"/>), 409, 404, 403, 401 and 400.
    /// </remarks>
    Error SelectPrimaryError(IReadOnlyList<Error> errors);

    /// <summary>
    /// The <c>title</c> for the response.
    /// </summary>
    string? GetTitle(Error primaryError, int statusCode);

    /// <summary>
    /// The statuses an error of type <paramref name="errorType"/> can be sent with. <see cref="HttpResult{TSuccess, E1}"/>
    /// declares one problem response per status for each error slot, so the OpenAPI document lists them.
    /// </summary>
    /// <remarks>
    /// The default reads <see cref="ErrorKindAttribute"/> from <paramref name="errorType"/> and returns
    /// <see cref="GetStatusCode(Error)"/> for an error of that kind, or nothing when the type has no kind (such as
    /// <see cref="Error"/>). Override it when <see cref="GetStatusCode(Error)"/> maps errors of the type by code or by
    /// metadata, so every status it can return is declared.
    /// </remarks>
    /// <param name="errorType">The error type of a result slot.</param>
    IEnumerable<int> GetDeclaredStatusCodes(Type errorType) => HttpResultMetadata.DeclaredStatusCodes(this, errorType);
}
