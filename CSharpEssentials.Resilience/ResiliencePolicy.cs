using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace CSharpEssentials.Resilience;

public readonly partial struct ResiliencePolicy
{
    private readonly ResiliencePipeline _pipeline;

    private ResiliencePolicy(ResiliencePipeline pipeline) =>
        _pipeline = pipeline;

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
        _pipeline;

    public async Task<Result> ExecuteAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _pipeline.ExecuteAsync(async token => await action(token), cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    public async Task<Result<T>> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _pipeline.ExecuteAsync(async token => await action(token), cancellationToken);
        }
        catch (Exception ex)
        {
            return HandleException<T>(ex);
        }
    }

    public async Task<Result<T>> ExecuteAsync<T>(
        Func<CancellationToken, Task<Result<T>>> action,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _pipeline.ExecuteAsync(async token => await action(token), cancellationToken);
        }
        catch (Exception ex)
        {
            return HandleException<T>(ex);
        }
    }

    public async Task<Result> ExecuteAsync(
        Func<CancellationToken, Task<Result>> action,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _pipeline.ExecuteAsync(async token => await action(token), cancellationToken);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    private static Result HandleException(Exception ex)
    {
        if (ex is OperationCanceledException oce && oce.CancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException(oce.Message, oce, oce.CancellationToken);
        }

        if (ex is BrokenCircuitException)
        {
            return Error.Failure("Resilience.CircuitBroken", "Circuit breaker is open.");
        }

        if (ex is TimeoutRejectedException)
        {
            return Error.Failure("Resilience.Timeout", "Operation timed out.");
        }

        return Error.Exception(ex, ErrorType.Unexpected);
    }

    private static Result<T> HandleException<T>(Exception ex)
    {
        if (ex is OperationCanceledException oce && oce.CancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException(oce.Message, oce, oce.CancellationToken);
        }

        if (ex is BrokenCircuitException)
        {
            return Error.Failure("Resilience.CircuitBroken", "Circuit breaker is open.");
        }

        if (ex is TimeoutRejectedException)
        {
            return Error.Failure("Resilience.Timeout", "Operation timed out.");
        }

        return Error.Exception(ex, ErrorType.Unexpected);
    }

    private ResiliencePolicy Merge(ResiliencePipeline additionalPipeline)
    {
        if (_pipeline == ResiliencePipeline.Empty)
        {
            return new(additionalPipeline);
        }

        ResiliencePipeline merged = new ResiliencePipelineBuilder()
            .AddPipeline(_pipeline)
            .AddPipeline(additionalPipeline)
            .Build();

        return new(merged);
    }
}
