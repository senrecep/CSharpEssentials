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
    /// await rules.For(() => model.Address!).SetValidatorAsync(new AddressValidator(), ct);
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
        {
            string prefix = chain.PropertyName + ".";
            foreach (Error error in result.Errors)
            {
                chain.AppendToContext(error with
                {
                    Code = prefix + error.Code,
                    Description = PrefixDescription(error.Description, prefix)
                });
            }
            chain.MarkFailed();
        }
        return chain;
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
