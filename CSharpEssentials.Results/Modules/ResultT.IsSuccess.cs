using System.Diagnostics.CodeAnalysis;
using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result<TValue>
{
    /// <summary>
    /// Attempts to extract the success value and errors from the result.
    /// </summary>
    /// <param name="value">The success value when <see cref="IsSuccess"/> is <c>true</c>; otherwise, <c>default</c>.</param>
    /// <param name="errors">The errors when <see cref="IsFailure"/> is <c>true</c>; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if the result is successful; otherwise, <c>false</c>.</returns>
    public bool TryGet([MaybeNullWhen(false)] out TValue value, out Error[]? errors)
    {
        if (IsSuccess)
        {
            value = Value;
            errors = null;
            return true;
        }

        value = default;
        errors = _errors;
        return false;
    }
}
