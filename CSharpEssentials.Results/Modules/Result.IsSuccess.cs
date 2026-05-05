using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result
{
    /// <summary>
    /// Attempts to extract the errors from the result.
    /// </summary>
    /// <param name="errors">The errors when <see cref="IsFailure"/> is <c>true</c>; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if the result is successful; otherwise, <c>false</c>.</returns>
    public bool TryGet(out Error[]? errors)
    {
        if (IsSuccess)
        {
            errors = null;
            return true;
        }

        errors = _errors;
        return false;
    }
}
