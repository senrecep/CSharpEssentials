using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Minimal API endpoint filter of <see cref="ConditionalRequestExtensions.WithIfMatch{TBuilder}(TBuilder, bool)"/>: evaluates
/// <c>If-Match</c> before the handler. MVC actions are left to <see cref="IfMatchResourceFilter"/>.
/// </summary>
internal static class IfMatchEndpointFilter
{
    // One instance, so a group and an endpoint that both call WithIfMatch add the filter once.
    public static readonly Func<EndpointFilterFactoryContext, EndpointFilterDelegate, EndpointFilterDelegate> Factory =
        static (_, next) => invocation => InvokeAsync(invocation, next);

    private static async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext invocation, EndpointFilterDelegate next)
    {
        HttpContext httpContext = invocation.HttpContext;
        Endpoint? endpoint = httpContext.GetEndpoint();
        if (endpoint is null
            || endpoint.Metadata.GetMetadata<ActionDescriptor>() is not null
            || endpoint.Metadata.GetMetadata<IfMatchMetadata>() is not { } settings)
            return await next(invocation).ConfigureAwait(false);

        ProblemDetails? problem = await IfMatchEvaluator.EvaluateAsync(
            httpContext,
            settings,
            nameof(ConditionalRequestExtensions.WithIfMatch)).ConfigureAwait(false);
        return problem is null ? await next(invocation).ConfigureAwait(false) : new ProblemResult(problem);
    }

    private sealed class ProblemResult(ProblemDetails problem) : IResult
    {
        public Task ExecuteAsync(HttpContext httpContext) => IfMatchEvaluator.WriteAsync(httpContext, problem);
    }
}
