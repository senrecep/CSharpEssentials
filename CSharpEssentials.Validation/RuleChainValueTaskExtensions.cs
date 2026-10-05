using CSharpEssentials.Errors;

namespace CSharpEssentials.Validation;

/// <summary>
/// Provides a <see cref="ValueTask{T}"/>-based <c>MustAsync</c> overload as an extension method.
/// It lives outside <see cref="RuleChain{T,TProp}"/> on purpose: as an extension it is only
/// considered when the instance <c>MustAsync</c> (Task-based) is not applicable, so existing
/// async-lambda call sites keep binding to the instance overload without ambiguity.
/// </summary>
public static class RuleChainValueTaskExtensions
{
    /// <summary>
    /// Validates the property using a custom asynchronous predicate returning <see cref="ValueTask{T}"/>.
    /// Prefer this overload when the predicate source is <see cref="ValueTask{T}"/>-based — it avoids
    /// the <see cref="Task{T}"/> allocation of the instance overload.
    /// Returns the awaited chain so further calls can be chained after awaiting.
    /// </summary>
    public static async ValueTask<RuleChain<T, TProp>> MustAsync<T, TProp>(
        this RuleChain<T, TProp> chain,
        Func<TProp, CancellationToken, ValueTask<bool>> predicate,
        string errorCode,
        string errorMessage,
        CancellationToken ct = default)
    {
        if (!chain.HasFailed && !await predicate(chain.Value, ct).ConfigureAwait(false))
            chain.AddError(Error.Validation(errorCode, errorMessage));
        return chain;
    }
}
