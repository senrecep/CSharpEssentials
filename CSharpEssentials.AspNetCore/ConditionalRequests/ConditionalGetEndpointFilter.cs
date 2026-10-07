using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Minimal API endpoint filter of <see cref="ConditionalRequestExtensions.WithConditionalGet{TBuilder}(TBuilder)"/>. After the
/// handler (and any inner filter) ran, a successful value is taken from a plain return value, a successful <c>Result&lt;T&gt;</c>
/// or an <see cref="IValueHttpResult"/> with a 2xx or no status (also inside <see cref="INestedHttpResult"/>). Strings,
/// problem details, <see langword="null"/> and failures pass through (also inside a result). MVC actions are left to <see cref="ConditionalGetResultFilter"/>.
/// </summary>
internal static class ConditionalGetEndpointFilter
{
    // One instance, so a group and an endpoint that both call WithConditionalGet add the filter once.
    public static readonly Func<EndpointFilterFactoryContext, EndpointFilterDelegate, EndpointFilterDelegate> Factory =
        static (_, next) => invocation => InvokeAsync(invocation, next);

    private static async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext invocation, EndpointFilterDelegate next)
    {
        object? result = await next(invocation);
        HttpContext httpContext = invocation.HttpContext;
        if (!ConditionalGet.IsGetOrHead(httpContext.Request.Method))
            return result;
        Endpoint? endpoint = httpContext.GetEndpoint();
        if (endpoint is null
            || endpoint.Metadata.GetMetadata<ActionDescriptor>() is not null
            || endpoint.Metadata.GetMetadata<ConditionalGetAttribute>() is null)
            return result;

        object? resource = result switch
        {
            null => null,
            IResult httpResult => GetResource(httpResult),
            _ => ConditionalGet.GetResource(result),
        };
        if (resource is null)
            return result;

        return ConditionalGet.Apply(httpContext, resource, nameof(ConditionalRequestExtensions.WithConditionalGet))
            ? NotModifiedResult.Instance
            : result;
    }

    private static object? GetResource(IResult result)
    {
        while (result is INestedHttpResult nested)
            result = nested.Result;
        if (result is IStatusCodeHttpResult { StatusCode: < 200 or > 299 })
            return null;
        return result is IValueHttpResult { Value: { } value } ? ConditionalGet.GetResource(value) : null;
    }

    /// <summary>
    /// Sets 304 and writes no body. Headers already on the response (<c>ETag</c>, <c>Cache-Control</c>, <c>Vary</c>,
    /// <c>Content-Location</c>) are kept; headers the replaced result would have set itself are not.
    /// </summary>
    private sealed class NotModifiedResult : IResult
    {
        public static readonly NotModifiedResult Instance = new();

        public Task ExecuteAsync(HttpContext httpContext)
        {
            httpContext.Response.StatusCode = StatusCodes.Status304NotModified;
            return Task.CompletedTask;
        }
    }
}
