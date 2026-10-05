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
}
