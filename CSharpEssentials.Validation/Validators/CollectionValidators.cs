using System.Collections;
using CSharpEssentials.Errors;

namespace CSharpEssentials.Validation.Validators;

/// <summary>
/// Extension validators for nullable collection properties.
/// Works with any collection type that implements <see cref="IEnumerable"/>:
/// <see cref="System.Collections.Generic.List{T}"/>, arrays, <see cref="System.Collections.Generic.IEnumerable{T}"/>,
/// <see cref="System.Collections.Generic.IList{T}"/>, <see cref="System.Collections.Generic.IReadOnlyList{T}"/>, etc.
/// </summary>
public static class CollectionValidators
{
    private static bool IsEmpty(IEnumerable value)
    {
        if (value is ICollection col)
            return col.Count == 0;
        IEnumerator e = value.GetEnumerator();
        try
        {
            return !e.MoveNext();
        }
        finally
        {
            (e as IDisposable)?.Dispose();
        }
    }

    private static int ResolveCount(IEnumerable value)
    {
        if (value is ICollection col)
            return col.Count;
        int count = 0;
        IEnumerator e = value.GetEnumerator();
        try
        {
            while (e.MoveNext())
                count++;
        }
        finally
        {
            (e as IDisposable)?.Dispose();
        }
        return count;
    }

    /// <summary>Fails if the collection is <see langword="null"/> or contains no elements.</summary>
    public static RuleChain<T, TCollection?> NotEmpty<T, TCollection>(
        this RuleChain<T, TCollection?> chain,
        string? message = null)
        where TCollection : class, IEnumerable
    {
        if (!chain.HasFailed && (chain.Value is null || IsEmpty(chain.Value)))
            chain.AddError(Error.Validation(
                $"{chain.PropertyName}.NotEmpty",
                message ?? $"'{chain.PropertyName}' must not be empty."));
        return chain;
    }

