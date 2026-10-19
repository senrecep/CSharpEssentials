using Microsoft.AspNetCore.Http;

namespace CSharpEssentials.Tests.AspNetCore.Idempotency;

/// <summary>
/// Produces the outcomes of the idempotency matrix. <c>/matrix</c> and <c>/mvc/orders/matrix</c> produce them from
/// the handler; a request with <c>X-Stage: middleware</c> gets them from a middleware inside <c>UseIdempotency</c>,
/// before any handler starts.
/// </summary>
internal static class IdempotencyOutcomes
{
    public const string StageHeader = "X-Stage";

    public const string Throw = "throw";

    public const string Abort = "abort";

    public static async Task RunAsync(HttpContext context, string outcome)
    {
        if (outcome == Throw)
            throw new InvalidOperationException("boom");
        if (outcome == Abort)
        {
            context.Abort();
            throw new OperationCanceledException(context.RequestAborted);
        }

        context.Response.StatusCode = int.Parse(outcome, System.Globalization.CultureInfo.InvariantCulture);
        await context.Response.WriteAsJsonAsync(new { outcome }, CancellationToken.None);
    }
}
