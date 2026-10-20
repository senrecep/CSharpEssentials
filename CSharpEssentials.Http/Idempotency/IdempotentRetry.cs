using System.Net;
using System.Net.Http.Headers;
using Polly;
using Polly.Timeout;

namespace CSharpEssentials.Http;

/// <summary>
/// The method-aware retry decision of the <c>Idempotency-Key</c> contract, independent of any message handler.
/// Plug it into a Polly <c>ShouldHandle</c> predicate, or use
/// <see cref="IdempotentRetryStrategyOptionsExtensions.RetryUnsafeMethodsOnlyWithIdempotencyKey"/>.
/// </summary>
public static class IdempotentRetry
{
    /// <summary>
    /// The <see cref="ResilienceContext"/> property that carries the request being sent. Its name matches the key that
    /// <c>Microsoft.Extensions.Http.Resilience</c> uses, so the request is found there when no response arrived.
    /// Set it yourself when you run a pipeline outside that handler.
    /// </summary>
    public static ResiliencePropertyKey<HttpRequestMessage> RequestMessageKey { get; } = new("Resilience.Http.RequestMessage");

    /// <summary>
    /// Decides whether the attempt that sent <paramref name="request"/> and ended with <paramref name="response"/> or
    /// <paramref name="exception"/> should be sent again with the same <c>Idempotency-Key</c>.
    /// <list type="bullet">
    /// <item>A response carrying <c>Idempotency-Replayed: true</c> is final and never retried.</item>
    /// <item>GET, HEAD, PUT, DELETE and OPTIONS retry when no response arrived, and on 408, 429 and 5xx.</item>
    /// <item>
    /// Every other method (POST, PATCH, ...) retries only when the request has an <c>Idempotency-Key</c>, and then only
    /// when no response arrived, on 408, 429, 503 and 504, and on a 409 whose <c>application/problem+json</c> body has
    /// the top-level <c>code</c> <c>Idempotency.InFlight</c>.
    /// </item>
    /// <item>
    /// A 409 is read for at most 4 KiB within <see cref="IdempotentRetryOptions.ProblemReadTimeout"/>; the bytes read
    /// are put back into the response. A body that cannot be read, is too long or carries another code is not retried.
    /// </item>
    /// <item>Caller cancellation (<paramref name="cancellationToken"/> is cancelled) is never retried.</item>
    /// </list>
    /// </summary>
    /// <param name="request">The request that was sent.</param>
    /// <param name="response">The response, or <see langword="null"/> when none arrived.</param>
    /// <param name="exception">The exception of the attempt when no response arrived.</param>
    /// <param name="options">Settings; <see langword="null"/> uses the defaults.</param>
    /// <param name="cancellationToken">The caller's token.</param>
    /// <returns><see langword="true"/> to send the request again.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    public static async ValueTask<bool> ShouldRetryAsync(
        HttpRequestMessage request,
        HttpResponseMessage? response,
        Exception? exception,
        IdempotentRetryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(request);
#else
        if (request is null)
            throw new ArgumentNullException(nameof(request));
#endif

        if (cancellationToken.IsCancellationRequested)
            return false;

        bool safe = IdempotencyHttpProtocol.IsSafe(request.Method);
        bool keyed = IdempotencyHttpProtocol.HasKey(request);
        if (!safe && !keyed)
            return false;

        if (response is null)
            return exception is not null && IsNoResponse(exception);

        if (IdempotencyHttpProtocol.IsReplayed(response))
            return false;

        int status = (int)response.StatusCode;
        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            return keyed && await IdempotencyProblemReader
                .IsInFlightAsync(response, options ?? new IdempotentRetryOptions(), cancellationToken)
                .ConfigureAwait(false);
        }

        if (status is 408 or 429 or 503 or 504)
            return true;

        return safe && status >= 500;
    }

    /// <summary>
    /// The wait asked for by the <c>Retry-After</c> header of <paramref name="response"/>, clamped to
    /// [0, <paramref name="maxRetryAfter"/>]. Delta-seconds are used as they are; an HTTP-date is measured from the
    /// response's <c>Date</c> header, or from the current UTC time when the response has none.
    /// </summary>
    /// <param name="response">The response to read.</param>
    /// <param name="maxRetryAfter">The longest wait to return.</param>
    /// <returns>The wait, or <see langword="null"/> when the response has no valid <c>Retry-After</c> header.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="response"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxRetryAfter"/> is negative.</exception>
    public static TimeSpan? GetRetryAfter(HttpResponseMessage response, TimeSpan maxRetryAfter)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(response);
#else
        if (response is null)
            throw new ArgumentNullException(nameof(response));
#endif
        if (maxRetryAfter < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(maxRetryAfter), maxRetryAfter, "The maximum wait cannot be negative.");

        RetryConditionHeaderValue? retryAfter = response.Headers.RetryAfter;
        TimeSpan delay;
        if (retryAfter?.Delta is { } delta)
            delay = delta;
        else if (retryAfter?.Date is { } date)
            delay = date - (response.Headers.Date ?? DateTimeOffset.UtcNow);
        else
            return null;

        if (delay < TimeSpan.Zero)
            return TimeSpan.Zero;

        return delay > maxRetryAfter ? maxRetryAfter : delay;
    }

    private static bool IsNoResponse(Exception exception) =>
        exception switch
        {
#if NET9_0_OR_GREATER
            SsrfBlockedException => false,
#endif
            HttpRequestException => true,
            TimeoutRejectedException => true,
            OperationCanceledException => true,
            _ => false,
        };
}
