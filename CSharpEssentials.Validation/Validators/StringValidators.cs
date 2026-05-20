using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using CSharpEssentials.Errors;

namespace CSharpEssentials.Validation.Validators;

/// <summary>Extension validators for nullable <see langword="string"/> properties.</summary>
public static class StringValidators
{
    // -------------------------------------------------------------------------
    // Presence
    // -------------------------------------------------------------------------

    /// <summary>Fails if the value is <see langword="null"/>, empty, or whitespace.</summary>
    public static RuleChain<T, string?> NotEmpty<T>(
        this RuleChain<T, string?> chain,
        string? message = null)
    {
        if (!chain.HasFailed && string.IsNullOrWhiteSpace(chain.Value))
            chain.AddError(Error.Validation(
                $"{chain.PropertyName}.NotEmpty",
                message ?? $"'{chain.PropertyName}' must not be empty."));
        return chain;
    }

    /// <summary>Fails if the value is <see langword="null"/>, empty, or whitespace. Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, string?> NotEmpty<T>(
        this RuleChain<T, string?> chain,
        Error error)
    {
        if (!chain.HasFailed && string.IsNullOrWhiteSpace(chain.Value))
            chain.AddError(error);
        return chain;
    }

    /// <summary>Fails if the value is <see langword="null"/>.</summary>
    public static RuleChain<T, string?> NotNull<T>(
        this RuleChain<T, string?> chain,
        string? message = null)
    {
        if (!chain.HasFailed && chain.Value is null)
            chain.AddError(Error.Validation(
                $"{chain.PropertyName}.NotNull",
                message ?? $"'{chain.PropertyName}' must not be null."));
        return chain;
    }

    /// <summary>Fails if the value is <see langword="null"/>. Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, string?> NotNull<T>(
        this RuleChain<T, string?> chain,
        Error error)
    {
        if (!chain.HasFailed && chain.Value is null)
            chain.AddError(error);
        return chain;
    }

    // -------------------------------------------------------------------------
    // Length
    // -------------------------------------------------------------------------

    /// <summary>Fails if the value length exceeds <paramref name="max"/>.</summary>
    public static RuleChain<T, string?> MaxLength<T>(
        this RuleChain<T, string?> chain,
        int max,
        string? message = null)
    {
        if (max < 0)
            throw new ArgumentOutOfRangeException(nameof(max), $"max ({max}) must not be negative.");
        if (!chain.HasFailed && chain.Value?.Length > max)
            chain.AddError(Error.Validation(
                $"{chain.PropertyName}.MaxLength",
                message ?? $"'{chain.PropertyName}' must be {max} characters or fewer. You entered {chain.Value.Length} characters."));
        return chain;
    }

    /// <summary>Fails if the value length exceeds <paramref name="max"/>. Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, string?> MaxLength<T>(
        this RuleChain<T, string?> chain,
        int max,
        Error error)
    {
        if (max < 0)
            throw new ArgumentOutOfRangeException(nameof(max), $"max ({max}) must not be negative.");
        if (!chain.HasFailed && chain.Value?.Length > max)
            chain.AddError(error);
        return chain;
    }

    /// <summary>Fails if the value length is less than <paramref name="min"/>.</summary>
    public static RuleChain<T, string?> MinLength<T>(
        this RuleChain<T, string?> chain,
        int min,
        string? message = null)
    {
        if (min < 0)
            throw new ArgumentOutOfRangeException(nameof(min), $"min ({min}) must not be negative.");
        if (!chain.HasFailed && chain.Value is not null && chain.Value.Length < min)
            chain.AddError(Error.Validation(
                $"{chain.PropertyName}.MinLength",
                message ?? $"'{chain.PropertyName}' must be at least {min} characters. You entered {chain.Value.Length} characters."));
        return chain;
    }

    /// <summary>Fails if the value length is less than <paramref name="min"/>. Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, string?> MinLength<T>(
        this RuleChain<T, string?> chain,
        int min,
        Error error)
    {
        if (min < 0)
            throw new ArgumentOutOfRangeException(nameof(min), $"min ({min}) must not be negative.");
        if (!chain.HasFailed && chain.Value is not null && chain.Value.Length < min)
            chain.AddError(error);
        return chain;
    }

    /// <summary>Fails if the value length is not between <paramref name="min"/> and <paramref name="max"/> (inclusive).</summary>
    public static RuleChain<T, string?> Length<T>(
        this RuleChain<T, string?> chain,
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
        int len = chain.Value.Length;
        if (len < min || len > max)
            chain.AddError(Error.Validation(
                $"{chain.PropertyName}.Length",
                message ?? $"'{chain.PropertyName}' must be between {min} and {max} characters. You entered {len} characters."));
        return chain;
    }

    /// <summary>Fails if the value length is not between <paramref name="min"/> and <paramref name="max"/> (inclusive). Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, string?> Length<T>(
        this RuleChain<T, string?> chain,
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
        int len = chain.Value.Length;
        if (len < min || len > max)
            chain.AddError(error);
        return chain;
    }

    // -------------------------------------------------------------------------
    // Format
    // -------------------------------------------------------------------------

    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        TimeSpan.FromMilliseconds(100));

    // Cache is unbounded — only pass known, finite sets of patterns. See Matches(string, ...) remarks.
    private static readonly ConcurrentDictionary<string, Regex> _regexCache = new();

    private static Regex GetOrCreateRegex(string pattern) =>
        _regexCache.GetOrAdd(pattern, p => new Regex(p,
            RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(100)));

    /// <summary>Fails if the value is not a valid e-mail address format.</summary>
    public static RuleChain<T, string?> EmailAddress<T>(
        this RuleChain<T, string?> chain,
        string? message = null)
    {
        if (!chain.HasFailed && chain.Value is not null)
        {
            bool isMatch;
            try
            {
                isMatch = EmailRegex.IsMatch(chain.Value);
            }
            catch (RegexMatchTimeoutException)
            {
                isMatch = false;
            }
            if (!isMatch)
                chain.AddError(Error.Validation(
                    $"{chain.PropertyName}.EmailAddress",
                    message ?? $"'{chain.PropertyName}' is not a valid email address."));
        }
        return chain;
    }

    /// <summary>Fails if the value is not a valid e-mail address format. Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, string?> EmailAddress<T>(
        this RuleChain<T, string?> chain,
        Error error)
    {
        if (!chain.HasFailed && chain.Value is not null)
        {
            bool isMatch;
            try
            {
                isMatch = EmailRegex.IsMatch(chain.Value);
            }
            catch (RegexMatchTimeoutException)
            {
                isMatch = false;
            }
            if (!isMatch)
                chain.AddError(error);
        }
        return chain;
    }

    /// <summary>Fails if the value does not match <paramref name="pattern"/>.</summary>
    /// <remarks>
    /// The compiled <see cref="Regex"/> for <paramref name="pattern"/> is cached statically and never evicted.
    /// Do not pass user-supplied or dynamically generated patterns — use the <see cref="Regex"/> overload with
    /// a pre-compiled instance instead.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown if <paramref name="pattern"/> is not a valid regular expression.</exception>
    public static RuleChain<T, string?> Matches<T>(
        this RuleChain<T, string?> chain,
        string pattern,
        string? message = null)
    {
        if (!chain.HasFailed && chain.Value is not null)
        {
            bool isMatch;
            try
            {
                isMatch = GetOrCreateRegex(pattern).IsMatch(chain.Value);
            }
            catch (RegexMatchTimeoutException)
            {
                isMatch = false;
            }
            if (!isMatch)
                chain.AddError(Error.Validation(
                    $"{chain.PropertyName}.Matches",
                    message ?? $"'{chain.PropertyName}' is not in the correct format."));
        }
        return chain;
    }

    /// <summary>Fails if the value does not match <paramref name="pattern"/>. Uses <paramref name="error"/> directly.</summary>
    /// <remarks>
    /// The compiled <see cref="Regex"/> for <paramref name="pattern"/> is cached statically and never evicted.
    /// Do not pass user-supplied or dynamically generated patterns — use the <see cref="Regex"/> overload with
    /// a pre-compiled instance instead.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown if <paramref name="pattern"/> is not a valid regular expression.</exception>
    public static RuleChain<T, string?> Matches<T>(
        this RuleChain<T, string?> chain,
        string pattern,
        Error error)
    {
        if (!chain.HasFailed && chain.Value is not null)
        {
            bool isMatch;
            try
            {
                isMatch = GetOrCreateRegex(pattern).IsMatch(chain.Value);
            }
            catch (RegexMatchTimeoutException)
            {
                isMatch = false;
            }
            if (!isMatch)
                chain.AddError(error);
        }
        return chain;
    }

    /// <summary>Fails if the value does not match the compiled <paramref name="regex"/>.</summary>
    public static RuleChain<T, string?> Matches<T>(
        this RuleChain<T, string?> chain,
        Regex regex,
        string? message = null)
    {
        if (!chain.HasFailed && chain.Value is not null)
        {
            bool isMatch;
            try
            {
                isMatch = regex.IsMatch(chain.Value);
            }
            catch (RegexMatchTimeoutException)
            {
                isMatch = false;
            }
            if (!isMatch)
                chain.AddError(Error.Validation(
                    $"{chain.PropertyName}.Matches",
                    message ?? $"'{chain.PropertyName}' is not in the correct format."));
        }
        return chain;
    }

    /// <summary>Fails if the value does not match the compiled <paramref name="regex"/>. Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, string?> Matches<T>(
        this RuleChain<T, string?> chain,
        Regex regex,
        Error error)
    {
        if (!chain.HasFailed && chain.Value is not null)
        {
            bool isMatch;
            try
            {
                isMatch = regex.IsMatch(chain.Value);
            }
            catch (RegexMatchTimeoutException)
            {
                isMatch = false;
            }
            if (!isMatch)
                chain.AddError(error);
        }
        return chain;
    }

    // -------------------------------------------------------------------------
    // Equality
    // -------------------------------------------------------------------------

    /// <summary>Fails if the value is not equal to <paramref name="expected"/> (ordinal comparison).</summary>
    public static RuleChain<T, string?> Equal<T>(
        this RuleChain<T, string?> chain,
        string? expected,
        string? message = null)
    {
        if (!chain.HasFailed && !string.Equals(chain.Value, expected, StringComparison.Ordinal))
            chain.AddError(Error.Validation(
                $"{chain.PropertyName}.Equal",
                message ?? $"'{chain.PropertyName}' must be equal to '{expected}'."));
        return chain;
    }

    /// <summary>Fails if the value is not equal to <paramref name="expected"/> (ordinal comparison). Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, string?> Equal<T>(
        this RuleChain<T, string?> chain,
        string? expected,
        Error error)
    {
        if (!chain.HasFailed && !string.Equals(chain.Value, expected, StringComparison.Ordinal))
            chain.AddError(error);
        return chain;
    }

    /// <summary>Fails if the value is equal to <paramref name="forbidden"/> (ordinal comparison).</summary>
    public static RuleChain<T, string?> NotEqual<T>(
        this RuleChain<T, string?> chain,
        string? forbidden,
        string? message = null)
    {
        if (!chain.HasFailed && string.Equals(chain.Value, forbidden, StringComparison.Ordinal))
            chain.AddError(Error.Validation(
                $"{chain.PropertyName}.NotEqual",
                message ?? $"'{chain.PropertyName}' must not be equal to '{forbidden}'."));
        return chain;
    }

    /// <summary>Fails if the value is equal to <paramref name="forbidden"/> (ordinal comparison). Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, string?> NotEqual<T>(
        this RuleChain<T, string?> chain,
        string? forbidden,
        Error error)
    {
        if (!chain.HasFailed && string.Equals(chain.Value, forbidden, StringComparison.Ordinal))
            chain.AddError(error);
        return chain;
    }

    /// <summary>Fails if the value is not equal to <paramref name="expected"/>.</summary>
    public static RuleChain<T, string?> Equal<T>(
        this RuleChain<T, string?> chain,
        string? expected,
        StringComparison comparison,
        string? message = null)
    {
        if (!chain.HasFailed && !string.Equals(chain.Value, expected, comparison))
            chain.AddError(Error.Validation(
                $"{chain.PropertyName}.Equal",
                message ?? $"'{chain.PropertyName}' must be equal to '{expected}'."));
        return chain;
    }

    /// <summary>Fails if the value is not equal to <paramref name="expected"/>. Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, string?> Equal<T>(
        this RuleChain<T, string?> chain,
        string? expected,
        StringComparison comparison,
        Error error)
    {
        if (!chain.HasFailed && !string.Equals(chain.Value, expected, comparison))
            chain.AddError(error);
        return chain;
    }

    /// <summary>Fails if the value is equal to <paramref name="forbidden"/>.</summary>
    public static RuleChain<T, string?> NotEqual<T>(
        this RuleChain<T, string?> chain,
        string? forbidden,
        StringComparison comparison,
        string? message = null)
    {
        if (!chain.HasFailed && string.Equals(chain.Value, forbidden, comparison))
            chain.AddError(Error.Validation(
                $"{chain.PropertyName}.NotEqual",
                message ?? $"'{chain.PropertyName}' must not be equal to '{forbidden}'."));
        return chain;
    }

    /// <summary>Fails if the value is equal to <paramref name="forbidden"/>. Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, string?> NotEqual<T>(
        this RuleChain<T, string?> chain,
        string? forbidden,
        StringComparison comparison,
        Error error)
    {
        if (!chain.HasFailed && string.Equals(chain.Value, forbidden, comparison))
            chain.AddError(error);
        return chain;
    }

    // -------------------------------------------------------------------------
    // Content
    // -------------------------------------------------------------------------

    /// <summary>Fails if the value does not contain <paramref name="substring"/>.</summary>
    public static RuleChain<T, string?> Contains<T>(
        this RuleChain<T, string?> chain,
        string substring,
        StringComparison comparison = StringComparison.Ordinal,
        string? message = null)
    {
        if (!chain.HasFailed && chain.Value is not null && !chain.Value.Contains(substring, comparison))
            chain.AddError(Error.Validation(
                $"{chain.PropertyName}.Contains",
                message ?? $"'{chain.PropertyName}' must contain '{substring}'."));
        return chain;
    }

    /// <summary>Fails if the value does not contain <paramref name="substring"/>. Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, string?> Contains<T>(
        this RuleChain<T, string?> chain,
        string substring,
        Error error,
        StringComparison comparison = StringComparison.Ordinal)
    {
        if (!chain.HasFailed && chain.Value is not null && !chain.Value.Contains(substring, comparison))
            chain.AddError(error);
        return chain;
    }

    /// <summary>Fails if the value does not start with <paramref name="prefix"/>.</summary>
    public static RuleChain<T, string?> StartsWith<T>(
        this RuleChain<T, string?> chain,
        string prefix,
        StringComparison comparison = StringComparison.Ordinal,
        string? message = null)
    {
        if (!chain.HasFailed && chain.Value is not null && !chain.Value.StartsWith(prefix, comparison))
            chain.AddError(Error.Validation(
                $"{chain.PropertyName}.StartsWith",
                message ?? $"'{chain.PropertyName}' must start with '{prefix}'."));
        return chain;
    }

    /// <summary>Fails if the value does not start with <paramref name="prefix"/>. Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, string?> StartsWith<T>(
        this RuleChain<T, string?> chain,
        string prefix,
        Error error,
        StringComparison comparison = StringComparison.Ordinal)
    {
        if (!chain.HasFailed && chain.Value is not null && !chain.Value.StartsWith(prefix, comparison))
            chain.AddError(error);
        return chain;
    }

    /// <summary>Fails if the value does not end with <paramref name="suffix"/>.</summary>
    public static RuleChain<T, string?> EndsWith<T>(
        this RuleChain<T, string?> chain,
        string suffix,
        StringComparison comparison = StringComparison.Ordinal,
        string? message = null)
    {
        if (!chain.HasFailed && chain.Value is not null && !chain.Value.EndsWith(suffix, comparison))
            chain.AddError(Error.Validation(
                $"{chain.PropertyName}.EndsWith",
                message ?? $"'{chain.PropertyName}' must end with '{suffix}'."));
        return chain;
    }

    /// <summary>Fails if the value does not end with <paramref name="suffix"/>. Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, string?> EndsWith<T>(
        this RuleChain<T, string?> chain,
        string suffix,
        Error error,
        StringComparison comparison = StringComparison.Ordinal)
    {
        if (!chain.HasFailed && chain.Value is not null && !chain.Value.EndsWith(suffix, comparison))
            chain.AddError(error);
        return chain;
    }
}
