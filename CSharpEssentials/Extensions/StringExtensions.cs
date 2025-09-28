using System;
using CSharpEssentials.Core;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Core;

public static class StringExtensions
{
    public static Result<string> TrimStart(
    this string? input,
    string prefixToRemove,
    StringComparison comparisonType = StringComparison.CurrentCulture)
    {
        if (input.IsEmpty())
            return Error.Validation(code: "InputIsEmpty", description: "Input is null or empty.");
        if (prefixToRemove.IsEmpty())
            return Error.Validation(code: "PrefixIsEmpty", description: "Prefix is null or empty.");
        return input.StartsWith(prefixToRemove, comparisonType) ? input[prefixToRemove.Length..] : input;
    }

    public static Result<string> TrimEnd(
        this string? input,
        string suffixToRemove,
        StringComparison comparisonType = StringComparison.CurrentCulture)
    {
        if (input.IsEmpty())
            return Error.Validation(code: "InputIsEmpty", description: "Input is null or empty.");
        if (suffixToRemove.IsEmpty())
            return Error.Validation(code: "SuffixIsEmpty", description: "Suffix is null or empty.");
        return input.EndsWith(suffixToRemove, comparisonType) ? input[..^suffixToRemove.Length] : input;
    }

}
