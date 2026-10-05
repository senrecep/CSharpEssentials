using CSharpEssentials.ResultPattern;
using Polly;

namespace CSharpEssentials.Resilience;

public readonly partial struct ResiliencePolicy<T>
{
    private readonly ResiliencePipeline<Result<T>> _pipeline;

    private ResiliencePolicy(ResiliencePipeline<Result<T>> pipeline) =>
        _pipeline = pipeline;

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
        _pipeline;

    public async Task<Result<T>> ExecuteAsync(
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        ResiliencePipeline<Result<T>> pipeline = _pipeline ?? new ResiliencePipelineBuilder<Result<T>>().Build();
        try
        {
            return await pipeline.ExecuteAsync(
                async token =>
                {
                    T value = await action(token);
                    return Result<T>.Success(value);
                },
                cancellationToken);
        }
        catch (Exception ex)
        {
            return ResilienceClassifier.HandleException(ex);
        }
    }

    public async Task<Result<T>> ExecuteAsync(
        Func<CancellationToken, Task<Result<T>>> action,
        CancellationToken cancellationToken = default)
    {
        ResiliencePipeline<Result<T>> pipeline = _pipeline ?? new ResiliencePipelineBuilder<Result<T>>().Build();
        try
        {
            return await pipeline.ExecuteAsync(async token => await action(token), cancellationToken);
        }
        catch (Exception ex)
        {
            return ResilienceClassifier.HandleException(ex);
        }
    }

    private ResiliencePolicy<T> Merge(ResiliencePipeline<Result<T>> additionalPipeline)
    {
        ResiliencePipeline<Result<T>> existing = _pipeline ?? new ResiliencePipelineBuilder<Result<T>>().Build();
        ResiliencePipeline<Result<T>> merged = new ResiliencePipelineBuilder<Result<T>>()
            .AddPipeline(existing)
            .AddPipeline(additionalPipeline)
            .Build();

        return new(merged);
    }
}
