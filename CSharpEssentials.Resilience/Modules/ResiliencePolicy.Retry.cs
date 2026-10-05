using CSharpEssentials.ResultPattern;
using CSharpEssentials.ResultPattern.Interfaces;
using Polly;
using Polly.Retry;

namespace CSharpEssentials.Resilience;

public readonly partial struct ResiliencePolicy
{
    public ResiliencePolicy WithRetry(int maxAttempts = 3, TimeSpan? delay = null, bool exponentialBackoff = true)
    {
        TimeSpan effectiveDelay = delay ?? TimeSpan.FromSeconds(1);

        ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = maxAttempts,
                Delay = effectiveDelay,
                BackoffType = exponentialBackoff ? DelayBackoffType.Exponential : DelayBackoffType.Constant,
                ShouldHandle = static args => new ValueTask<bool>(args.Outcome.Exception is { } ex
                    ? !ResilienceClassifier.IsCallerCancellation(ex, args.Context.CancellationToken)
                    : args.Outcome.Result is IResultBase result && ResilienceClassifier.IsRetryable(result))
            })
            .Build();

        return Merge(pipeline);
    }

    public ResiliencePolicy WithRetry(RetryOptions options)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(options);
#else
        if (options is null)
            throw new ArgumentNullException(nameof(options));
#endif

        return WithRetry(options.MaxAttempts, options.Delay, options.ExponentialBackoff);
    }
}

public readonly partial struct ResiliencePolicy<T>
{
    public ResiliencePolicy<T> WithRetry(int maxAttempts = 3, TimeSpan? delay = null, bool exponentialBackoff = true)
    {
        TimeSpan effectiveDelay = delay ?? TimeSpan.FromSeconds(1);

        ResiliencePipeline<Result<T>> pipeline = new ResiliencePipelineBuilder<Result<T>>()
            .AddRetry(new RetryStrategyOptions<Result<T>>
            {
                MaxRetryAttempts = maxAttempts,
                Delay = effectiveDelay,
                BackoffType = exponentialBackoff ? DelayBackoffType.Exponential : DelayBackoffType.Constant,
                ShouldHandle = static args => new ValueTask<bool>(args.Outcome.Exception is { } ex
                    ? !ResilienceClassifier.IsCallerCancellation(ex, args.Context.CancellationToken)
                    : ResilienceClassifier.IsRetryable(args.Outcome.Result))
            })
            .Build();

        return Merge(pipeline);
    }

    public ResiliencePolicy<T> WithRetry(RetryOptions options)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(options);
#else
        if (options is null)
            throw new ArgumentNullException(nameof(options));
#endif

        return WithRetry(options.MaxAttempts, options.Delay, options.ExponentialBackoff);
    }
}
