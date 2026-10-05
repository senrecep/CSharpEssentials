using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using CSharpEssentials.Errors;

namespace CSharpEssentials.Validation;

/// <summary>
/// Collects validation errors and acts as the factory for <see cref="RuleChain{T,TProp}"/> instances.
/// An instance is created per <see cref="Validator{T}.ValidateAsync"/> call — it is not thread-safe
/// and should never be stored beyond the validation lifecycle.
/// </summary>
/// <typeparam name="T">The model type being validated.</typeparam>
public sealed class RuleContext<T>
{
    private List<Error>? _errors;
    private readonly string _propertyPrefix;

    /// <summary>Creates a root-level context with no property prefix.</summary>
    public RuleContext() => _propertyPrefix = string.Empty;

    /// <summary>
    /// Creates a prefixed context used inside <see cref="ForEach{TElement}"/>.
    /// The prefix is prepended to all property names resolved via <see cref="For{TProp}(System.Func{TProp},string)"/>.
    /// </summary>
    internal RuleContext(string propertyPrefix) => _propertyPrefix = propertyPrefix;

    internal bool HasErrors => _errors is not null && _errors.Count > 0;
    internal IReadOnlyList<Error> Errors => (IReadOnlyList<Error>?)_errors ?? [];

    /// <summary>Called by <see cref="RuleChain{T,TProp}.AddError"/> — not intended for direct use.</summary>
    internal void Append(Error error) => (_errors ??= []).Add(error);

    /// <summary>Appends all errors from a child context (used by ForEach and SetValidator).</summary>
    internal void AppendRange(IReadOnlyList<Error> errors)
    {
        if (errors.Count == 0)
            return;
        (_errors ??= []).AddRange(errors);
    }

    // -------------------------------------------------------------------------
    // Chain factory
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a validation chain for a property expression.
    /// The property is evaluated immediately; its value is captured for the chain lifetime.
    /// </summary>
    /// <remarks>
    /// A <see langword="null"/> intermediate segment in a member path is tolerated:
    /// <c>() => model.Address.City</c> with a <see langword="null"/> <c>Address</c> is treated
    /// as the property itself being <see langword="null"/> (guards such as <c>NotEmpty()</c> will fire).
    /// This tolerance only applies when <typeparamref name="TProp"/> can itself be null;
    /// for non-nullable value types the <see cref="NullReferenceException"/> propagates.
    /// </remarks>
    /// <example>
    /// <code>
    /// rules.For(() => model.Name).NotEmpty().MaxLength(100);
    /// rules.For(() => model.Address.City).NotEmpty();
    /// </code>
    /// </example>
    public RuleChain<T, TProp> For<TProp>(
        Func<TProp> expr,
        [CallerArgumentExpression(nameof(expr))] string memberExpr = "")
    {
        TProp value = EvaluateSafely(expr);
        if (_propertyPrefix.Length > 0)
        {
            (string extracted, bool isItemSelf) = PropertyPath.ExtractWithMeta(memberExpr);
            string fullName = isItemSelf
                ? _propertyPrefix[..^1]          // strip trailing dot: "Items[0]"
                : _propertyPrefix + extracted;   // "Items[0].PropertyName"
            return new RuleChain<T, TProp>(this, value, fullName);
        }

        return new RuleChain<T, TProp>(this, value, PropertyPath.Extract(memberExpr));
    }

    [SuppressMessage("Major Bug", "S1696:NullReferenceException should not be caught",
        Justification = "The expression is an opaque member-access delegate; a null intermediate " +
        "segment cannot be tested for null upfront. Catching NRE mirrors FluentValidation's " +
        "null-tolerant member paths and only applies when TProp can itself be null.")]
    private static TProp EvaluateSafely<TProp>(Func<TProp> expr)
    {
        try
        {
            return expr();
        }
        catch (NullReferenceException) when (default(TProp) is null)
        {
            return default!;
        }
    }

    /// <summary>
    /// Creates a validation chain from a pre-evaluated value and an explicit property name.
    /// Use this overload in performance-critical validators to eliminate both the lambda delegate
    /// allocation and the <see cref="PropertyPath"/> parsing overhead incurred by the
    /// <c>For(() => ...)</c> overload.
    /// </summary>
    /// <example>
    /// <code>
    /// rules.For(model.Name, nameof(model.Name)).NotEmpty().MaxLength(100);
    /// rules.For(model.Age, nameof(model.Age)).GreaterThan(0);
    /// </code>
    /// </example>
    public RuleChain<T, TProp> For<TProp>(TProp value, string propertyName)
    {
        string fullName = _propertyPrefix.Length > 0
            ? _propertyPrefix + propertyName
            : propertyName;
        return new RuleChain<T, TProp>(this, value, fullName);
    }

