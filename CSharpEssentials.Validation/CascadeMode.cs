namespace CSharpEssentials.Validation;

/// <summary>
/// Controls how a <see cref="RuleChain{T,TProp}"/> behaves after a validation failure.
/// </summary>
public enum CascadeMode
{
    /// <summary>
    /// Stops executing subsequent validators on the chain as soon as one fails.
    /// This is the default and is appropriate for guard checks (e.g. <c>NotNull</c>, <c>NotEmpty</c>)
    /// where running further validators on a null or empty value would produce meaningless errors.
    /// </summary>
    Stop = 0,

    /// <summary>
    /// Continues executing subsequent validators even after a failure, accumulating all errors.
    /// Use this after a guard check to collect every constraint violation for a property at once.
    /// </summary>
    Continue = 1
}
