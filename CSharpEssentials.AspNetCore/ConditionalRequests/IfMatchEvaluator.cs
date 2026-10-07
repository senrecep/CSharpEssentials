using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// The <c>If-Match</c> handling shared by <see cref="IfMatchEndpointFilter"/> and <see cref="IfMatchResourceFilter"/>: parses
/// the header into <see cref="Preconditions"/> (a request feature) and returns the problem to send instead of running the
/// handler, if any.
/// </summary>
internal static class IfMatchEvaluator
{
    public static async ValueTask<ProblemDetails?> EvaluateAsync(
        HttpContext httpContext,
        IfMatchMetadata settings,
        string caller)
    {
        if (!IsUnsafe(httpContext.Request.Method))
        {
            httpContext.Features.Set(Preconditions.None);
            return null;
        }

        StringValues header = httpContext.Request.Headers.IfMatch;
        if (IsBlank(header))
        {
            httpContext.Features.Set(Preconditions.None);
            return settings.Required
                ? Problem(StatusCodes.Status428PreconditionRequired, "Precondition Required",
                    "This request requires an If-Match header with the current ETag of the resource.")
                : null;
        }

        if (!EntityTagHeaderValue.TryParseStrictList(header, out IList<EntityTagHeaderValue>? tags)
            || tags.Count == 0)
            return Problem(StatusCodes.Status400BadRequest, "Invalid If-Match header",
                "The If-Match header must be '*' or a comma-separated list of entity tags.");

        bool isWildcard = false;
        foreach (EntityTagHeaderValue tag in tags)
            isWildcard |= tag.Equals(EntityTagHeaderValue.Any);
        if (isWildcard && tags.Count > 1)
            return Problem(StatusCodes.Status400BadRequest, "Invalid If-Match header",
                "'*' cannot be combined with entity tags in the If-Match header.");

        var preconditions = new Preconditions(isWildcard ? [] : [.. tags], isWildcard);
        httpContext.Features.Set(preconditions);
        if (settings.LoadCurrent is null)
            return null;

        object? current = await settings.LoadCurrent(httpContext, httpContext.RequestAborted);
        IServiceProvider services = httpContext.RequestServices;
        ResourceValidators? validators = current is null
            ? null
            : ResourceValidatorsResolver.GetRequired(services, caller).Resolve(services, current);
        return current is not null && (isWildcard || preconditions.Matches(validators))
            ? null
            : Problem(StatusCodes.Status412PreconditionFailed, "Precondition Failed", ConditionalRequestErrors.PreconditionFailed.Description);
    }

    public static Task WriteAsync(HttpContext httpContext, ProblemDetails problem) =>
        EnhancedProblemDetailsWriter.WriteAsync(new ProblemDetailsContext { HttpContext = httpContext, ProblemDetails = problem });

    private static bool IsBlank(StringValues header)
    {
        foreach (string? value in header)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return false;
        }
        return true;
    }

    private static bool IsUnsafe(string method) =>
        HttpMethods.IsPost(method) || HttpMethods.IsPut(method) || HttpMethods.IsPatch(method) || HttpMethods.IsDelete(method);

    private static ProblemDetails Problem(int statusCode, string title, string detail) =>
        new() { Status = statusCode, Title = title, Detail = detail };
}
