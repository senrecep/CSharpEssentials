using System.Collections.Concurrent;
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
        if (cancellationToken.IsCancellationRequested && ResilienceClassifier.IsRetryable(result))
        {
            throw new OperationCanceledException(cancellationToken);
        }

        return result;
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
        if (cancellationToken.IsCancellationRequested && ResilienceClassifier.IsRetryable(result))
        {
            throw new OperationCanceledException(cancellationToken);
        }

        return result;
    }

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
