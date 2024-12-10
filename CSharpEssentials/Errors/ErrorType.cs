namespace CSharpEssentials;

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