    // -------------------------------------------------------------------------
    // ForEach
    // -------------------------------------------------------------------------

    /// <summary>
    /// Iterates <paramref name="collectionExpr"/> and runs <paramref name="configure"/> for each element.
    /// Errors produced per element are accumulated with an indexed prefix, e.g. <c>"Tags[0].Name"</c>.
    /// A <see langword="null"/> or empty collection produces no errors; a <see langword="null"/>
    /// intermediate segment in the collection path is treated as a <see langword="null"/> collection.
    /// </summary>
    /// <example>
    /// <code>
    /// rules.ForEach(() => model.Addresses, (address, addressRules) =>
    /// {
    ///     addressRules.For(() => address.City).NotEmpty();
    ///     addressRules.For(() => address.ZipCode).Matches(@"^\d{5}$");
    /// });
    /// </code>
    /// </example>
    public void ForEach<TElement>(
        Func<IEnumerable<TElement>?> collectionExpr,
        Action<TElement, RuleContext<TElement>> configure,
        [CallerArgumentExpression(nameof(collectionExpr))] string memberExpr = "")
    {
        IEnumerable<TElement>? collection = EvaluateCollectionSafely(collectionExpr);
        if (collection is null)
            return;

        string collectionName = PropertyPath.Extract(memberExpr);
        string basePrefix = _propertyPrefix + collectionName;
        int index = 0;
        foreach (TElement item in collection)
        {
            // Prepend _propertyPrefix so nested ForEach produces fully-qualified paths:
            // e.g. outer prefix "Orders[0]." + "Items[1]." → "Orders[0].Items[1].City.NotEmpty"
            RuleContext<TElement> itemCtx = new($"{basePrefix}[{index}].");
            configure(item, itemCtx);
            if (itemCtx.HasErrors)
                AppendRange(itemCtx.Errors);
            index++;
        }
    }

    [SuppressMessage("Major Bug", "S1696:NullReferenceException should not be caught",
        Justification = "The expression is an opaque member-access delegate; a null intermediate " +
        "segment cannot be tested for null upfront and is treated as a null collection, " +
        "which ForEach already defines as producing no errors.")]
    private static IEnumerable<TElement>? EvaluateCollectionSafely<TElement>(
        Func<IEnumerable<TElement>?> collectionExpr)
    {
        try
        {
            return collectionExpr();
        }
        catch (NullReferenceException)
        {
            return null;
        }
    }

    /// <summary>
    /// Asynchronous variant of <see cref="ForEach{TElement}"/>.
    /// </summary>
    /// <remarks>
    /// Cancellation is checked between iterations via <see cref="CancellationToken.ThrowIfCancellationRequested"/>.
    /// Errors from iterations completed before cancellation are preserved — callers should treat a cancelled
    /// <see cref="ForEachAsync{TElement}"/> as producing partial results.
    /// </remarks>
    public async ValueTask ForEachAsync<TElement>(
        Func<IEnumerable<TElement>?> collectionExpr,
        Func<TElement, RuleContext<TElement>, CancellationToken, ValueTask> configure,
        CancellationToken ct = default,
        [CallerArgumentExpression(nameof(collectionExpr))] string memberExpr = "")
    {
        IEnumerable<TElement>? collection = EvaluateCollectionSafely(collectionExpr);
        if (collection is null)
            return;

        string collectionName = PropertyPath.Extract(memberExpr);
        string basePrefix = _propertyPrefix + collectionName;
        int index = 0;
        foreach (TElement item in collection)
        {
            ct.ThrowIfCancellationRequested();
            RuleContext<TElement> itemCtx = new($"{basePrefix}[{index}].");
            await configure(item, itemCtx, ct).ConfigureAwait(false);
            if (itemCtx.HasErrors)
                AppendRange(itemCtx.Errors);
            index++;
        }
    }

    // -------------------------------------------------------------------------
    // Manual error injection
    // -------------------------------------------------------------------------

    /// <summary>
    /// Manually adds a validation error to the context.
    /// Useful for cross-field or model-level validation that does not target a single property.
    /// </summary>
    public RuleContext<T> AddFailure(Error error)
    {
        (_errors ??= []).Add(error);
        return this;
    }

    /// <summary>
    /// Manually adds a validation error with <paramref name="code"/> and <paramref name="description"/>.
    /// </summary>
    public RuleContext<T> AddFailure(string code, string description)
        => AddFailure(Error.Validation(code, description));
}
