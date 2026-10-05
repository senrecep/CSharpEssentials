using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern.Interfaces;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace CSharpEssentials.Resilience;

internal static class ResilienceClassifier
{
    internal static bool IsCallerCancellation(Exception ex, CancellationToken callerToken) =>
        ex is OperationCanceledException && callerToken.IsCancellationRequested;

    internal static Error HandleException(Exception ex, CancellationToken callerToken)
    {
        if (IsCallerCancellation(ex, callerToken))
        {
            throw new OperationCanceledException(ex.Message, ex, callerToken);
        }

        return ToError(ex);
    }

    internal static Error ToError(Exception ex)
    {
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

    // A retryable failure observed while the caller's token is cancelled surfaces as cancellation, whether the
    // cancel landed during an attempt or during the backoff delay. This also applies right after the final
    // attempt: cancellation wins over returning the last failure.
    internal static TResult ThrowIfCallerCancelled<TResult>(TResult result, CancellationToken callerToken)
        where TResult : IResultBase
    {
        if (callerToken.IsCancellationRequested && IsRetryable(result))
        {
            throw new OperationCanceledException(callerToken);
        }

        return result;
    }

    internal static TResult ThrowIfCallerCancelled<TResult>(TResult result, Func<Error, bool> shouldRetry, CancellationToken callerToken)
        where TResult : IResultBase
    {
        if (callerToken.IsCancellationRequested && IsRetryable(result, shouldRetry))
        {
            throw new OperationCanceledException(callerToken);
        }

        return result;
    }

    internal static bool IsRetryable<TResult>(TResult result)
        where TResult : IResultBase
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

    internal static bool IsRetryable<TResult>(TResult result, Func<Error, bool> shouldRetry)
        where TResult : IResultBase =>
        result.IsFailure && shouldRetry(result.FirstError);
}
