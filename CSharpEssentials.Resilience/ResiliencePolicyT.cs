using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace CSharpEssentials.Resilience;

public readonly partial struct ResiliencePolicy<T>
{
    private readonly ResiliencePipeline<Result<T>> _pipeline;

    private ResiliencePolicy(ResiliencePipeline<Result<T>> pipeline) =>
        _pipeline = pipeline;

    public static ResiliencePolicy<T> Create() =>
        new(new ResiliencePipelineBuilder<Result<T>>().Build());

    public static ResiliencePolicy<T> Create(Action<ResiliencePipelineBuilder<Result<T>>> configure)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(configure);
#else
        if (configure is null)
            throw new ArgumentNullException(nameof(configure));
#endif

        ResiliencePipelineBuilder<Result<T>> builder = new();
        configure(builder);
        return new(builder.Build());
    }

    public ResiliencePipeline<Result<T>> ToPipeline() =>
        _pipeline;

    public async Task<Result<T>> ExecuteAsync(
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _pipeline.ExecuteAsync(
                async token =>
                {
                    T value = await action(token);
                    return Result<T>.Success(value);
                },
                cancellationToken);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    public async Task<Result<T>> ExecuteAsync(
        Func<CancellationToken, Task<Result<T>>> action,
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

    private static Result<T> HandleException(Exception ex)
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

    private ResiliencePolicy<T> Merge(ResiliencePipeline<Result<T>> additionalPipeline)
    {
        ResiliencePipeline<Result<T>> merged = new ResiliencePipelineBuilder<Result<T>>()
            .AddPipeline(_pipeline)
            .AddPipeline(additionalPipeline)
            .Build();

        return new(merged);
    }

    private static bool IsRetryable(Result<T> result)
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
