#if NET5_0_OR_GREATER
using System.Globalization;
#endif
using CSharpEssentials.Core;
using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public static class StringResultExtensions
{
    /// <summary>
    /// Removes <paramref name="prefixToRemove"/> from the start of <paramref name="input"/> when it matches.
    /// A culture-aware match removes every character the comparison consumed, including ignorable ones.
    /// </summary>
    /// <param name="input">The string to trim. Null, empty or whitespace-only input yields an <c>InputIsEmpty</c> validation error.</param>
    /// <param name="prefixToRemove">The prefix to remove. Null, empty or whitespace-only values yield a <c>PrefixIsEmpty</c> validation error.</param>
    /// <param name="comparisonType">The comparison used to match the prefix.</param>
    /// <returns>The trimmed string, or <paramref name="input"/> unchanged when it does not start with the prefix.</returns>
    public static Result<string> TrimStart(
        this string? input,
        string prefixToRemove,
        StringComparison comparisonType = StringComparison.CurrentCulture)
    {
        if (input.IsEmpty())
            return Error.Validation(code: "InputIsEmpty", description: "Input is null or empty.");
        if (prefixToRemove.IsEmpty())
            return Error.Validation(code: "PrefixIsEmpty", description: "Prefix is null or empty.");
#if NET5_0_OR_GREATER
        if (TryGetCultureComparison(comparisonType, out CompareInfo? compareInfo, out CompareOptions options))
            return compareInfo.IsPrefix(input.AsSpan(), prefixToRemove.AsSpan(), options, out int matchLength) ? input[matchLength..] : input;
#endif
        return input.StartsWith(prefixToRemove, comparisonType) ? input[prefixToRemove.Length..] : input;
    }

    /// <summary>
    /// Removes <paramref name="suffixToRemove"/> from the end of <paramref name="input"/> when it matches.
    /// A culture-aware match removes every character the comparison consumed, including ignorable ones.
    /// </summary>
    /// <param name="input">The string to trim. Null, empty or whitespace-only input yields an <c>InputIsEmpty</c> validation error.</param>
    /// <param name="suffixToRemove">The suffix to remove. Null, empty or whitespace-only values yield a <c>SuffixIsEmpty</c> validation error.</param>
    /// <param name="comparisonType">The comparison used to match the suffix.</param>
    /// <returns>The trimmed string, or <paramref name="input"/> unchanged when it does not end with the suffix.</returns>
    public static Result<string> TrimEnd(
        this string? input,
        string suffixToRemove,
        StringComparison comparisonType = StringComparison.CurrentCulture)
    {
        if (input.IsEmpty())
            return Error.Validation(code: "InputIsEmpty", description: "Input is null or empty.");
        if (suffixToRemove.IsEmpty())
            return Error.Validation(code: "SuffixIsEmpty", description: "Suffix is null or empty.");
#if NET5_0_OR_GREATER
        if (TryGetCultureComparison(comparisonType, out CompareInfo? compareInfo, out CompareOptions options))
            return compareInfo.IsSuffix(input.AsSpan(), suffixToRemove.AsSpan(), options, out int matchLength) ? input[..^matchLength] : input;
#endif
        return input.EndsWith(suffixToRemove, comparisonType) ? input[..^suffixToRemove.Length] : input;
    }

#if NET5_0_OR_GREATER
    private static bool TryGetCultureComparison(
        StringComparison comparisonType,
        [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out CompareInfo? compareInfo,
        out CompareOptions options)
    {
        compareInfo = comparisonType switch
        {
            StringComparison.CurrentCulture or StringComparison.CurrentCultureIgnoreCase => CultureInfo.CurrentCulture.CompareInfo,
            StringComparison.InvariantCulture or StringComparison.InvariantCultureIgnoreCase => CultureInfo.InvariantCulture.CompareInfo,
            StringComparison.Ordinal or StringComparison.OrdinalIgnoreCase => null,
            _ => null,
        };
        options = comparisonType is StringComparison.CurrentCultureIgnoreCase or StringComparison.InvariantCultureIgnoreCase
            ? CompareOptions.IgnoreCase
            : CompareOptions.None;
        return compareInfo is not null;
    }
#endif
}
