using CSharpEssentials.Errors;

namespace CSharpEssentials.Validation.Validators;

/// <summary>
/// Extension validators for properties that implement <see cref="IComparable{T}"/>.
/// Works with <see cref="int"/>, <see cref="long"/>, <see cref="double"/>,
/// <see cref="decimal"/>, <see cref="DateTime"/>, <see cref="string"/>, and any other comparable type.
/// </summary>
public static class ComparableValidators
{
    /// <summary>Fails if the value is less than or equal to <paramref name="threshold"/>.</summary>
    public static RuleChain<T, TProp> GreaterThan<T, TProp>(
        this RuleChain<T, TProp> chain,
        TProp threshold,
        string? message = null)
        where TProp : IComparable<TProp>
    {
        if (!chain.HasFailed && chain.Value is not null && chain.Value.CompareTo(threshold) <= 0)
            chain.AddError(Error.Validation(
                $"{chain.PropertyName}.GreaterThan",
                message ?? $"'{chain.PropertyName}' must be greater than {threshold}."));
        return chain;
    }

    /// <summary>Fails if the value is less than or equal to <paramref name="threshold"/>. Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, TProp> GreaterThan<T, TProp>(
        this RuleChain<T, TProp> chain,
        TProp threshold,
        Error error)
        where TProp : IComparable<TProp>
    {
        if (!chain.HasFailed && chain.Value is not null && chain.Value.CompareTo(threshold) <= 0)
            chain.AddError(error);
        return chain;
    }

    /// <summary>Fails if the value is less than <paramref name="threshold"/>.</summary>
    public static RuleChain<T, TProp> GreaterThanOrEqualTo<T, TProp>(
        this RuleChain<T, TProp> chain,
        TProp threshold,
        string? message = null)
        where TProp : IComparable<TProp>
    {
        if (!chain.HasFailed && chain.Value is not null && chain.Value.CompareTo(threshold) < 0)
            chain.AddError(Error.Validation(
                $"{chain.PropertyName}.GreaterThanOrEqualTo",
                message ?? $"'{chain.PropertyName}' must be greater than or equal to {threshold}."));
        return chain;
    }

    /// <summary>Fails if the value is less than <paramref name="threshold"/>. Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, TProp> GreaterThanOrEqualTo<T, TProp>(
        this RuleChain<T, TProp> chain,
        TProp threshold,
        Error error)
        where TProp : IComparable<TProp>
    {
        if (!chain.HasFailed && chain.Value is not null && chain.Value.CompareTo(threshold) < 0)
            chain.AddError(error);
        return chain;
    }

    /// <summary>Fails if the value is greater than or equal to <paramref name="threshold"/>.</summary>
    public static RuleChain<T, TProp> LessThan<T, TProp>(
        this RuleChain<T, TProp> chain,
        TProp threshold,
        string? message = null)
        where TProp : IComparable<TProp>
    {
        if (!chain.HasFailed && chain.Value is not null && chain.Value.CompareTo(threshold) >= 0)
            chain.AddError(Error.Validation(
                $"{chain.PropertyName}.LessThan",
                message ?? $"'{chain.PropertyName}' must be less than {threshold}."));
        return chain;
    }

    /// <summary>Fails if the value is greater than or equal to <paramref name="threshold"/>. Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, TProp> LessThan<T, TProp>(
        this RuleChain<T, TProp> chain,
        TProp threshold,
        Error error)
        where TProp : IComparable<TProp>
    {
        if (!chain.HasFailed && chain.Value is not null && chain.Value.CompareTo(threshold) >= 0)
            chain.AddError(error);
        return chain;
    }

    /// <summary>Fails if the value is greater than <paramref name="threshold"/>.</summary>
    public static RuleChain<T, TProp> LessThanOrEqualTo<T, TProp>(
        this RuleChain<T, TProp> chain,
        TProp threshold,
        string? message = null)
        where TProp : IComparable<TProp>
    {
        if (!chain.HasFailed && chain.Value is not null && chain.Value.CompareTo(threshold) > 0)
            chain.AddError(Error.Validation(
                $"{chain.PropertyName}.LessThanOrEqualTo",
                message ?? $"'{chain.PropertyName}' must be less than or equal to {threshold}."));
        return chain;
    }

    /// <summary>Fails if the value is greater than <paramref name="threshold"/>. Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, TProp> LessThanOrEqualTo<T, TProp>(
        this RuleChain<T, TProp> chain,
        TProp threshold,
        Error error)
        where TProp : IComparable<TProp>
    {
        if (!chain.HasFailed && chain.Value is not null && chain.Value.CompareTo(threshold) > 0)
            chain.AddError(error);
        return chain;
    }

    /// <summary>
    /// Fails if the value is outside the inclusive range [<paramref name="min"/>, <paramref name="max"/>].
    /// </summary>
    public static RuleChain<T, TProp> InclusiveBetween<T, TProp>(
        this RuleChain<T, TProp> chain,
        TProp min,
        TProp max,
        string? message = null)
        where TProp : IComparable<TProp>
    {
        if (min.CompareTo(max) > 0)
            throw new ArgumentOutOfRangeException(nameof(min), $"min ({min}) must not be greater than max ({max}).");
        if (!chain.HasFailed && chain.Value is not null && (chain.Value.CompareTo(min) < 0 || chain.Value.CompareTo(max) > 0))
            chain.AddError(Error.Validation(
                $"{chain.PropertyName}.InclusiveBetween",
                message ?? $"'{chain.PropertyName}' must be between {min} and {max} (inclusive)."));
        return chain;
    }

    /// <summary>
    /// Fails if the value is outside the inclusive range [<paramref name="min"/>, <paramref name="max"/>]. Uses <paramref name="error"/> directly.
    /// </summary>
    public static RuleChain<T, TProp> InclusiveBetween<T, TProp>(
        this RuleChain<T, TProp> chain,
        TProp min,
        TProp max,
        Error error)
        where TProp : IComparable<TProp>
    {
        if (min.CompareTo(max) > 0)
            throw new ArgumentOutOfRangeException(nameof(min), $"min ({min}) must not be greater than max ({max}).");
        if (!chain.HasFailed && chain.Value is not null && (chain.Value.CompareTo(min) < 0 || chain.Value.CompareTo(max) > 0))
            chain.AddError(error);
        return chain;
    }

    /// <summary>
    /// Fails if the value is outside the exclusive range (<paramref name="min"/>, <paramref name="max"/>).
    /// </summary>
    public static RuleChain<T, TProp> ExclusiveBetween<T, TProp>(
        this RuleChain<T, TProp> chain,
        TProp min,
        TProp max,
        string? message = null)
        where TProp : IComparable<TProp>
    {
        if (min.CompareTo(max) >= 0)
            throw new ArgumentOutOfRangeException(nameof(min), $"min ({min}) must be less than max ({max}) for an exclusive range.");
        if (!chain.HasFailed && chain.Value is not null && (chain.Value.CompareTo(min) <= 0 || chain.Value.CompareTo(max) >= 0))
            chain.AddError(Error.Validation(
                $"{chain.PropertyName}.ExclusiveBetween",
                message ?? $"'{chain.PropertyName}' must be between {min} and {max} (exclusive)."));
        return chain;
    }

    /// <summary>
    /// Fails if the value is outside the exclusive range (<paramref name="min"/>, <paramref name="max"/>). Uses <paramref name="error"/> directly.
    /// </summary>
    public static RuleChain<T, TProp> ExclusiveBetween<T, TProp>(
        this RuleChain<T, TProp> chain,
        TProp min,
        TProp max,
        Error error)
        where TProp : IComparable<TProp>
    {
        if (min.CompareTo(max) >= 0)
            throw new ArgumentOutOfRangeException(nameof(min), $"min ({min}) must be less than max ({max}) for an exclusive range.");
        if (!chain.HasFailed && chain.Value is not null && (chain.Value.CompareTo(min) <= 0 || chain.Value.CompareTo(max) >= 0))
            chain.AddError(error);
        return chain;
    }

    /// <summary>Fails if the value is not equal to <paramref name="expected"/>.</summary>
    public static RuleChain<T, TProp> Equal<T, TProp>(
        this RuleChain<T, TProp> chain,
        TProp expected,
        string? message = null)
        where TProp : IEquatable<TProp>
    {
        if (!chain.HasFailed && (chain.Value is null || !chain.Value.Equals(expected)))
            chain.AddError(Error.Validation(
                $"{chain.PropertyName}.Equal",
                message ?? $"'{chain.PropertyName}' must be equal to {expected}."));
        return chain;
    }

    /// <summary>Fails if the value is not equal to <paramref name="expected"/>. Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, TProp> Equal<T, TProp>(
        this RuleChain<T, TProp> chain,
        TProp expected,
        Error error)
        where TProp : IEquatable<TProp>
    {
        if (!chain.HasFailed && (chain.Value is null || !chain.Value.Equals(expected)))
            chain.AddError(error);
        return chain;
    }

    /// <summary>Fails if the value equals <paramref name="forbidden"/>.</summary>
    public static RuleChain<T, TProp> NotEqual<T, TProp>(
        this RuleChain<T, TProp> chain,
        TProp forbidden,
        string? message = null)
        where TProp : IEquatable<TProp>
    {
        if (!chain.HasFailed && chain.Value is not null && chain.Value.Equals(forbidden))
            chain.AddError(Error.Validation(
                $"{chain.PropertyName}.NotEqual",
                message ?? $"'{chain.PropertyName}' must not be equal to {forbidden}."));
        return chain;
    }

    /// <summary>Fails if the value equals <paramref name="forbidden"/>. Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, TProp> NotEqual<T, TProp>(
        this RuleChain<T, TProp> chain,
        TProp forbidden,
        Error error)
        where TProp : IEquatable<TProp>
    {
        if (!chain.HasFailed && chain.Value is not null && chain.Value.Equals(forbidden))
            chain.AddError(error);
        return chain;
    }
}
