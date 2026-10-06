using System.Diagnostics.CodeAnalysis;
using CSharpEssentials.Enums;
using CSharpEssentials.Json;
using Microsoft.AspNetCore.Http;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Added by <see cref="EnumConventionsExtensions.AddEnumConventions"/>: a rejected enum value of a JSON request body
/// (<see cref="EnumValueJsonException"/>, also inside a <see cref="BadHttpRequestException"/>) becomes a 400 with the error of
/// <see cref="EnumConventionsBuilder.ConfigureErrors"/>, keyed by the JSON path without <c>$.</c>.
/// </summary>
internal sealed class EnumValueExceptionProblemMapper(EnumConventionsRegistration registration) : IExceptionProblemMapper
{
    public bool TryMap(HttpContext httpContext, Exception exception, [NotNullWhen(true)] out ExceptionProblem? problem)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is EnumValueJsonException enumException)
            {
                EnumValueError error = enumException.Error;
                string key = error.Path is { } path && path.StartsWith("$.", StringComparison.Ordinal) ? path[2..] : error.Path ?? "$";
                problem = new ExceptionProblem(StatusCodes.Status400BadRequest, Errors: [registration.CreateError(error, key)]);
                return true;
            }
        }

        problem = null;
        return false;
    }
}
