using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Rules;

public static partial class RuleEngine
{
    private static TRule? LinearNext<TRule>(bool isFailure, TRule? next) where TRule : class =>
        isFailure ? null : next;

    private static TRule? ConditionalBranch<TRule>(bool isFailure, TRule? success, TRule? failure) where TRule : class =>
        isFailure ? failure : success;

    private static Result<TResult> FirstAndValue<TResult>(Result<TResult[]> andResult) =>
        andResult.IsFailure ? andResult.Errors : andResult.Value[0];

    private static bool IsCallerCancellation(Exception exception, CancellationToken cancellationToken) =>
        exception is OperationCanceledException && cancellationToken.IsCancellationRequested;
}
