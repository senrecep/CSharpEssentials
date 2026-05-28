using CSharpEssentials.Resilience;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Http;

public static class HttpClientResilienceExtensions
{
    public static ResiliencePolicy CreateRetryPipeline(int maxRetryAttempts = 3, TimeSpan? delay = null)
        => ResiliencePolicy.Create().WithRetry(maxRetryAttempts, delay);

    public static ResiliencePolicy<T> CreateRetryPipeline<T>(int maxRetryAttempts = 3, TimeSpan? delay = null)
        => ResiliencePolicy<T>.Create().WithRetry(maxRetryAttempts, delay);

    public static ResiliencePolicy CreateTimeoutPipeline(TimeSpan timeout)
        => ResiliencePolicy.Create().WithTimeout(timeout);

    public static ResiliencePolicy CreateCircuitBreakerPipeline(int minimumThroughput = 5, TimeSpan? samplingDuration = null, TimeSpan? breakDuration = null)
        => ResiliencePolicy.Create().WithCircuitBreaker(minimumThroughput, samplingDuration, breakDuration);

    public static ResiliencePolicy<T> CreateCircuitBreakerPipeline<T>(int minimumThroughput = 5, TimeSpan? samplingDuration = null, TimeSpan? breakDuration = null)
        => ResiliencePolicy<T>.Create().WithCircuitBreaker(minimumThroughput, samplingDuration, breakDuration);

    public static ResiliencePolicy CreateResiliencePipeline(int maxRetryAttempts = 3, TimeSpan? timeout = null, TimeSpan? retryDelay = null)
    {
        ResiliencePolicy policy = ResiliencePolicy.Create()
            .WithRetry(maxRetryAttempts, retryDelay);

        if (timeout.HasValue)
        {
            policy = policy.WithTimeout(timeout.Value);
        }

        return policy;
    }

    public static ResiliencePolicy<T> CreateResiliencePipeline<T>(int maxRetryAttempts = 3, TimeSpan? timeout = null, TimeSpan? retryDelay = null)
    {
        ResiliencePolicy<T> policy = ResiliencePolicy<T>.Create()
            .WithRetry(maxRetryAttempts, retryDelay);

        if (timeout.HasValue)
        {
            policy = policy.WithTimeout(timeout.Value);
        }

        return policy;
    }

    public static Task<Result> ExecuteAsResultAsync(
        this ResiliencePolicy policy,
        Func<CancellationToken, Task<Result>> callback,
        CancellationToken cancellationToken = default)
    {
        return policy.ExecuteAsync(callback, cancellationToken);
    }

    public static Task<Result<T>> ExecuteAsResultAsync<T>(
        this ResiliencePolicy policy,
        Func<CancellationToken, Task<Result<T>>> callback,
        CancellationToken cancellationToken = default)
    {
        return policy.ExecuteAsync(callback, cancellationToken);
    }

    public static Task<Result<T>> ExecuteAsResultAsync<T>(
        this ResiliencePolicy<T> policy,
        Func<CancellationToken, Task<Result<T>>> callback,
        CancellationToken cancellationToken = default)
    {
        return policy.ExecuteAsync(callback, cancellationToken);
    }
}
