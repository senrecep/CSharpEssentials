using System.Text.Json.Serialization;

namespace CSharpEssentials.Errors;

/// <summary>
/// Typed error of kind <see cref="ErrorType.Validation"/>. The input failed validation. Maps to HTTP 400.
/// Derive from it to give an error its own type while keeping the kind.
/// </summary>
[ErrorKind(ErrorType.Validation)]
public record ValidationError : TypedError
{
    /// <summary>
    /// Creates a <see cref="ValidationError"/>.
    /// </summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="description">The error description.</param>
    /// <param name="metadata">Optional extra information.</param>
    public ValidationError(string code, string description, ErrorMetadata? metadata = null)
        : base(code, description, metadata)
    {
    }

    /// <summary>
    /// The property path the error belongs to, read from the <see cref="ErrorMetadata.FieldKey"/> metadata entry.
    /// Set when the error is built from a validation failure, for example
    /// <c>new ValidationError(error.Code, error.Description, error.Metadata)</c>; <see langword="null"/> otherwise.
    /// </summary>
    [JsonIgnore]
    public string? Field => Metadata?.GetField();

    /// <inheritdoc />
    public sealed override ErrorType Type => ErrorType.Validation;
}
