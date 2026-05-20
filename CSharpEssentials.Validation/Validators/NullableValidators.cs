using CSharpEssentials.Errors;

namespace CSharpEssentials.Validation.Validators;

/// <summary>
/// Extension validators for nullable value types (<see cref="Nullable{T}"/>).
/// </summary>
public static class NullableValidators
{
    /// <summary>Fails if the nullable value has no value (is <see langword="null"/>).</summary>
    public static RuleChain<T, TValue?> NotNull<T, TValue>(
        this RuleChain<T, TValue?> chain,
        string? message = null)
        where TValue : struct
    {
        if (!chain.HasFailed && chain.Value is null)
            chain.AddError(Error.Validation(
                $"{chain.PropertyName}.NotNull",
                message ?? $"'{chain.PropertyName}' must not be null."));
        return chain;
    }

    /// <summary>Fails if the nullable value has no value (is <see langword="null"/>). Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, TValue?> NotNull<T, TValue>(
        this RuleChain<T, TValue?> chain,
        Error error)
        where TValue : struct
    {
        if (!chain.HasFailed && chain.Value is null)
            chain.AddError(error);
        return chain;
    }

    /// <summary>Fails if the nullable value has a value (is not <see langword="null"/>).</summary>
    public static RuleChain<T, TValue?> Null<T, TValue>(
        this RuleChain<T, TValue?> chain,
        string? message = null)
        where TValue : struct
    {
        if (!chain.HasFailed && chain.Value.HasValue)
            chain.AddError(Error.Validation(
                $"{chain.PropertyName}.Null",
                message ?? $"'{chain.PropertyName}' must be null."));
        return chain;
    }

    /// <summary>Fails if the nullable value has a value (is not <see langword="null"/>). Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, TValue?> Null<T, TValue>(
        this RuleChain<T, TValue?> chain,
        Error error)
        where TValue : struct
    {
        if (!chain.HasFailed && chain.Value.HasValue)
            chain.AddError(error);
        return chain;
    }

    // -------------------------------------------------------------------------
    // Comparable forwarding for nullable struct values
    // -------------------------------------------------------------------------

    /// <summary>Fails if the nullable value is not null and less than or equal to <paramref name="threshold"/>.</summary>
    public static RuleChain<T, TValue?> GreaterThan<T, TValue>(
        this RuleChain<T, TValue?> chain,
        TValue threshold,
        string? message = null)
        where TValue : struct, IComparable<TValue>
    {
        if (chain.HasFailed || chain.Value is null)
            return chain;
        if (chain.Value.Value.CompareTo(threshold) <= 0)
            chain.AddError(Error.Validation(
                $"{chain.PropertyName}.GreaterThan",
                message ?? $"'{chain.PropertyName}' must be greater than {threshold}."));
        return chain;
    }

    /// <summary>Fails if the nullable value is not null and less than or equal to <paramref name="threshold"/>. Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, TValue?> GreaterThan<T, TValue>(
        this RuleChain<T, TValue?> chain,
        TValue threshold,
        Error error)
        where TValue : struct, IComparable<TValue>
    {
        if (chain.HasFailed || chain.Value is null)
            return chain;
        if (chain.Value.Value.CompareTo(threshold) <= 0)
            chain.AddError(error);
        return chain;
    }

    /// <summary>Fails if the nullable value is not null and less than <paramref name="threshold"/>.</summary>
    public static RuleChain<T, TValue?> GreaterThanOrEqualTo<T, TValue>(
        this RuleChain<T, TValue?> chain,
        TValue threshold,
        string? message = null)
        where TValue : struct, IComparable<TValue>
    {
        if (chain.HasFailed || chain.Value is null)
            return chain;
        if (chain.Value.Value.CompareTo(threshold) < 0)
            chain.AddError(Error.Validation(
                $"{chain.PropertyName}.GreaterThanOrEqualTo",
                message ?? $"'{chain.PropertyName}' must be greater than or equal to {threshold}."));
        return chain;
    }

    /// <summary>Fails if the nullable value is not null and less than <paramref name="threshold"/>. Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, TValue?> GreaterThanOrEqualTo<T, TValue>(
        this RuleChain<T, TValue?> chain,
        TValue threshold,
        Error error)
        where TValue : struct, IComparable<TValue>
    {
        if (chain.HasFailed || chain.Value is null)
            return chain;
        if (chain.Value.Value.CompareTo(threshold) < 0)
            chain.AddError(error);
        return chain;
    }

    /// <summary>Fails if the nullable value is not null and greater than or equal to <paramref name="threshold"/>.</summary>
    public static RuleChain<T, TValue?> LessThan<T, TValue>(
        this RuleChain<T, TValue?> chain,
        TValue threshold,
        string? message = null)
        where TValue : struct, IComparable<TValue>
    {
        if (chain.HasFailed || chain.Value is null)
            return chain;
        if (chain.Value.Value.CompareTo(threshold) >= 0)
            chain.AddError(Error.Validation(
                $"{chain.PropertyName}.LessThan",
                message ?? $"'{chain.PropertyName}' must be less than {threshold}."));
        return chain;
    }

    /// <summary>Fails if the nullable value is not null and greater than or equal to <paramref name="threshold"/>. Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, TValue?> LessThan<T, TValue>(
        this RuleChain<T, TValue?> chain,
        TValue threshold,
        Error error)
        where TValue : struct, IComparable<TValue>
    {
        if (chain.HasFailed || chain.Value is null)
            return chain;
        if (chain.Value.Value.CompareTo(threshold) >= 0)
            chain.AddError(error);
        return chain;
    }

    /// <summary>Fails if the nullable value is not null and greater than <paramref name="threshold"/>.</summary>
    public static RuleChain<T, TValue?> LessThanOrEqualTo<T, TValue>(
        this RuleChain<T, TValue?> chain,
        TValue threshold,
        string? message = null)
        where TValue : struct, IComparable<TValue>
    {
        if (chain.HasFailed || chain.Value is null)
            return chain;
        if (chain.Value.Value.CompareTo(threshold) > 0)
            chain.AddError(Error.Validation(
                $"{chain.PropertyName}.LessThanOrEqualTo",
                message ?? $"'{chain.PropertyName}' must be less than or equal to {threshold}."));
        return chain;
    }

    /// <summary>Fails if the nullable value is not null and greater than <paramref name="threshold"/>. Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, TValue?> LessThanOrEqualTo<T, TValue>(
        this RuleChain<T, TValue?> chain,
        TValue threshold,
        Error error)
        where TValue : struct, IComparable<TValue>
    {
        if (chain.HasFailed || chain.Value is null)
            return chain;
        if (chain.Value.Value.CompareTo(threshold) > 0)
            chain.AddError(error);
        return chain;
    }

    /// <summary>
    /// Fails if the nullable value is not null and outside the inclusive range
    /// [<paramref name="min"/>, <paramref name="max"/>].
    /// </summary>
    public static RuleChain<T, TValue?> InclusiveBetween<T, TValue>(
        this RuleChain<T, TValue?> chain,
        TValue min,
        TValue max,
        string? message = null)
        where TValue : struct, IComparable<TValue>
    {
        if (min.CompareTo(max) > 0)
            throw new ArgumentOutOfRangeException(nameof(min), $"min ({min}) must not be greater than max ({max}).");
        if (chain.HasFailed || chain.Value is null)
            return chain;
        if (chain.Value.Value.CompareTo(min) < 0
            || chain.Value.Value.CompareTo(max) > 0)
            chain.AddError(Error.Validation(
                $"{chain.PropertyName}.InclusiveBetween",
                message ?? $"'{chain.PropertyName}' must be between {min} and {max} (inclusive)."));
        return chain;
    }

    /// <summary>
    /// Fails if the nullable value is not null and outside the inclusive range
    /// [<paramref name="min"/>, <paramref name="max"/>]. Uses <paramref name="error"/> directly.
    /// </summary>
    public static RuleChain<T, TValue?> InclusiveBetween<T, TValue>(
        this RuleChain<T, TValue?> chain,
        TValue min,
        TValue max,
        Error error)
        where TValue : struct, IComparable<TValue>
    {
        if (min.CompareTo(max) > 0)
            throw new ArgumentOutOfRangeException(nameof(min), $"min ({min}) must not be greater than max ({max}).");
        if (chain.HasFailed || chain.Value is null)
            return chain;
        if (chain.Value.Value.CompareTo(min) < 0
            || chain.Value.Value.CompareTo(max) > 0)
            chain.AddError(error);
        return chain;
    }

    /// <summary>
    /// Fails if the nullable value is not null and outside the exclusive range
    /// (<paramref name="min"/>, <paramref name="max"/>).
    /// </summary>
    public static RuleChain<T, TValue?> ExclusiveBetween<T, TValue>(
        this RuleChain<T, TValue?> chain,
        TValue min,
        TValue max,
        string? message = null)
        where TValue : struct, IComparable<TValue>
    {
        if (min.CompareTo(max) >= 0)
            throw new ArgumentOutOfRangeException(nameof(min), $"min ({min}) must be less than max ({max}) for an exclusive range.");
        if (chain.HasFailed || chain.Value is null)
            return chain;
        if (chain.Value.Value.CompareTo(min) <= 0 || chain.Value.Value.CompareTo(max) >= 0)
            chain.AddError(Error.Validation(
                $"{chain.PropertyName}.ExclusiveBetween",
                message ?? $"'{chain.PropertyName}' must be between {min} and {max} (exclusive)."));
        return chain;
    }

    /// <summary>
    /// Fails if the nullable value is not null and outside the exclusive range
    /// (<paramref name="min"/>, <paramref name="max"/>). Uses <paramref name="error"/> directly.
    /// </summary>
    public static RuleChain<T, TValue?> ExclusiveBetween<T, TValue>(
        this RuleChain<T, TValue?> chain,
        TValue min,
        TValue max,
        Error error)
        where TValue : struct, IComparable<TValue>
    {
        if (min.CompareTo(max) >= 0)
            throw new ArgumentOutOfRangeException(nameof(min), $"min ({min}) must be less than max ({max}) for an exclusive range.");
        if (chain.HasFailed || chain.Value is null)
            return chain;
        if (chain.Value.Value.CompareTo(min) <= 0 || chain.Value.Value.CompareTo(max) >= 0)
            chain.AddError(error);
        return chain;
    }
}
