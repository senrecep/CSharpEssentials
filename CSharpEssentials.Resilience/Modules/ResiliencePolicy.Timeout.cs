using CSharpEssentials.ResultPattern;
using Polly;
using Polly.Timeout;

namespace CSharpEssentials.Resilience;

public readonly partial struct ResiliencePolicy
{
    public ResiliencePolicy WithTimeout(TimeSpan timeout)
    {
        ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = timeout
            })
            .Build();

        return Merge(pipeline);
    }

    public ResiliencePolicy WithTimeout(TimeoutOptions options)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(options);
#else
        if (options is null)
            throw new ArgumentNullException(nameof(options));
#endif

        return WithTimeout(options.Timeout);
    }
}

public readonly partial struct ResiliencePolicy<T>
{
    public ResiliencePolicy<T> WithTimeout(TimeSpan timeout)
    {
        ResiliencePipeline<Result<T>> pipeline = new ResiliencePipelineBuilder<Result<T>>()
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = timeout
            })
            .Build();

        return Merge(pipeline);
    }

    public ResiliencePolicy<T> WithTimeout(TimeoutOptions options)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(options);
#else
        if (options is null)
            throw new ArgumentNullException(nameof(options));
#endif

        return WithTimeout(options.Timeout);
    }
}
