using System.Collections.Concurrent;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.ResultPattern.Interfaces;
using Polly;
using Polly.Retry;

namespace CSharpEssentials.Resilience;

public static class ResilienceResultExtensions
{
    private static readonly TimeSpan DefaultDelay = TimeSpan.FromSeconds(1);

    private static readonly ResiliencePropertyKey<Func<Error, bool>> ShouldRetryKey = new("CSharpEssentials.Resilience.ShouldRetry");

    /// <summary>
    /// Retries <paramref name="operation"/> while it returns a retryable failure or throws. Failures of type
    /// <see cref="ErrorType.Unauthorized"/>, <see cref="ErrorType.Forbidden"/>, <see cref="ErrorType.NotFound"/> and
    /// <see cref="ErrorType.Validation"/> are not retried. Cancellation of <paramref name="cancellationToken"/> is never
    /// retried and throws <see cref="OperationCanceledException"/>.
    /// </summary>
    /// <param name="operation">The operation to run.</param>
    /// <param name="maxAttempts">Number of retries after the first attempt, so the operation runs at most
    /// <c>maxAttempts + 1</c> times.</param>
    /// <param name="delay">Delay before the first retry. Defaults to one second.</param>
    /// <param name="exponentialBackoff">When <see langword="true"/>, the delay doubles after each retry.</param>
    /// <param name="cancellationToken">Caller token. Cancelling it stops retrying and throws
    /// <see cref="OperationCanceledException"/>.</param>
    public static async ValueTask<Result<T>> RetryIfFailed<T>(
        this Func<CancellationToken, Task<Result<T>>> operation,
        int maxAttempts = 3,
        TimeSpan? delay = null,
        bool exponentialBackoff = true,
        CancellationToken cancellationToken = default)
    {
        ResiliencePipeline<Result<T>> pipeline = RetryPipelineCache<Result<T>>.Get(maxAttempts, delay, exponentialBackoff, usesPredicate: false);

        Result<T> result;
        try
        {
            result = await pipeline.ExecuteAsync(
                async token => await operation(token).ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return ResilienceClassifier.HandleException(ex, cancellationToken);
        }

        // Polly stops retrying once the caller cancels and returns the last outcome; surface that as cancellation.
        return ResilienceClassifier.ThrowIfCallerCancelled(result, cancellationToken);
    }

    /// <inheritdoc cref="RetryIfFailed{T}(Func{CancellationToken, Task{Result{T}}}, int, TimeSpan?, bool, CancellationToken)"/>
    public static async ValueTask<Result> RetryIfFailed(
        this Func<CancellationToken, Task<Result>> operation,
        int maxAttempts = 3,
        TimeSpan? delay = null,
        bool exponentialBackoff = true,
        CancellationToken cancellationToken = default)
    {
        ResiliencePipeline<Result> pipeline = RetryPipelineCache<Result>.Get(maxAttempts, delay, exponentialBackoff, usesPredicate: false);

        Result result;
        try
        {
            result = await pipeline.ExecuteAsync(
                async token => await operation(token).ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return ResilienceClassifier.HandleException(ex, cancellationToken);
        }

        // Polly stops retrying once the caller cancels and returns the last outcome; surface that as cancellation.
        return ResilienceClassifier.ThrowIfCallerCancelled(result, cancellationToken);
    }

    /// <summary>
    /// Retries <paramref name="operation"/> while <paramref name="shouldRetry"/> returns <see langword="true"/> for its error.
    /// The predicate replaces the default classification. An exception thrown by the operation is converted to the
    /// <see cref="Error"/> that would be returned for it (<see cref="ErrorType.Unexpected"/>) and passed to the
    /// predicate as well. Cancellation of <paramref name="cancellationToken"/> is never retried and throws
    /// <see cref="OperationCanceledException"/>, as in the overload without a predicate.
    /// </summary>
    /// <remarks>
    /// The predicate can be called more than once for the same attempt (for example, again after the last attempt
    /// when the caller has cancelled), so keep it pure and cheap. If the predicate throws while an attempt is being
    /// classified, retrying stops and the exception is returned as an <see cref="ErrorType.Unexpected"/> error.
    /// </remarks>
    /// <param name="operation">The operation to run.</param>
    /// <param name="shouldRetry">Returns <see langword="true"/> for errors that should be retried.</param>
    /// <param name="maxAttempts">Number of retries after the first attempt, so the operation runs at most
    /// <c>maxAttempts + 1</c> times.</param>
    /// <param name="delay">Delay before the first retry. Defaults to one second.</param>
    /// <param name="exponentialBackoff">When <see langword="true"/>, the delay doubles after each retry.</param>
    /// <param name="cancellationToken">Caller token. Cancelling it stops retrying and throws
    /// <see cref="OperationCanceledException"/>.</param>
    public static async ValueTask<Result<T>> RetryIfFailed<T>(
        this Func<CancellationToken, Task<Result<T>>> operation,
        Func<Error, bool> shouldRetry,
        int maxAttempts = 3,
        TimeSpan? delay = null,
        bool exponentialBackoff = true,
        CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(shouldRetry);
#else
        if (shouldRetry is null)
            throw new ArgumentNullException(nameof(shouldRetry));
#endif

        ResiliencePipeline<Result<T>> pipeline = RetryPipelineCache<Result<T>>.Get(maxAttempts, delay, exponentialBackoff, usesPredicate: true);
        ResilienceContext context = ResilienceContextPool.Shared.Get(cancellationToken);
        context.Properties.Set(ShouldRetryKey, shouldRetry);

        Result<T> result;
        try
        {
            result = await pipeline.ExecuteAsync(
                static async (ctx, op) => await op(ctx.CancellationToken).ConfigureAwait(false),
                context,
                operation).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return ResilienceClassifier.HandleException(ex, cancellationToken);
        }
        finally
        {
            ResilienceContextPool.Shared.Return(context);
        }

        return ResilienceClassifier.ThrowIfCallerCancelled(result, shouldRetry, cancellationToken);
    }

    /// <inheritdoc cref="RetryIfFailed{T}(Func{CancellationToken, Task{Result{T}}}, Func{Error, bool}, int, TimeSpan?, bool, CancellationToken)"/>
    public static async ValueTask<Result> RetryIfFailed(
        this Func<CancellationToken, Task<Result>> operation,
        Func<Error, bool> shouldRetry,
        int maxAttempts = 3,
        TimeSpan? delay = null,
        bool exponentialBackoff = true,
        CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(shouldRetry);
#else
        if (shouldRetry is null)
            throw new ArgumentNullException(nameof(shouldRetry));
#endif

        ResiliencePipeline<Result> pipeline = RetryPipelineCache<Result>.Get(maxAttempts, delay, exponentialBackoff, usesPredicate: true);
        ResilienceContext context = ResilienceContextPool.Shared.Get(cancellationToken);
        context.Properties.Set(ShouldRetryKey, shouldRetry);

        Result result;
        try
        {
            result = await pipeline.ExecuteAsync(
                static async (ctx, op) => await op(ctx.CancellationToken).ConfigureAwait(false),
                context,
                operation).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return ResilienceClassifier.HandleException(ex, cancellationToken);
        }
        finally
        {
            ResilienceContextPool.Shared.Return(context);
        }

        return ResilienceClassifier.ThrowIfCallerCancelled(result, shouldRetry, cancellationToken);
    }

    private static ResiliencePipeline<TResult> BuildRetryPipeline<TResult>(
        int maxAttempts,
        TimeSpan delay,
        bool exponentialBackoff,
        Func<RetryPredicateArguments<TResult>, ValueTask<bool>> shouldHandle) =>
        new ResiliencePipelineBuilder<TResult>()
            .AddRetry(new RetryStrategyOptions<TResult>
            {
                MaxRetryAttempts = maxAttempts,
                Delay = delay,
                BackoffType = exponentialBackoff ? DelayBackoffType.Exponential : DelayBackoffType.Constant,
                ShouldHandle = shouldHandle
            })
            .Build();

    private static class RetryPipelineCache<TResult>
        where TResult : IResultBase
    {
        // The cap applies per closed generic type (Result and each Result<T>). The Count check is not atomic,
        // so concurrent callers can overshoot it slightly, which is acceptable for a cache bound.
        private const int MaxEntries = 64;

        private static readonly ConcurrentDictionary<(int MaxAttempts, TimeSpan Delay, bool ExponentialBackoff, bool UsesPredicate), ResiliencePipeline<TResult>> Pipelines = new();

        internal static ResiliencePipeline<TResult> Get(int maxAttempts, TimeSpan? delay, bool exponentialBackoff, bool usesPredicate)
        {
            (int MaxAttempts, TimeSpan Delay, bool ExponentialBackoff, bool UsesPredicate) key = (maxAttempts, delay ?? DefaultDelay, exponentialBackoff, usesPredicate);
            if (Pipelines.TryGetValue(key, out ResiliencePipeline<TResult>? cached))
            {
                return cached;
            }

            ResiliencePipeline<TResult> pipeline = BuildRetryPipeline<TResult>(
                key.MaxAttempts,
                key.Delay,
                key.ExponentialBackoff,
                key.UsesPredicate
                    ? static args => new ValueTask<bool>(ShouldHandleWithPredicate(args))
                    : static args => new ValueTask<bool>(ShouldHandle(args)));

            return Pipelines.Count < MaxEntries ? Pipelines.GetOrAdd(key, pipeline) : pipeline;
        }

        private static bool ShouldHandle(RetryPredicateArguments<TResult> args) =>
            args.Outcome.Exception is { } ex
                ? !ResilienceClassifier.IsCallerCancellation(ex, args.Context.CancellationToken)
                : args.Outcome.Result is { } result && ResilienceClassifier.IsRetryable(result);

        private static bool ShouldHandleWithPredicate(RetryPredicateArguments<TResult> args)
        {
            args.Context.Properties.TryGetValue(ShouldRetryKey, out Func<Error, bool>? shouldRetry);
            if (args.Outcome.Exception is { } ex)
            {
                return !ResilienceClassifier.IsCallerCancellation(ex, args.Context.CancellationToken)
                    && (shouldRetry is null || shouldRetry(ResilienceClassifier.ToError(ex)));
            }

            return args.Outcome.Result is { } result && (shouldRetry is null
                ? ResilienceClassifier.IsRetryable(result)
                : ResilienceClassifier.IsRetryable(result, shouldRetry));
        }
    }
}
