using CSharpEssentials.ResultPattern;
using Polly;

namespace CSharpEssentials.Resilience;

public readonly partial struct ResiliencePolicy
{
    private readonly ResiliencePipeline _pipeline;

    private ResiliencePolicy(ResiliencePipeline pipeline) =>
        _pipeline = pipeline;

    private ResiliencePipeline GetPipeline() => _pipeline ?? ResiliencePipeline.Empty;

    public static ResiliencePolicy Create() =>
        new(ResiliencePipeline.Empty);

    public static ResiliencePolicy FromPipeline(ResiliencePipeline pipeline) =>
        new(pipeline);

    public static ResiliencePolicy Create(ResiliencePolicyOptions options)
    {
        ResiliencePolicy policy = Create();

        if (options.Retry is not null)
        {
            policy = policy.WithRetry(options.Retry.MaxAttempts, options.Retry.Delay, options.Retry.ExponentialBackoff);
        }

        if (options.CircuitBreaker is not null)
        {
            policy = policy.WithCircuitBreaker(
                options.CircuitBreaker.MinimumThroughput,
                options.CircuitBreaker.SamplingDuration,
                options.CircuitBreaker.BreakDuration,
                options.CircuitBreaker.FailureRatio);
        }

        if (options.Timeout is not null)
        {
            policy = policy.WithTimeout(options.Timeout.Timeout);
        }

        return policy;
    }

    public static ResiliencePolicy Create(Action<ResiliencePipelineBuilder> configure)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(configure);
#else
        if (configure is null)
            throw new ArgumentNullException(nameof(configure));
#endif

        ResiliencePipelineBuilder builder = new();
        configure(builder);
        return new(builder.Build());
    }

    public ResiliencePipeline ToPipeline() =>
        GetPipeline();

    public async Task<Result> ExecuteAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        ResiliencePipeline pipeline = GetPipeline();
        try
        {
            await pipeline.ExecuteAsync(async token => await action(token), cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return ResilienceClassifier.HandleException(ex);
        }
    }

    public async Task<Result<T>> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        ResiliencePipeline pipeline = GetPipeline();
        try
        {
            return await pipeline.ExecuteAsync(async token => await action(token), cancellationToken);
        }
        catch (Exception ex)
        {
            return ResilienceClassifier.HandleException(ex);
        }
    }

    public async Task<Result<T>> ExecuteAsync<T>(
        Func<CancellationToken, Task<Result<T>>> action,
        CancellationToken cancellationToken = default)
    {
        ResiliencePipeline pipeline = GetPipeline();
        try
        {
            return await pipeline.ExecuteAsync(async token => await action(token), cancellationToken);
        }
        catch (Exception ex)
        {
            return ResilienceClassifier.HandleException(ex);
        }
    }

    public async Task<Result> ExecuteAsync(
        Func<CancellationToken, Task<Result>> action,
        CancellationToken cancellationToken = default)
    {
        ResiliencePipeline pipeline = GetPipeline();
        try
        {
            return await pipeline.ExecuteAsync(async token => await action(token), cancellationToken);
        }
        catch (Exception ex)
        {
            return ResilienceClassifier.HandleException(ex);
        }
    }

    private ResiliencePolicy Merge(ResiliencePipeline additionalPipeline)
    {
        ResiliencePipeline existing = GetPipeline();
        if (existing == ResiliencePipeline.Empty)
        {
            return new(additionalPipeline);
        }

        ResiliencePipeline merged = new ResiliencePipelineBuilder()
            .AddPipeline(existing)
            .AddPipeline(additionalPipeline)
            .Build();

        return new(merged);
    }
}
