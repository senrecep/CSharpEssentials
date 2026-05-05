using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace CSharpEssentials.Http;

public static class HttpClientResilienceExtensions
{
    public static ResiliencePipeline CreateRetryPipeline(int maxRetryAttempts = 3, TimeSpan? delay = null)
    {
        TimeSpan baseDelay = delay ?? TimeSpan.FromSeconds(1);

        return new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = maxRetryAttempts,
                Delay = baseDelay,
                BackoffType = DelayBackoffType.Exponential,
                ShouldHandle = new PredicateBuilder()
                    .Handle<HttpRequestException>()
                    .Handle<IOException>()
                    .Handle<TimeoutRejectedException>()
                    .Handle<TaskCanceledException>(ex => !ex.CancellationToken.IsCancellationRequested)
            })
            .Build();
    }

    public static ResiliencePipeline<Result<T>> CreateRetryPipeline<T>(int maxRetryAttempts = 3, TimeSpan? delay = null)
    {
        TimeSpan baseDelay = delay ?? TimeSpan.FromSeconds(1);

        return new ResiliencePipelineBuilder<Result<T>>()
            .AddRetry(new RetryStrategyOptions<Result<T>>
            {
                MaxRetryAttempts = maxRetryAttempts,
                Delay = baseDelay,
                BackoffType = DelayBackoffType.Exponential,
                ShouldHandle = new PredicateBuilder<Result<T>>()
                    .HandleResult(IsRetryable)
                    .Handle<HttpRequestException>()
                    .Handle<IOException>()
                    .Handle<TimeoutRejectedException>()
                    .Handle<TaskCanceledException>(ex => !ex.CancellationToken.IsCancellationRequested)
            })
            .Build();
    }

    public static ResiliencePipeline CreateTimeoutPipeline(TimeSpan timeout)
    {
        return new ResiliencePipelineBuilder()
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = timeout
            })
            .Build();
    }

    public static ResiliencePipeline CreateCircuitBreakerPipeline(int minimumThroughput = 5, TimeSpan? samplingDuration = null, TimeSpan? breakDuration = null)
    {
        return new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                MinimumThroughput = minimumThroughput,
                SamplingDuration = samplingDuration ?? TimeSpan.FromMinutes(1),
                BreakDuration = breakDuration ?? TimeSpan.FromSeconds(30),
                ShouldHandle = new PredicateBuilder()
                    .Handle<HttpRequestException>()
                    .Handle<IOException>()
                    .Handle<TimeoutRejectedException>()
                    .Handle<TaskCanceledException>(ex => !ex.CancellationToken.IsCancellationRequested)
            })
            .Build();
    }

    public static ResiliencePipeline<Result<T>> CreateCircuitBreakerPipeline<T>(int minimumThroughput = 5, TimeSpan? samplingDuration = null, TimeSpan? breakDuration = null)
    {
        return new ResiliencePipelineBuilder<Result<T>>()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<Result<T>>
            {
                FailureRatio = 0.5,
                MinimumThroughput = minimumThroughput,
                SamplingDuration = samplingDuration ?? TimeSpan.FromMinutes(1),
                BreakDuration = breakDuration ?? TimeSpan.FromSeconds(30),
                ShouldHandle = new PredicateBuilder<Result<T>>()
                    .HandleResult(IsRetryable)
                    .Handle<HttpRequestException>()
                    .Handle<IOException>()
                    .Handle<TimeoutRejectedException>()
                    .Handle<TaskCanceledException>(ex => !ex.CancellationToken.IsCancellationRequested)
            })
            .Build();
    }

    public static ResiliencePipeline CreateResiliencePipeline(int maxRetryAttempts = 3, TimeSpan? timeout = null, TimeSpan? retryDelay = null)
    {
        TimeSpan effectiveTimeout = timeout ?? TimeSpan.FromSeconds(30);
        TimeSpan effectiveDelay = retryDelay ?? TimeSpan.FromSeconds(1);

        return new ResiliencePipelineBuilder()
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = effectiveTimeout
            })
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = maxRetryAttempts,
                Delay = effectiveDelay,
                BackoffType = DelayBackoffType.Exponential,
                ShouldHandle = new PredicateBuilder()
                    .Handle<HttpRequestException>()
                    .Handle<IOException>()
                    .Handle<TimeoutRejectedException>()
                    .Handle<TaskCanceledException>(ex => !ex.CancellationToken.IsCancellationRequested)
            })
            .Build();
    }

    public static ResiliencePipeline<Result<T>> CreateResiliencePipeline<T>(int maxRetryAttempts = 3, TimeSpan? timeout = null, TimeSpan? retryDelay = null)
    {
        TimeSpan effectiveTimeout = timeout ?? TimeSpan.FromSeconds(30);
        TimeSpan effectiveDelay = retryDelay ?? TimeSpan.FromSeconds(1);

        return new ResiliencePipelineBuilder<Result<T>>()
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = effectiveTimeout
            })
            .AddRetry(new RetryStrategyOptions<Result<T>>
            {
                MaxRetryAttempts = maxRetryAttempts,
                Delay = effectiveDelay,
                BackoffType = DelayBackoffType.Exponential,
                ShouldHandle = new PredicateBuilder<Result<T>>()
                    .HandleResult(r => IsRetryable(r))
                    .Handle<HttpRequestException>()
                    .Handle<IOException>()
                    .Handle<TimeoutRejectedException>()
                    .Handle<TaskCanceledException>(ex => !ex.CancellationToken.IsCancellationRequested)
            })
            .Build();
    }

    public static async Task<Result> ExecuteAsResultAsync(
        this ResiliencePipeline pipeline,
        Func<CancellationToken, Task<Result>> callback,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteResilienceAsync(async () =>
            await pipeline.ExecuteAsync(async token => await callback(token), cancellationToken));
    }

    public static Task<Result<T>> ExecuteAsResultAsync<T>(
        this ResiliencePipeline pipeline,
        Func<CancellationToken, Task<Result<T>>> callback,
        CancellationToken cancellationToken = default)
    {
        return Result.TryAsync(
            () => pipeline.ExecuteAsync(async token => await callback(token), cancellationToken).AsTask(),
            HandleException,
            cancellationToken);
    }

    public static Task<Result<T>> ExecuteAsResultAsync<T>(
        this ResiliencePipeline<Result<T>> pipeline,
        Func<CancellationToken, Task<Result<T>>> callback,
        CancellationToken cancellationToken = default)
    {
        return Result.TryAsync(
            () => pipeline.ExecuteAsync(async token => await callback(token), cancellationToken).AsTask(),
            HandleException,
            cancellationToken);
    }

    private static bool IsRetryable<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return false;

        ErrorType type = result.FirstError.Type;
        return type is not ErrorType.Unauthorized
            and not ErrorType.Forbidden
            and not ErrorType.NotFound
            and not ErrorType.Validation;
    }

    private static Error HandleException(Exception ex)
    {
        if (ex is OperationCanceledException oce && oce.CancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(oce.Message, oce, oce.CancellationToken);

        if (ex is BrokenCircuitException)
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();

        return Error.Exception(ex, ErrorType.Unexpected);
    }

    private static async Task<Result> ExecuteResilienceAsync(Func<Task<Result>> action)
    {
        try
        {
            return await action();
        }
        catch (TimeoutRejectedException ex)
        {
            return Error.Exception(ex, ErrorType.Unexpected);
        }
        catch (HttpRequestException ex)
        {
            return Error.Exception(ex, ErrorType.Unexpected);
        }
        catch (IOException ex)
        {
            return Error.Exception(ex, ErrorType.Unexpected);
        }
        catch (TaskCanceledException ex) when (!ex.CancellationToken.IsCancellationRequested)
        {
            return Error.Exception(ex, ErrorType.Unexpected);
        }
        catch (Exception ex) when (ex is not OperationCanceledException and not BrokenCircuitException)
        {
            return Error.Exception(ex, ErrorType.Unexpected);
        }
    }
}
