namespace CSharpEssentials.Validation;

/// <summary>
/// Controls how a <see cref="RuleChain{T,TProp}"/> behaves after a validation failure.
/// </summary>
public enum CascadeMode
{
    /// <summary>
    /// Stops running the remaining validators on the chain as soon as one fails.
    /// This is the default. Use it for guard checks (e.g. <c>NotNull</c>, <c>NotEmpty</c>),
    /// where further validators on a null or empty value would only add meaningless errors.
    /// </summary>
    Stop = 0,

    /// <summary>
    /// Keeps running the remaining validators after a failure and collects all errors.
    /// Use this after a guard check to collect every constraint violation for a property at once.
    /// </summary>
    Continue = 1
}
