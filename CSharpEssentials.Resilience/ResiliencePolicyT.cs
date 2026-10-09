using CSharpEssentials.ResultPattern;
using Polly;

namespace CSharpEssentials.Resilience;

public readonly partial struct ResiliencePolicy<T>
{
    private readonly ResiliencePipeline<Result<T>> _pipeline;

    private ResiliencePolicy(ResiliencePipeline<Result<T>> pipeline) =>
        _pipeline = pipeline;

    private ResiliencePipeline<Result<T>> GetPipeline() => _pipeline ?? ResiliencePipeline<Result<T>>.Empty;

    public static ResiliencePolicy<T> Create() =>
        new(new ResiliencePipelineBuilder<Result<T>>().Build());

    public static ResiliencePolicy<T> FromPipeline(ResiliencePipeline<Result<T>> pipeline) =>
        new(pipeline);

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
        GetPipeline();

    public async Task<Result<T>> ExecuteAsync(
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        ResiliencePipeline<Result<T>> pipeline = GetPipeline();
        Result<T> result;
        try
        {
            result = await pipeline.ExecuteAsync(
                async token =>
                {
                    T value = await action(token).ConfigureAwait(false);
                    return Result<T>.Success(value);
                },
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return ResilienceClassifier.HandleException(ex, cancellationToken);
        }

        return ResilienceClassifier.ThrowIfCallerCancelled(result, cancellationToken);
    }

    public async Task<Result<T>> ExecuteAsync(
        Func<CancellationToken, Task<Result<T>>> action,
        CancellationToken cancellationToken = default)
    {
        ResiliencePipeline<Result<T>> pipeline = GetPipeline();
        Result<T> result;
        try
        {
            result = await pipeline.ExecuteAsync(async token => await action(token).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return ResilienceClassifier.HandleException(ex, cancellationToken);
        }

        return ResilienceClassifier.ThrowIfCallerCancelled(result, cancellationToken);
    }

    private ResiliencePolicy<T> Merge(ResiliencePipeline<Result<T>> additionalPipeline)
    {
        ResiliencePipeline<Result<T>> existing = GetPipeline();
        ResiliencePipeline<Result<T>> merged = new ResiliencePipelineBuilder<Result<T>>()
            .AddPipeline(existing)
            .AddPipeline(additionalPipeline)
            .Build();

        return new(merged);
    }
}
