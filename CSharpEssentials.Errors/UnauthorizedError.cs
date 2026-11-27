namespace CSharpEssentials.Errors;

/// <summary>
/// Typed error of kind <see cref="ErrorType.Unauthorized"/>. The caller is not authenticated. Maps to HTTP 401.
/// Derive from it to give an error its own type while keeping the kind.
/// </summary>
[ErrorKind(ErrorType.Unauthorized)]
public record UnauthorizedError : TypedError
{
    /// <summary>
    /// Creates a <see cref="UnauthorizedError"/>.
    /// </summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="description">The error description.</param>
    /// <param name="metadata">Optional extra information.</param>
    public UnauthorizedError(string code, string description, ErrorMetadata? metadata = null)
        : base(code, description, metadata)
    {
    }

    /// <inheritdoc />
    public sealed override ErrorType Type => ErrorType.Unauthorized;
}
