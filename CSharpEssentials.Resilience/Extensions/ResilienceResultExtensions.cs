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

        try
        {
            return await pipeline.ExecuteAsync(
                async token => await operation(token),
                cancellationToken);
        }
        catch (Exception ex)
        {
            return ResilienceClassifier.HandleException(ex);
        }
    }

    public static async ValueTask<Result> RetryIfFailed(
        this Func<CancellationToken, Task<Result>> operation,
        int maxAttempts = 3,
        TimeSpan? delay = null,
        bool exponentialBackoff = true,
        CancellationToken cancellationToken = default)
    {
        ResiliencePipeline<Result> pipeline = RetryPipelineCache<Result>.Get(maxAttempts, delay, exponentialBackoff);

        try
        {
            return await pipeline.ExecuteAsync(
                async token => await operation(token),
                cancellationToken);
        }
        catch (Exception ex)
        {
            return ResilienceClassifier.HandleException(ex);
        }
    }

    private static class RetryPipelineCache<TResult>
        where TResult : IResultBase
    {
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
                    ShouldHandle = new PredicateBuilder<TResult>()
                        .HandleResult(ResilienceClassifier.IsRetryable)
                        .Handle<Exception>(static ex => !ResilienceClassifier.IsCancellation(ex))
                })
                .Build();

            return Pipelines.Count < MaxEntries ? Pipelines.GetOrAdd(key, pipeline) : pipeline;
        }
    }
}
