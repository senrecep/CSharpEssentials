using Microsoft.AspNetCore.Http;

namespace CSharpEssentials.AspNetCore;

internal sealed class IdempotencyExecutionFeature : IIdempotencyExecutionFeature
{
    public bool HandlerStarted { get; set; }

    /// <summary>
    /// Marks the handler as started unless an earlier step already failed the request: the request delegate factory
    /// runs endpoint filters after a binding failure with the status already set to 400 and then skips the handler.
    /// </summary>
    internal static void MarkStarted(HttpContext context)
    {
        if (context.Response.StatusCode < StatusCodes.Status400BadRequest
            && context.Features.Get<IIdempotencyExecutionFeature>() is { } feature)
            feature.HandlerStarted = true;
    }
}
