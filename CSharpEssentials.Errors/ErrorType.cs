using CSharpEssentials.Enums;

namespace CSharpEssentials.Errors;

/// <summary>
/// Error types.
/// </summary>
[StringEnum]
public enum ErrorType
{
    Failure,
    Unexpected,
    Validation,
    Conflict,
    NotFound,
    Unauthorized,
    Forbidden,
    Unknown
}