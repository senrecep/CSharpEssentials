using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.RegularExpressions;
using CSharpEssentials.Errors;
using CSharpEssentials.Json;
using Microsoft.AspNetCore.Http;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Added by <see cref="Extensions.AddEnhancedProblemDetails(Microsoft.Extensions.DependencyInjection.IServiceCollection)"/>:
/// a <see cref="BadHttpRequestException"/> whose inner exception is a <see cref="JsonException"/> (a request body that
/// failed to bind) becomes a 400 with one
/// <see cref="ErrorType.Validation"/> error per field, keyed by the JSON path without <c>$.</c> (for example
/// <c>items[2].quantity</c>) and described by a fixed message. A missing required member gives one error per member.
/// An <see cref="EnumValueJsonException"/> is left to the enum conventions mapper. Any other <see cref="JsonException"/>
/// is a server fault and falls through to the 500 of <see cref="DefaultExceptionProblemMapper"/>.
/// <see cref="GlobalExceptionHandler"/> tries this mapper after every other registered mapper.
/// </summary>
internal sealed partial class BindingExceptionProblemMapper : IExceptionProblemMapper
{
    internal const string InvalidValueDescription = "The value is invalid.";
    internal const string RequiredDescription = "The field is required.";

    public bool TryMap(HttpContext httpContext, Exception exception, [NotNullWhen(true)] out ExceptionProblem? problem)
    {
        problem = null;
        if (exception is not BadHttpRequestException { InnerException: JsonException json } || json is EnumValueJsonException)
            return false;

        problem = new ExceptionProblem(StatusCodes.Status400BadRequest, Errors: CreateErrors(json.Path, json.Message));
        return true;
    }

    /// <summary>
    /// The errors of a JSON failure at <paramref name="path"/>. <paramref name="message"/> is only read for the names of
    /// missing required members and is never written. System.Text.Json names them only there and cuts a long list short,
    /// so only the members it names get an error.
    /// </summary>
    internal static Error[] CreateErrors(string? path, string message)
    {
        string key = ToKey(path);
        Match match = MissingRequiredRegex().Match(message);
        if (!match.Success)
            return [Error.Validation(code: key, description: InvalidValueDescription)];

        return [.. match.Groups["name"].Captures.Select(name => Error.Validation(
            code: key == "$" ? name.Value : $"{key}.{name.Value}",
            description: RequiredDescription))];
    }

    internal static string ToKey(string? path)
    {
        if (path is null)
            return "$";
        return path.StartsWith("$.", StringComparison.Ordinal) ? path[2..] : path;
    }

    [GeneratedRegex(@"missing required properties including: (?:'(?<name>[^']*)'(?:, )?)+\.$", RegexOptions.CultureInvariant)]
    private static partial Regex MissingRequiredRegex();
}
