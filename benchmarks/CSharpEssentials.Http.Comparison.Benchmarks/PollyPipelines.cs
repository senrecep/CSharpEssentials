using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace CSharpEssentials.Http.Comparison.Benchmarks;

/// <summary>The bare Polly pipelines, built with the same values as the CSE policies they are compared with.</summary>
internal static class PollyPipelines
{
    public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

    public static ResiliencePipeline Retry() =>
        new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions { MaxRetryAttempts = 3, Delay = TimeSpan.Zero, BackoffType = DelayBackoffType.Exponential })
            .Build();

    public static ResiliencePipeline RetryCircuitBreakerTimeout() =>
        new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions { MaxRetryAttempts = 3, Delay = TimeSpan.Zero, BackoffType = DelayBackoffType.Exponential })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                MinimumThroughput = 10,
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromMinutes(1),
                BreakDuration = TimeSpan.FromSeconds(30),
            })
            .AddTimeout(Timeout)
            .Build();

    public static ResiliencePipeline<Result<int>> RetryResult() =>
        new ResiliencePipelineBuilder<Result<int>>()
            .AddRetry(new RetryStrategyOptions<Result<int>>
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.Zero,
                BackoffType = DelayBackoffType.Exponential,
                ShouldHandle = static args => new ValueTask<bool>(Transient.IsTransient(args.Outcome)),
            })
            .Build();

    public static ResiliencePipeline<Result<int>> RetryCircuitBreakerTimeoutResult() =>
        new ResiliencePipelineBuilder<Result<int>>()
            .AddRetry(new RetryStrategyOptions<Result<int>>
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.Zero,
                BackoffType = DelayBackoffType.Exponential,
                ShouldHandle = static args => new ValueTask<bool>(Transient.IsTransient(args.Outcome)),
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<Result<int>>
            {
                MinimumThroughput = 10,
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromMinutes(1),
                BreakDuration = TimeSpan.FromSeconds(30),
                ShouldHandle = static args => new ValueTask<bool>(Transient.IsTransient(args.Outcome)),
            })
            .AddTimeout(Timeout)
            .Build();
}
