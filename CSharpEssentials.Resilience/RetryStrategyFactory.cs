using Polly;
using Polly.Retry;

namespace CSharpEssentials.Resilience;

/// <summary>Builds the Polly retry options for <c>WithRetry</c> and <c>RetryIfFailed</c> from one place.</summary>
internal static class RetryStrategyFactory
{
    internal static TOptions Create<TOptions, TResult>(
        int maxAttempts,
        TimeSpan delay,
        bool exponentialBackoff,
        bool useJitter,
        TimeSpan? maxDelay,
        Func<RetryPredicateArguments<TResult>, ValueTask<bool>> shouldHandle)
        where TOptions : RetryStrategyOptions<TResult>, new() =>
        new()
        {
            MaxRetryAttempts = maxAttempts,
            Delay = delay,
            BackoffType = exponentialBackoff ? DelayBackoffType.Exponential : DelayBackoffType.Constant,
            UseJitter = useJitter,
            MaxDelay = maxDelay,
            ShouldHandle = shouldHandle
        };
}
