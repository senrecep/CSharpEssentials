using CSharpEssentials.ResultPattern;
using Polly;
using Polly.Fallback;

namespace CSharpEssentials.Resilience;

public readonly partial struct ResiliencePolicy<T>
{
    public ResiliencePolicy<T> WithFallback(Func<CancellationToken, Task<T>> fallbackAsync)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(fallbackAsync);
#else
        if (fallbackAsync is null)
            throw new ArgumentNullException(nameof(fallbackAsync));
#endif

        ResiliencePipeline<Result<T>> pipeline = new ResiliencePipelineBuilder<Result<T>>()
            .AddFallback(new FallbackStrategyOptions<Result<T>>
            {
                ShouldHandle = new PredicateBuilder<Result<T>>()
                    .HandleResult(static result => result.IsFailure)
                    .Handle<Exception>(),
                FallbackAction = async args =>
                {
                    T fallbackValue = await fallbackAsync(args.Context.CancellationToken);
                    return Outcome.FromResult(Result<T>.Success(fallbackValue));
                }
            })
            .Build();

        ResiliencePipeline<Result<T>> merged = new ResiliencePipelineBuilder<Result<T>>()
            .AddPipeline(pipeline)
            .AddPipeline(_pipeline)
            .Build();

        return new(merged);
    }

    public ResiliencePolicy<T> WithFallback(Func<CancellationToken, Task<Result<T>>> fallbackAsync)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(fallbackAsync);
#else
        if (fallbackAsync is null)
            throw new ArgumentNullException(nameof(fallbackAsync));
#endif

        ResiliencePipeline<Result<T>> pipeline = new ResiliencePipelineBuilder<Result<T>>()
            .AddFallback(new FallbackStrategyOptions<Result<T>>
            {
                ShouldHandle = new PredicateBuilder<Result<T>>()
                    .HandleResult(static result => result.IsFailure)
                    .Handle<Exception>(),
                FallbackAction = async args =>
                {
                    Result<T> fallbackResult = await fallbackAsync(args.Context.CancellationToken);
                    return Outcome.FromResult(fallbackResult);
                }
            })
            .Build();

        ResiliencePipeline<Result<T>> merged = new ResiliencePipelineBuilder<Result<T>>()
            .AddPipeline(pipeline)
            .AddPipeline(_pipeline)
            .Build();

        return new(merged);
    }
}
