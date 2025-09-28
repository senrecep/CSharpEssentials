using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern.Comparers;

namespace CSharpEssentials.ResultPattern.Interfaces;

public interface IResultBase
{
    static readonly IEqualityComparer<IResultBase> Comparer = new ResultBaseComparer();
    /// <summary>
    /// Determines if the result is a failure.
    /// </summary>
    bool IsFailure { get; }
    /// <summary>
    /// Determines if the result is a success.
    /// </summary>
    bool IsSuccess { get; }
    /// <summary>
    /// The first error in the result.
    /// </summary>
    Error FirstError { get; }
    /// <summary>
    /// The last error in the result.
    /// </summary>
    Error LastError { get; }
    /// <summary>
    /// The errors in the result or an no errors.
    /// </summary>
    Error[] Errors { get; }
    /// <summary>
    /// The errors in the result or an empty array.
    /// </summary>
    Error[] ErrorsOrEmptyArray { get; }
}
