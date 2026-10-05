using System.Collections.Concurrent;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.ResultPattern.Interfaces;
using Polly;
using Polly.Retry;

namespace CSharpEssentials.Resilience;

public static class ResilienceResultExtensions
{
    public static async ValueTask<Result<T>> RetryIfFailed<T>(
        this Func<CancellationToken, Task<Result<T>>> operation,
        int maxAttempts = 3,
        TimeSpan? delay = null,
        bool exponentialBackoff = true,
        CancellationToken cancellationToken = default)
    {
        ResiliencePipeline<Result<T>> pipeline = RetryPipelineCache<Result<T>>.Get(maxAttempts, delay, exponentialBackoff);

        Result<T> result;
        try
        {
            result = await pipeline.ExecuteAsync(
                async token => await operation(token),
                cancellationToken);
        }
        catch (Exception ex)
        {
            return ResilienceClassifier.HandleException(ex, cancellationToken);
        }

        // Polly stops retrying once the caller cancels and returns the last outcome; surface that as cancellation.
        return ResilienceClassifier.ThrowIfCallerCancelled(result, cancellationToken);
    }

    public static async ValueTask<Result> RetryIfFailed(
        this Func<CancellationToken, Task<Result>> operation,
        int maxAttempts = 3,
        TimeSpan? delay = null,
        bool exponentialBackoff = true,
        CancellationToken cancellationToken = default)
    {
        ResiliencePipeline<Result> pipeline = RetryPipelineCache<Result>.Get(maxAttempts, delay, exponentialBackoff);

        Result result;
        try
        {
            result = await pipeline.ExecuteAsync(
                async token => await operation(token),
                cancellationToken);
        }
        catch (Exception ex)
        {
            return ResilienceClassifier.HandleException(ex, cancellationToken);
        }

        // Polly stops retrying once the caller cancels and returns the last outcome; surface that as cancellation.
        return ResilienceClassifier.ThrowIfCallerCancelled(result, cancellationToken);
    }

    /// <summary>
    /// Retries <paramref name="operation"/> while <paramref name="shouldRetry"/> returns <see langword="true"/> for its error.
    /// The predicate replaces the default classification. An exception thrown by the operation is converted to the
    /// <see cref="Error"/> that would be returned for it (<see cref="ErrorType.Unexpected"/>) and passed to the
    /// predicate as well. Cancellation of <paramref name="cancellationToken"/> is never retried and throws
    /// <see cref="OperationCanceledException"/>, as in the overload without a predicate.
    /// </summary>
    public static async ValueTask<Result<T>> RetryIfFailed<T>(
        this Func<CancellationToken, Task<Result<T>>> operation,
        Func<Error, bool> shouldRetry,
        int maxAttempts = 3,
        TimeSpan? delay = null,
        bool exponentialBackoff = true,
        CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(shouldRetry);
#else
        if (shouldRetry is null)
            throw new ArgumentNullException(nameof(shouldRetry));
#endif

        ResiliencePipeline<Result<T>> pipeline = BuildPipeline<Result<T>>(maxAttempts, delay, exponentialBackoff, shouldRetry);

        Result<T> result;
        try
        {
            result = await pipeline.ExecuteAsync(
                async token => await operation(token),
                cancellationToken);
        }
        catch (Exception ex)
        {
            return ResilienceClassifier.HandleException(ex, cancellationToken);
        }

        return ResilienceClassifier.ThrowIfCallerCancelled(result, shouldRetry, cancellationToken);
    }

    /// <inheritdoc cref="RetryIfFailed{T}(Func{CancellationToken, Task{Result{T}}}, Func{Error, bool}, int, TimeSpan?, bool, CancellationToken)"/>
    public static async ValueTask<Result> RetryIfFailed(
        this Func<CancellationToken, Task<Result>> operation,
        Func<Error, bool> shouldRetry,
        int maxAttempts = 3,
        TimeSpan? delay = null,
        bool exponentialBackoff = true,
        CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(shouldRetry);
#else
        if (shouldRetry is null)
            throw new ArgumentNullException(nameof(shouldRetry));
#endif

        ResiliencePipeline<Result> pipeline = BuildPipeline<Result>(maxAttempts, delay, exponentialBackoff, shouldRetry);

        Result result;
        try
        {
            result = await pipeline.ExecuteAsync(
                async token => await operation(token),
                cancellationToken);
        }
        catch (Exception ex)
        {
            return ResilienceClassifier.HandleException(ex, cancellationToken);
        }

        return ResilienceClassifier.ThrowIfCallerCancelled(result, shouldRetry, cancellationToken);
    }

    // Predicate pipelines are not cached: a delegate cannot key the cache safely (lambdas that capture state are new
    // instances on every call, so the cache would fill with entries that are never hit). Building a retry pipeline is
    // a few allocations, negligible next to an operation that is worth retrying.
    private static ResiliencePipeline<TResult> BuildPipeline<TResult>(
        int maxAttempts,
        TimeSpan? delay,
        bool exponentialBackoff,
        Func<Error, bool> shouldRetry)
        where TResult : IResultBase =>
        new ResiliencePipelineBuilder<TResult>()
            .AddRetry(new RetryStrategyOptions<TResult>
            {
                MaxRetryAttempts = maxAttempts,
                Delay = delay ?? TimeSpan.FromSeconds(1),
                BackoffType = exponentialBackoff ? DelayBackoffType.Exponential : DelayBackoffType.Constant,
                ShouldHandle = args => new ValueTask<bool>(args.Outcome.Exception is { } ex
                    ? !ResilienceClassifier.IsCallerCancellation(ex, args.Context.CancellationToken)
                        && shouldRetry(ResilienceClassifier.ToError(ex))
                    : args.Outcome.Result is { } result && ResilienceClassifier.IsRetryable(result, shouldRetry))
            })
            .Build();

    private static class RetryPipelineCache<TResult>
        where TResult : IResultBase
    {
        // The cap applies per closed generic type (Result and each Result<T>). The Count check is not atomic,
        // so concurrent callers can overshoot it slightly, which is acceptable for a cache bound.
        private const int MaxEntries = 64;

        private static readonly ConcurrentDictionary<(int MaxAttempts, TimeSpan Delay, bool ExponentialBackoff), ResiliencePipeline<TResult>> Pipelines = new();

        internal static ResiliencePipeline<TResult> Get(int maxAttempts, TimeSpan? delay, bool exponentialBackoff)
        {
            (int MaxAttempts, TimeSpan Delay, bool ExponentialBackoff) key = (maxAttempts, delay ?? TimeSpan.FromSeconds(1), exponentialBackoff);
            if (Pipelines.TryGetValue(key, out ResiliencePipeline<TResult>? cached))
            {
                return cached;
            }

            ResiliencePipeline<TResult> pipeline = new ResiliencePipelineBuilder<TResult>()
                .AddRetry(new RetryStrategyOptions<TResult>
                {
                    MaxRetryAttempts = key.MaxAttempts,
                    Delay = key.Delay,
                    BackoffType = key.ExponentialBackoff ? DelayBackoffType.Exponential : DelayBackoffType.Constant,
                    ShouldHandle = static args => new ValueTask<bool>(args.Outcome.Exception is { } ex
                        ? !ResilienceClassifier.IsCallerCancellation(ex, args.Context.CancellationToken)
                        : args.Outcome.Result is { } result && ResilienceClassifier.IsRetryable(result))
                })
                .Build();

            return Pipelines.Count < MaxEntries ? Pipelines.GetOrAdd(key, pipeline) : pipeline;
        }
    }
}
