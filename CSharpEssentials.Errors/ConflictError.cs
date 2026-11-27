namespace CSharpEssentials.Errors;

/// <summary>
/// Typed error of kind <see cref="ErrorType.Conflict"/>. The request conflicts with the current state. Maps to HTTP 409.
/// Derive from it to give an error its own type while keeping the kind.
/// </summary>
[ErrorKind(ErrorType.Conflict)]
public record ConflictError : TypedError
{
    /// <summary>
    /// Creates a <see cref="ConflictError"/>.
    /// </summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="description">The error description.</param>
    /// <param name="metadata">Optional extra information.</param>
    public ConflictError(string code, string description, ErrorMetadata? metadata = null)
        : base(code, description, metadata)
    {
    }

    /// <inheritdoc />
    public sealed override ErrorType Type => ErrorType.Conflict;
}
