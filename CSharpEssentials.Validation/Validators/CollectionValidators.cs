using CSharpEssentials.Errors;

namespace CSharpEssentials.Validation.Validators;

/// <summary>
/// Extension validators for collection properties (<see cref="IEnumerable{T}"/>).
/// </summary>
public static class CollectionValidators
{
    private static bool IsEmpty<TElement>(IEnumerable<TElement> value) =>
        value switch
        {
            ICollection<TElement> col => col.Count == 0,
            IReadOnlyCollection<TElement> rcol => rcol.Count == 0,
            _ => !value.Any()
        };

    private static int ResolveCount<TElement>(IEnumerable<TElement> value) =>
        value switch
        {
            ICollection<TElement> col => col.Count,
            IReadOnlyCollection<TElement> rcol => rcol.Count,
            _ => value.Count()
        };

    /// <summary>Fails if the collection is <see langword="null"/> or contains no elements.</summary>
    public static RuleChain<T, IEnumerable<TElement>?> NotEmpty<T, TElement>(
        this RuleChain<T, IEnumerable<TElement>?> chain,
        string? message = null)
    {
        if (!chain.HasFailed && (chain.Value is null || IsEmpty(chain.Value)))
            chain.AddError(Error.Validation(
                $"{chain.PropertyName}.NotEmpty",
                message ?? $"'{chain.PropertyName}' must not be empty."));
        return chain;
    }

    /// <summary>Fails if the collection is <see langword="null"/> or contains no elements. Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, IEnumerable<TElement>?> NotEmpty<T, TElement>(
        this RuleChain<T, IEnumerable<TElement>?> chain,
        Error error)
    {
        if (!chain.HasFailed && (chain.Value is null || IsEmpty(chain.Value)))
            chain.AddError(error);
        return chain;
    }

    /// <summary>Fails if the collection is <see langword="null"/>.</summary>
    public static RuleChain<T, IEnumerable<TElement>?> NotNull<T, TElement>(
        this RuleChain<T, IEnumerable<TElement>?> chain,
        string? message = null)
    {
        if (!chain.HasFailed && chain.Value is null)
            chain.AddError(Error.Validation(
                $"{chain.PropertyName}.NotNull",
                message ?? $"'{chain.PropertyName}' must not be null."));
        return chain;
    }

    /// <summary>Fails if the collection is <see langword="null"/>. Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, IEnumerable<TElement>?> NotNull<T, TElement>(
        this RuleChain<T, IEnumerable<TElement>?> chain,
        Error error)
    {
        if (!chain.HasFailed && chain.Value is null)
            chain.AddError(error);
        return chain;
    }

    /// <summary>Fails if the collection is not null and has fewer than <paramref name="min"/> elements.</summary>
    public static RuleChain<T, IEnumerable<TElement>?> MinCount<T, TElement>(
        this RuleChain<T, IEnumerable<TElement>?> chain,
        int min,
        string? message = null)
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
    public static RuleChain<T, IEnumerable<TElement>?> MinCount<T, TElement>(
        this RuleChain<T, IEnumerable<TElement>?> chain,
        int min,
        Error error)
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
    public static RuleChain<T, IEnumerable<TElement>?> MaxCount<T, TElement>(
        this RuleChain<T, IEnumerable<TElement>?> chain,
        int max,
        string? message = null)
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
    public static RuleChain<T, IEnumerable<TElement>?> MaxCount<T, TElement>(
        this RuleChain<T, IEnumerable<TElement>?> chain,
        int max,
        Error error)
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
    public static RuleChain<T, IEnumerable<TElement>?> CountBetween<T, TElement>(
        this RuleChain<T, IEnumerable<TElement>?> chain,
        int min,
        int max,
        string? message = null)
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
    public static RuleChain<T, IEnumerable<TElement>?> CountBetween<T, TElement>(
        this RuleChain<T, IEnumerable<TElement>?> chain,
        int min,
        int max,
        Error error)
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
