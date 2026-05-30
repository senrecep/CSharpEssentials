using CSharpEssentials.Resilience;
using CSharpEssentials.ResultPattern;
using Polly;

namespace CSharpEssentials.Http;

public static class HttpClientResilienceExtensions
{
    // ──────────────────────────────────────────────────────
    //  Legacy API — returns Polly ResiliencePipeline (preserved for backward compatibility)
    // ──────────────────────────────────────────────────────

    public static ResiliencePipeline CreateRetryPipeline(int maxRetryAttempts = 3, TimeSpan? delay = null)
        => ResiliencePolicy.Create().WithRetry(maxRetryAttempts, delay).ToPipeline();

    public static ResiliencePipeline<Result<T>> CreateRetryPipeline<T>(int maxRetryAttempts = 3, TimeSpan? delay = null)
        => ResiliencePolicy<T>.Create().WithRetry(maxRetryAttempts, delay).ToPipeline();

    public static ResiliencePipeline CreateTimeoutPipeline(TimeSpan timeout)
        => ResiliencePolicy.Create().WithTimeout(timeout).ToPipeline();

    public static ResiliencePipeline CreateCircuitBreakerPipeline(int minimumThroughput = 5, TimeSpan? samplingDuration = null, TimeSpan? breakDuration = null)
        => ResiliencePolicy.Create().WithCircuitBreaker(minimumThroughput, samplingDuration, breakDuration).ToPipeline();

    public static ResiliencePipeline<Result<T>> CreateCircuitBreakerPipeline<T>(int minimumThroughput = 5, TimeSpan? samplingDuration = null, TimeSpan? breakDuration = null)
        => ResiliencePolicy<T>.Create().WithCircuitBreaker(minimumThroughput, samplingDuration, breakDuration).ToPipeline();

    public static ResiliencePipeline CreateResiliencePipeline(int maxRetryAttempts = 3, TimeSpan? timeout = null, TimeSpan? retryDelay = null)
    {
        TimeSpan effectiveTimeout = timeout ?? TimeSpan.FromSeconds(30);

        return ResiliencePolicy.Create()
            .WithRetry(maxRetryAttempts, retryDelay)
            .WithTimeout(effectiveTimeout)
            .ToPipeline();
    }

    public static ResiliencePipeline<Result<T>> CreateResiliencePipeline<T>(int maxRetryAttempts = 3, TimeSpan? timeout = null, TimeSpan? retryDelay = null)
    {
        TimeSpan effectiveTimeout = timeout ?? TimeSpan.FromSeconds(30);

        return ResiliencePolicy<T>.Create()
            .WithRetry(maxRetryAttempts, retryDelay)
            .WithTimeout(effectiveTimeout)
            .ToPipeline();
    }

    public static Task<Result> ExecuteAsResultAsync(
        this ResiliencePipeline pipeline,
        Func<CancellationToken, Task<Result>> callback,
        CancellationToken cancellationToken = default)
    {
        return ResiliencePolicy.FromPipeline(pipeline).ExecuteAsync(callback, cancellationToken);
    }

    public static Task<Result<T>> ExecuteAsResultAsync<T>(
        this ResiliencePipeline pipeline,
        Func<CancellationToken, Task<Result<T>>> callback,
        CancellationToken cancellationToken = default)
    {
        return ResiliencePolicy.FromPipeline(pipeline).ExecuteAsync(callback, cancellationToken);
    }

    public static Task<Result<T>> ExecuteAsResultAsync<T>(
        this ResiliencePipeline<Result<T>> pipeline,
        Func<CancellationToken, Task<Result<T>>> callback,
        CancellationToken cancellationToken = default)
    {
        return ResiliencePolicy<T>.FromPipeline(pipeline).ExecuteAsync(callback, cancellationToken);
    }

    // ──────────────────────────────────────────────────────
    //  New API — returns ResiliencePolicy / ResiliencePolicy<T>
    // ──────────────────────────────────────────────────────

    public static ResiliencePolicy CreateRetryPolicy(int maxRetryAttempts = 3, TimeSpan? delay = null)
        => ResiliencePolicy.Create().WithRetry(maxRetryAttempts, delay);

    public static ResiliencePolicy<T> CreateRetryPolicy<T>(int maxRetryAttempts = 3, TimeSpan? delay = null)
        => ResiliencePolicy<T>.Create().WithRetry(maxRetryAttempts, delay);

    public static ResiliencePolicy CreateTimeoutPolicy(TimeSpan timeout)
        => ResiliencePolicy.Create().WithTimeout(timeout);

    public static ResiliencePolicy CreateCircuitBreakerPolicy(int minimumThroughput = 5, TimeSpan? samplingDuration = null, TimeSpan? breakDuration = null)
        => ResiliencePolicy.Create().WithCircuitBreaker(minimumThroughput, samplingDuration, breakDuration);

    public static ResiliencePolicy<T> CreateCircuitBreakerPolicy<T>(int minimumThroughput = 5, TimeSpan? samplingDuration = null, TimeSpan? breakDuration = null)
        => ResiliencePolicy<T>.Create().WithCircuitBreaker(minimumThroughput, samplingDuration, breakDuration);

    public static ResiliencePolicy CreateResiliencePolicy(int maxRetryAttempts = 3, TimeSpan? timeout = null, TimeSpan? retryDelay = null)
    {
        TimeSpan effectiveTimeout = timeout ?? TimeSpan.FromSeconds(30);

        return ResiliencePolicy.Create()
            .WithRetry(maxRetryAttempts, retryDelay)
            .WithTimeout(effectiveTimeout);
    }

    public static ResiliencePolicy<T> CreateResiliencePolicy<T>(int maxRetryAttempts = 3, TimeSpan? timeout = null, TimeSpan? retryDelay = null)
    {
        TimeSpan effectiveTimeout = timeout ?? TimeSpan.FromSeconds(30);

        return ResiliencePolicy<T>.Create()
            .WithRetry(maxRetryAttempts, retryDelay)
            .WithTimeout(effectiveTimeout);
    }
}
