using System.Runtime.CompilerServices;
using CSharpEssentials.Errors;

namespace CSharpEssentials.Validation;

/// <summary>
/// A fluent validation chain for a single property value of type <typeparamref name="TProp"/>
/// on a model of type <typeparamref name="T"/>.
/// <para>
/// Validators are evaluated <b>eagerly</b>: each call checks the value immediately and appends
/// an <see cref="Error"/> to the parent <see cref="RuleContext{T}"/> when the check fails.
/// </para>
/// <para>
/// The default cascade mode is <see cref="CascadeMode.Stop"/>: once the first failure is recorded,
/// subsequent calls on the same chain are skipped. Call <see cref="Cascade"/> with
/// <see cref="CascadeMode.Continue"/> after a guard check (e.g. <c>NotEmpty()</c>) to switch
/// to error-accumulation mode for the remaining validators.
/// </para>
/// </summary>
/// <typeparam name="T">The model type being validated.</typeparam>
/// <typeparam name="TProp">The property type.</typeparam>
public sealed class RuleChain<T, TProp>
{
    private readonly RuleContext<T> _ctx;
    private bool _hasError;
    private bool _continueMode;

    internal RuleChain(RuleContext<T> ctx, TProp value, string propertyName)
    {
        _ctx = ctx;
        Value = value;
        PropertyName = propertyName;
    }

    /// <summary>The resolved value of the property at the time <c>For()</c> was called.</summary>
    public TProp Value { get; }

    /// <summary>The extracted property path (e.g. <c>"Name"</c> or <c>"Address.City"</c>).</summary>
    public string PropertyName { get; }

    /// <summary>
    /// <see langword="true"/> when at least one error has been recorded AND the chain is in
    /// <see cref="CascadeMode.Stop"/> mode. Subsequent validator calls are skipped while this
    /// is <see langword="true"/>.
    /// In <see cref="CascadeMode.Continue"/> mode this always returns <see langword="false"/>
    /// so that all validators run and accumulate errors independently.
    /// </summary>
    public bool HasFailed => _hasError && !_continueMode;

    /// <summary>
    /// Changes the cascade behaviour for all subsequent validators on this chain.
    /// <para>
    /// Call this after a guard check (<c>NotNull</c> / <c>NotEmpty</c>) to switch from
    /// stop-on-first-failure to accumulate-all-errors mode:
    /// </para>
    /// <code>
    /// rules.For(() => model.Email)
    ///     .NotEmpty()                       // guard: null/empty → stop
    ///     .Cascade(CascadeMode.Continue)    // value exists → collect every error
    ///     .MinLength(5)
    ///     .MaxLength(100)
    ///     .EmailAddress();
    /// </code>
    /// <para>
    /// If the chain has already failed (guard fired), this call is a no-op — the chain stays
    /// stopped and the null/empty guard error is the only one reported.
    /// </para>
    /// </summary>
    public RuleChain<T, TProp> Cascade(CascadeMode mode)
    {
        // Only honour the mode switch when no error has occurred yet (guard passed).
        // If a guard already fired, keep the chain stopped to avoid running validators
        // on a null or empty value.
        if (!_hasError)
            _continueMode = mode == CascadeMode.Continue;
        return this;
    }

    /// <summary>
    /// Appends <paramref name="error"/> to the parent context and marks this chain as failed.
    /// In <see cref="CascadeMode.Stop"/> mode subsequent calls on this chain become no-ops.
    /// In <see cref="CascadeMode.Continue"/> mode all validators keep running.
    /// Use this method when writing custom extension validators.
    /// </summary>
    public RuleChain<T, TProp> AddError(Error error)
    {
        // In Stop mode (HasFailed = _hasError): only the first call records an error.
        // In Continue mode (HasFailed = false always): every call records its error,
        // enabling full accumulation across independent constraint validators.
        if (!HasFailed)
        {
            _ctx.Append(error);
            _hasError = true;
        }
        return this;
    }

    /// <summary>
    /// Appends <paramref name="error"/> directly to the parent context, bypassing the cascade-stop
    /// guard. Used by <c>SetValidator</c> to propagate multiple errors from a nested validator.
    /// </summary>
    internal void AppendToContext(Error error) => _ctx.Append(error);

    /// <summary>
    /// Marks this chain as failed without appending an extra error.
    /// Used after <see cref="AppendToContext"/> so that subsequent rules on this chain are skipped.
    /// </summary>
    internal void MarkFailed() => _hasError = true;

    // -------------------------------------------------------------------------
    // Built-in: custom predicate
    // -------------------------------------------------------------------------

    /// <summary>
    /// Validates the property using a custom synchronous predicate.
    /// </summary>
    /// <param name="predicate">Returns <see langword="true"/> when the value is valid.</param>
    /// <param name="errorCode">The error code used when the predicate returns <see langword="false"/>.</param>
    /// <param name="errorMessage">The human-readable error message.</param>
    public RuleChain<T, TProp> Must(
        Func<TProp, bool> predicate,
        string errorCode,
        string errorMessage)
    {
        if (!HasFailed && !predicate(Value))
            AddError(Error.Validation(errorCode, errorMessage));
        return this;
    }

    /// <summary>
    /// Validates the property using a custom synchronous predicate with a default error message.
    /// </summary>
    public RuleChain<T, TProp> Must(
        Func<TProp, bool> predicate,
        [CallerArgumentExpression(nameof(predicate))] string predicateExpr = "")
    {
        if (!HasFailed && !predicate(Value))
            AddError(Error.Validation(
                $"{PropertyName}.Must",
                $"'{PropertyName}' did not satisfy the condition: {predicateExpr}."));
        return this;
    }

    /// <summary>
    /// Validates the property using a custom asynchronous predicate.
    /// Returns the awaited chain so further calls can be chained after awaiting.
    /// </summary>
    public async ValueTask<RuleChain<T, TProp>> MustAsync(
        Func<TProp, CancellationToken, Task<bool>> predicate,
        string errorCode,
        string errorMessage,
        CancellationToken ct = default)
    {
        if (!HasFailed && !await predicate(Value, ct).ConfigureAwait(false))
            AddError(Error.Validation(errorCode, errorMessage));
        return this;
    }
}
