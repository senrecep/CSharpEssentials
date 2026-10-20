using CSharpEssentials.Http;

using Polly;
using Polly.Retry;

namespace Examples.Idempotency.EndToEnd.Client;

/// <summary>
/// Retries inside the <see cref="HttpClient"/> pipeline, so the same request (and so the same <c>Idempotency-Key</c>)
/// is sent again. A POST is retried only when it carries a key, and never after a replayed response. With
/// <c>Microsoft.Extensions.Http.Resilience</c> the same rule is one line:
/// <c>AddStandardResilienceHandler(o =&gt; o.Retry.RetryUnsafeMethodsOnlyWithIdempotencyKey())</c>.
/// </summary>
public sealed class IdempotentRetryHandler(HttpMessageHandler innerHandler) : DelegatingHandler(innerHandler)
{
    private static readonly ResiliencePipeline<HttpResponseMessage> Pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
        .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
        {
            MaxRetryAttempts = 3,
            BackoffType = DelayBackoffType.Exponential,
            Delay = TimeSpan.FromMilliseconds(50),
            UseJitter = true,
        }.RetryUnsafeMethodsOnlyWithIdempotencyKey())
        .Build();

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ResilienceContext context = ResilienceContextPool.Shared.Get(cancellationToken);
        context.Properties.Set(IdempotentRetry.RequestMessageKey, request);
        try
        {
            return await Pipeline.ExecuteAsync(
                async (ctx, message) => await base.SendAsync(message, ctx.CancellationToken).ConfigureAwait(false),
                context,
                request).ConfigureAwait(false);
        }
        finally
        {
            ResilienceContextPool.Shared.Return(context);
        }
    }
}
