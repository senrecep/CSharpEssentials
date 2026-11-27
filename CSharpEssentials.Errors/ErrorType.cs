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

    /// <summary>
    /// The type is not known: a value this version does not define, or an error whose cause was not classified.
    /// Data reads map unknown names and numbers to it.
    /// </summary>
    [EnumFallback]
    Unknown,

    /// <summary>
    /// A business or domain rule rejected a well-formed request, such as a declined payment or an order that can no
    /// longer be cancelled. Maps to HTTP 422.
    /// </summary>
    Domain
}
