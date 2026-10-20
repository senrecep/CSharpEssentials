using Microsoft.AspNetCore.Http;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Endpoint filter added by <c>.WithIdempotency()</c>. It is appended when the convention runs, so it sits inside every
/// filter added before it and sets <see cref="IIdempotencyExecutionFeature.HandlerStarted"/> right before the handler.
/// </summary>
internal static class IdempotencyEndpointFilter
{
    internal static readonly Func<EndpointFilterFactoryContext, EndpointFilterDelegate, EndpointFilterDelegate> Factory =
        static (_, next) => context =>
        {
            IdempotencyExecutionFeature.MarkStarted(context.HttpContext);
            return next(context);
        };
}
