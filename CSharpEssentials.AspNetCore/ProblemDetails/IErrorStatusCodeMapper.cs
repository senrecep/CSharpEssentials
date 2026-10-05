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
    Error SelectPrimaryError(IReadOnlyList<Error> errors);

    /// <summary>
    /// The <c>title</c> for the response.
    /// </summary>
    string? GetTitle(Error primaryError, int statusCode);
}
