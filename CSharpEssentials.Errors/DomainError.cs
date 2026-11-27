namespace CSharpEssentials.Errors;

/// <summary>
/// Typed error of kind <see cref="ErrorType.Domain"/>. A business rule rejected a well-formed request. Maps to HTTP 422.
/// Derive from it to give an error its own type while keeping the kind.
/// </summary>
[ErrorKind(ErrorType.Domain)]
public record DomainError : TypedError
{
    /// <summary>
    /// Creates a <see cref="DomainError"/>.
    /// </summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="description">The error description.</param>
    /// <param name="metadata">Optional extra information.</param>
    public DomainError(string code, string description, ErrorMetadata? metadata = null)
        : base(code, description, metadata)
    {
    }

    /// <inheritdoc />
    public sealed override ErrorType Type => ErrorType.Domain;
}
