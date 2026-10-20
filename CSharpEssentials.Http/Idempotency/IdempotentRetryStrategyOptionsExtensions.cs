using System.Net;
using Polly.Retry;

namespace CSharpEssentials.Http;

/// <summary>Applies <see cref="IdempotentRetry"/> to a Polly retry strategy of <see cref="HttpResponseMessage"/>.</summary>
public static class IdempotentRetryStrategyOptionsExtensions
{
    /// <summary>
    /// Replaces <see cref="RetryStrategyOptions{TResult}.ShouldHandle"/> with <see cref="IdempotentRetry.ShouldRetryAsync"/>
    /// and makes a <c>409 Idempotency.InFlight</c> wait for its <c>Retry-After</c>, clamped to
    /// <see cref="IdempotentRetryOptions.MaxRetryAfter"/>. Every other delay comes from the
    /// <see cref="RetryStrategyOptions{TResult}.DelayGenerator"/> set before this call, or from the backoff.
    /// Works with <c>AddStandardResilienceHandler(o =&gt; o.Retry.RetryUnsafeMethodsOnlyWithIdempotencyKey())</c>,
    /// because <c>HttpRetryStrategyOptions</c> derives from <see cref="RetryStrategyOptions{TResult}"/>.
    /// The request is taken from <see cref="IdempotentRetry.RequestMessageKey"/>, then from
    /// <see cref="HttpResponseMessage.RequestMessage"/>; an attempt whose request cannot be found is not retried.
    /// </summary>
    /// <param name="options">The retry options to change.</param>
    /// <param name="idempotency">Settings; <see langword="null"/> uses the defaults.</param>
    /// <returns>The same <paramref name="options"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static RetryStrategyOptions<HttpResponseMessage> RetryUnsafeMethodsOnlyWithIdempotencyKey(
        this RetryStrategyOptions<HttpResponseMessage> options,
        IdempotentRetryOptions? idempotency = null)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(options);
#else
        if (options is null)
            throw new ArgumentNullException(nameof(options));
#endif

        IdempotentRetryOptions settings = idempotency ?? new IdempotentRetryOptions();
        Func<RetryDelayGeneratorArguments<HttpResponseMessage>, ValueTask<TimeSpan?>>? previous = options.DelayGenerator;
        options.ShouldHandle = args => ShouldHandleAsync(args, settings);
        options.DelayGenerator = args => GenerateDelayAsync(args, settings, previous);
        return options;
    }

    private static ValueTask<bool> ShouldHandleAsync(RetryPredicateArguments<HttpResponseMessage> args, IdempotentRetryOptions settings)
    {
        HttpResponseMessage? response = args.Outcome.Result;
        HttpRequestMessage? request = args.Context.Properties.TryGetValue(IdempotentRetry.RequestMessageKey, out HttpRequestMessage contextRequest)
            ? contextRequest
            : response?.RequestMessage;
        if (request is null)
            return new ValueTask<bool>(false);

        return IdempotentRetry.ShouldRetryAsync(request, response, args.Outcome.Exception, settings, args.Context.CancellationToken);
    }

    private static ValueTask<TimeSpan?> GenerateDelayAsync(
        RetryDelayGeneratorArguments<HttpResponseMessage> args,
        IdempotentRetryOptions settings,
        Func<RetryDelayGeneratorArguments<HttpResponseMessage>, ValueTask<TimeSpan?>>? previous)
    {
        if (args.Outcome.Result is { StatusCode: HttpStatusCode.Conflict } response
            && IdempotentRetry.GetRetryAfter(response, settings.MaxRetryAfter) is { } delay)
        {
            return new ValueTask<TimeSpan?>(delay);
        }

        return previous is null ? new ValueTask<TimeSpan?>((TimeSpan?)null) : previous(args);
    }
}
