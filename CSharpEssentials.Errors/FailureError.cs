namespace CSharpEssentials.Errors;

/// <summary>
/// Typed error of kind <see cref="ErrorType.Failure"/>. A general failure. Maps to HTTP 500.
/// Derive from it to give an error its own type while keeping the kind.
/// </summary>
[ErrorKind(ErrorType.Failure)]
public record FailureError : TypedError
{
    /// <summary>
    /// Creates a <see cref="FailureError"/>.
    /// </summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="description">The error description.</param>
    /// <param name="metadata">Optional extra information.</param>
    public FailureError(string code, string description, ErrorMetadata? metadata = null)
        : base(code, description, metadata)
    {
    }

    /// <inheritdoc />
    public sealed override ErrorType Type => ErrorType.Failure;
}
