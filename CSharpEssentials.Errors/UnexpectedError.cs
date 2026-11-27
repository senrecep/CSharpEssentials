namespace CSharpEssentials.Errors;

/// <summary>
/// Typed error of kind <see cref="ErrorType.Unexpected"/>. An unexpected error. Maps to HTTP 500.
/// Derive from it to give an error its own type while keeping the kind.
/// </summary>
[ErrorKind(ErrorType.Unexpected)]
public record UnexpectedError : TypedError
{
    /// <summary>
    /// Creates a <see cref="UnexpectedError"/>.
    /// </summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="description">The error description.</param>
    /// <param name="metadata">Optional extra information.</param>
    public UnexpectedError(string code, string description, ErrorMetadata? metadata = null)
        : base(code, description, metadata)
    {
    }

    /// <inheritdoc />
    public sealed override ErrorType Type => ErrorType.Unexpected;
}