    /// <summary>Fails if the collection is <see langword="null"/> or contains no elements. Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, TCollection?> NotEmpty<T, TCollection>(
        this RuleChain<T, TCollection?> chain,
        Error error)
        where TCollection : class, IEnumerable
    {
        if (!chain.HasFailed && (chain.Value is null || IsEmpty(chain.Value)))
            chain.AddError(error);
        return chain;
    }

    /// <summary>Fails if the collection is <see langword="null"/>.</summary>
    public static RuleChain<T, TCollection?> NotNull<T, TCollection>(
        this RuleChain<T, TCollection?> chain,
        string? message = null)
        where TCollection : class, IEnumerable
    {
        if (!chain.HasFailed && chain.Value is null)
            chain.AddError(Error.Validation(
                $"{chain.PropertyName}.NotNull",
                message ?? $"'{chain.PropertyName}' must not be null."));
        return chain;
    }

    /// <summary>Fails if the collection is <see langword="null"/>. Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, TCollection?> NotNull<T, TCollection>(
        this RuleChain<T, TCollection?> chain,
        Error error)
        where TCollection : class, IEnumerable
    {
        if (!chain.HasFailed && chain.Value is null)
            chain.AddError(error);
        return chain;
    }

    /// <summary>Fails if the collection is not null and has fewer than <paramref name="min"/> elements.</summary>
    public static RuleChain<T, TCollection?> MinCount<T, TCollection>(
        this RuleChain<T, TCollection?> chain,
        int min,
        string? message = null)
        where TCollection : class, IEnumerable
    {
        if (min < 0)
            throw new ArgumentOutOfRangeException(nameof(min), $"min ({min}) must not be negative.");
        if (chain.HasFailed || chain.Value is null)
            return chain;
        int count = ResolveCount(chain.Value);
        if (count < min)
            chain.AddError(Error.Validation(
                $"{chain.PropertyName}.MinCount",
                message ?? $"'{chain.PropertyName}' must contain at least {min} item(s). It contains {count}."));
        return chain;
    }

    /// <summary>Fails if the collection is not null and has fewer than <paramref name="min"/> elements. Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, TCollection?> MinCount<T, TCollection>(
        this RuleChain<T, TCollection?> chain,
        int min,
        Error error)
        where TCollection : class, IEnumerable
    {
        if (min < 0)
            throw new ArgumentOutOfRangeException(nameof(min), $"min ({min}) must not be negative.");
        if (chain.HasFailed || chain.Value is null)
            return chain;
        int count = ResolveCount(chain.Value);
        if (count < min)
            chain.AddError(error);
        return chain;
    }

    /// <summary>Fails if the collection is not null and has more than <paramref name="max"/> elements.</summary>
    public static RuleChain<T, TCollection?> MaxCount<T, TCollection>(
        this RuleChain<T, TCollection?> chain,
        int max,
        string? message = null)
        where TCollection : class, IEnumerable
    {
        if (max < 0)
            throw new ArgumentOutOfRangeException(nameof(max), $"max ({max}) must not be negative.");
        if (chain.HasFailed || chain.Value is null)
            return chain;
        int count = ResolveCount(chain.Value);
        if (count > max)
            chain.AddError(Error.Validation(
                $"{chain.PropertyName}.MaxCount",
                message ?? $"'{chain.PropertyName}' must contain at most {max} item(s). It contains {count}."));
        return chain;
    }

    /// <summary>Fails if the collection is not null and has more than <paramref name="max"/> elements. Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, TCollection?> MaxCount<T, TCollection>(
        this RuleChain<T, TCollection?> chain,
        int max,
        Error error)
        where TCollection : class, IEnumerable
    {
        if (max < 0)
            throw new ArgumentOutOfRangeException(nameof(max), $"max ({max}) must not be negative.");
        if (chain.HasFailed || chain.Value is null)
            return chain;
        int count = ResolveCount(chain.Value);
        if (count > max)
            chain.AddError(error);
        return chain;
    }

    /// <summary>
    /// Fails if the collection is not null and its element count is outside the inclusive range
    /// [<paramref name="min"/>, <paramref name="max"/>].
    /// </summary>
    public static RuleChain<T, TCollection?> CountBetween<T, TCollection>(
        this RuleChain<T, TCollection?> chain,
        int min,
        int max,
        string? message = null)
        where TCollection : class, IEnumerable
    {
        if (min < 0)
            throw new ArgumentOutOfRangeException(nameof(min), $"min ({min}) must not be negative.");
        if (min > max)
            throw new ArgumentOutOfRangeException(nameof(min), $"min ({min}) must not be greater than max ({max}).");
        if (chain.HasFailed || chain.Value is null)
            return chain;
        int count = ResolveCount(chain.Value);
        if (count < min || count > max)
            chain.AddError(Error.Validation(
                $"{chain.PropertyName}.CountBetween",
                message ?? $"'{chain.PropertyName}' must contain between {min} and {max} item(s). It contains {count}."));
        return chain;
    }

    /// <summary>
    /// Fails if the collection is not null and its element count is outside the inclusive range
    /// [<paramref name="min"/>, <paramref name="max"/>]. Uses <paramref name="error"/> directly.
    /// </summary>
    public static RuleChain<T, TCollection?> CountBetween<T, TCollection>(
        this RuleChain<T, TCollection?> chain,
        int min,
        int max,
        Error error)
        where TCollection : class, IEnumerable
    {
        if (min < 0)
            throw new ArgumentOutOfRangeException(nameof(min), $"min ({min}) must not be negative.");
        if (min > max)
            throw new ArgumentOutOfRangeException(nameof(min), $"min ({min}) must not be greater than max ({max}).");
        if (chain.HasFailed || chain.Value is null)
            return chain;
        int count = ResolveCount(chain.Value);
        if (count < min || count > max)
            chain.AddError(error);
        return chain;
    }
}
