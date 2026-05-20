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
    private readonly List<Error> _errors = [];
    private readonly string _propertyPrefix;

    /// <summary>Creates a root-level context with no property prefix.</summary>
    public RuleContext() => _propertyPrefix = string.Empty;

    /// <summary>
    /// Creates a prefixed context used inside <see cref="ForEach{TElement}"/>.
    /// The prefix is prepended to all property names resolved via <see cref="For{TProp}"/>.
    /// </summary>
    internal RuleContext(string propertyPrefix) => _propertyPrefix = propertyPrefix;

    internal bool HasErrors => _errors.Count > 0;
    internal IReadOnlyList<Error> Errors => _errors;

    /// <summary>Called by <see cref="RuleChain{T,TProp}.AddError"/> — not intended for direct use.</summary>
    internal void Append(Error error) => _errors.Add(error);

    /// <summary>Appends all errors from a child context (used by ForEach and SetValidator).</summary>
    internal void AppendRange(IReadOnlyList<Error> errors) => _errors.AddRange(errors);

    // -------------------------------------------------------------------------
    // Chain factory
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a validation chain for a property expression.
    /// The property is evaluated immediately; its value is captured for the chain lifetime.
    /// </summary>
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
        if (_propertyPrefix.Length > 0)
        {
            (string extracted, bool isItemSelf) = PropertyPath.ExtractWithMeta(memberExpr);
            string fullName = isItemSelf
                ? _propertyPrefix[..^1]          // strip trailing dot: "Items[0]"
                : _propertyPrefix + extracted;   // "Items[0].PropertyName"
            return new RuleChain<T, TProp>(this, expr(), fullName);
        }

        return new RuleChain<T, TProp>(this, expr(), PropertyPath.Extract(memberExpr));
    }

    // -------------------------------------------------------------------------
    // ForEach
    // -------------------------------------------------------------------------

    /// <summary>
    /// Iterates <paramref name="collectionExpr"/> and runs <paramref name="configure"/> for each element.
    /// Errors produced per element are accumulated with an indexed prefix, e.g. <c>"Tags[0].Name"</c>.
    /// A <see langword="null"/> or empty collection produces no errors.
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
        IEnumerable<TElement>? collection = collectionExpr();
        if (collection is null)
            return;

        string collectionName = PropertyPath.Extract(memberExpr);
        int index = 0;
        foreach (TElement item in collection)
        {
            // Prepend _propertyPrefix so nested ForEach produces fully-qualified paths:
            // e.g. outer prefix "Orders[0]." + "Items[1]." → "Orders[0].Items[1].City.NotEmpty"
            RuleContext<TElement> itemCtx = new($"{_propertyPrefix}{collectionName}[{index}].");
            configure(item, itemCtx);
            AppendRange(itemCtx.Errors);
            index++;
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
        IEnumerable<TElement>? collection = collectionExpr();
        if (collection is null)
            return;

        string collectionName = PropertyPath.Extract(memberExpr);
        int index = 0;
        foreach (TElement item in collection)
        {
            ct.ThrowIfCancellationRequested();
            RuleContext<TElement> itemCtx = new($"{_propertyPrefix}{collectionName}[{index}].");
            await configure(item, itemCtx, ct).ConfigureAwait(false);
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
        _errors.Add(error);
        return this;
    }

    /// <summary>
    /// Manually adds a validation error with <paramref name="code"/> and <paramref name="description"/>.
    /// </summary>
    public RuleContext<T> AddFailure(string code, string description)
        => AddFailure(Error.Validation(code, description));
}
