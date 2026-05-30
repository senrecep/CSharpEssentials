using CSharpEssentials.ResultPattern;
using Polly;
using Polly.CircuitBreaker;

namespace CSharpEssentials.Resilience;

public readonly partial struct ResiliencePolicy
{
    public ResiliencePolicy WithCircuitBreaker(
        int minimumThroughput = 10,
        TimeSpan? samplingDuration = null,
        TimeSpan? breakDuration = null,
        double failureRatio = 0.5)
    {
        ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = failureRatio,
                MinimumThroughput = minimumThroughput,
                SamplingDuration = samplingDuration ?? TimeSpan.FromMinutes(1),
                BreakDuration = breakDuration ?? TimeSpan.FromSeconds(30),
                ShouldHandle = new PredicateBuilder().Handle<Exception>()
            })
            .Build();

        return Merge(pipeline);
    }

    public ResiliencePolicy WithCircuitBreaker(CircuitBreakerOptions options)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(options);
#else
        if (options is null)
            throw new ArgumentNullException(nameof(options));
#endif

        return WithCircuitBreaker(
            options.MinimumThroughput,
            options.SamplingDuration,
            options.BreakDuration,
            options.FailureRatio);
    }
}

public readonly partial struct ResiliencePolicy<T>
{
    public ResiliencePolicy<T> WithCircuitBreaker(
        int minimumThroughput = 10,
        TimeSpan? samplingDuration = null,
        TimeSpan? breakDuration = null,
        double failureRatio = 0.5)
    {
        ResiliencePipeline<Result<T>> pipeline = new ResiliencePipelineBuilder<Result<T>>()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<Result<T>>
            {
                FailureRatio = failureRatio,
                MinimumThroughput = minimumThroughput,
                SamplingDuration = samplingDuration ?? TimeSpan.FromMinutes(1),
                BreakDuration = breakDuration ?? TimeSpan.FromSeconds(30),
                ShouldHandle = new PredicateBuilder<Result<T>>()
                    .HandleResult(IsRetryable)
                    .Handle<Exception>()
            })
            .Build();

        return Merge(pipeline);
    }

    public ResiliencePolicy<T> WithCircuitBreaker(CircuitBreakerOptions options)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(options);
#else
        if (options is null)
            throw new ArgumentNullException(nameof(options));
#endif

        return WithCircuitBreaker(
            options.MinimumThroughput,
            options.SamplingDuration,
            options.BreakDuration,
            options.FailureRatio);
    }
}
