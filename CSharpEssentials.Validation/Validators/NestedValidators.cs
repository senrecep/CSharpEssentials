using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Validation.Validators;

/// <summary>
/// Provides <c>SetValidatorAsync</c> — delegates validation of a nested property to a dedicated
/// <see cref="IValidator{TProp}"/> and propagates all resulting errors to the parent context.
/// </summary>
public static class NestedValidators
{
    /// <summary>
    /// Asynchronously runs <paramref name="validator"/> against the chain's current value and
    /// appends any validation errors to the parent <see cref="RuleContext{T}"/>.
    /// <para>
    /// If the chain has already failed, or the value is <see langword="null"/>, the validator
    /// is skipped. Use a preceding <c>NotNull()</c> call to enforce non-null before validating.
    /// </para>
    /// </summary>
    /// <example>
    /// <code>
    /// // Non-nullable nested property
    /// await rules.For(() => model.Address).SetValidatorAsync(new AddressValidator(), ct);
    ///
    /// // Nullable reference type — no ! operator needed
    /// await rules.For(() => model.BillingAddress).SetValidatorAsync(new AddressValidator(), ct);
    /// </code>
    /// </example>
    public static async ValueTask<RuleChain<T, TProp>> SetValidatorAsync<T, TProp>(
        this RuleChain<T, TProp> chain,
        IValidator<TProp> validator,
        CancellationToken ct = default)
    {
        if (chain.HasFailed || chain.Value is null)
            return chain;

        Result<TProp> result = await validator.ValidateAsync(chain.Value, ct).ConfigureAwait(false);
        if (result.IsFailure)
            PropagateErrors(chain, result.Errors);
        return chain;
    }

    /// <summary>
    /// Asynchronously runs <paramref name="validator"/> against the chain's current value and
    /// appends any validation errors to the parent <see cref="RuleContext{T}"/>.
    /// <para>
    /// Use this overload when the property is declared as a nullable reference type (<c>Address?</c>).
    /// The validator is typed against the non-null form — no null-forgiving operator is required.
    /// </para>
    /// <para>
    /// If the chain has already failed, or the value is <see langword="null"/>, the validator
    /// is skipped automatically.
    /// </para>
    /// </summary>
    /// <example>
    /// <code>
    /// // model.BillingAddress is Address? — works directly, no ! required
    /// await rules.For(() => model.BillingAddress).SetValidatorAsync(new AddressValidator(), ct);
    /// </code>
    /// </example>
    public static async ValueTask<RuleChain<T, TProp?>> SetValidatorAsync<T, TProp, TValidator>(
        this RuleChain<T, TProp?> chain,
        TValidator validator,
        CancellationToken ct = default)
        where TProp : class
        where TValidator : IValidator<TProp>
    {
        TProp? value = chain.Value;
        if (chain.HasFailed || value is null)
            return chain;

        Result<TProp> result = await validator.ValidateAsync(value, ct).ConfigureAwait(false);
        if (result.IsFailure)
            PropagateErrors(chain, result.Errors);
        return chain;
    }

    private static void PropagateErrors<T, TChainProp>(
        RuleChain<T, TChainProp> chain,
        IReadOnlyList<Error> errors)
    {
        string prefix = chain.PropertyName + ".";
        foreach (Error error in errors)
        {
            chain.AppendToContext(error with
            {
                Code = prefix + error.Code,
                Description = PrefixDescription(error.Description, prefix)
            });
        }
        chain.MarkFailed();
    }

    private static string PrefixDescription(string description, string prefix)
    {
        int open = description.IndexOf('\'');
        if (open < 0)
            return description;
        int close = description.IndexOf('\'', open + 1);
        if (close <= open)
            return description;
        string token = description[(open + 1)..close];
        if (token.Contains(' '))
            return description;
        return description[..open] + '\'' + prefix + token + '\'' + description[(close + 1)..];
    }
}
