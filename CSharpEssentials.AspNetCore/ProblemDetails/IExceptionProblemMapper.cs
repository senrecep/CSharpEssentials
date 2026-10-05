using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Turns an exception into a problem for <see cref="GlobalExceptionHandler"/>.
/// Registered mappers are tried in registration order; the first one that returns <see langword="true"/> wins.
/// <see cref="DefaultExceptionProblemMapper"/> always runs last.
/// </summary>
public interface IExceptionProblemMapper
{
    /// <summary>
    /// Maps <paramref name="exception"/>, or returns <see langword="false"/> to let the next mapper try.
    /// </summary>
    bool TryMap(HttpContext httpContext, Exception exception, [NotNullWhen(true)] out ExceptionProblem? problem);
}
