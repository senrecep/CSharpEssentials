#pragma warning disable IDE0390
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
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
        TimeSpan effectiveDelay = delay ?? TimeSpan.FromSeconds(1);

        ResiliencePipeline<Result<T>> pipeline = new ResiliencePipelineBuilder<Result<T>>()
            .AddRetry(new RetryStrategyOptions<Result<T>>
            {
                MaxRetryAttempts = maxAttempts,
                Delay = effectiveDelay,
                BackoffType = exponentialBackoff ? DelayBackoffType.Exponential : DelayBackoffType.Constant,
                ShouldHandle = new PredicateBuilder<Result<T>>()
                    .HandleResult(r => IsRetryable(r))
            })
            .Build();

        return await pipeline.ExecuteAsync(
            async token => await operation(token),
            cancellationToken);
    }

    public static async ValueTask<Result> RetryIfFailed(
        this Func<CancellationToken, Task<Result>> operation,
        int maxAttempts = 3,
        TimeSpan? delay = null,
        bool exponentialBackoff = true,
        CancellationToken cancellationToken = default)
    {
        TimeSpan effectiveDelay = delay ?? TimeSpan.FromSeconds(1);

        ResiliencePipeline<Result> pipeline = new ResiliencePipelineBuilder<Result>()
            .AddRetry(new RetryStrategyOptions<Result>
            {
                MaxRetryAttempts = maxAttempts,
                Delay = effectiveDelay,
                BackoffType = exponentialBackoff ? DelayBackoffType.Exponential : DelayBackoffType.Constant,
                ShouldHandle = new PredicateBuilder<Result>()
                    .HandleResult(r => r.IsFailure)
            })
            .Build();

        return await pipeline.ExecuteAsync(
            async token => await operation(token),
            cancellationToken);
    }

    private static bool IsRetryable<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return false;
        }

        ErrorType type = result.FirstError.Type;
        return type is not ErrorType.Unauthorized
            and not ErrorType.Forbidden
            and not ErrorType.NotFound
            and not ErrorType.Validation;
    }
}
