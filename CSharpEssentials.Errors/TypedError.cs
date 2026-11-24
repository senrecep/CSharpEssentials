namespace CSharpEssentials.Errors;

/// <summary>
/// Base type of the typed error kinds (<see cref="NotFoundError"/>, <see cref="ConflictError"/> and the rest).
/// The CLR type of an error tells its kind, so callers can match on the type instead of on <see cref="Type"/>.
/// </summary>
/// <remarks>
/// Two typed errors are equal when they have the same runtime type, code, description and type, and metadata
/// with equal content (compared the same way <see cref="Error"/> compares it). A derived error never equals its
/// base kind, even with the same code. Equality does not survive serialization when metadata holds values:
/// they come back as <c>JsonElement</c>. Compare on <see cref="Code"/> to tell errors apart.
/// </remarks>
public abstract record TypedError : IError
{
    /// <summary>
    /// Creates a typed error.
    /// </summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="description">The error description.</param>
    /// <param name="metadata">Optional extra information.</param>
    /// <exception cref="ArgumentNullException"><paramref name="code"/> or <paramref name="description"/> is null.</exception>
    protected TypedError(string code, string description, ErrorMetadata? metadata = null)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Metadata = metadata;
    }

    /// <summary>
    /// Gets the unique error code.
    /// </summary>
    public string Code { get; init => field = value ?? throw new ArgumentNullException(nameof(Code)); }

    /// <summary>
    /// Gets the error description.
    /// </summary>
    public string Description { get; init => field = value ?? throw new ArgumentNullException(nameof(Description)); }

    /// <summary>
    /// Gets the error type of this kind.
    /// </summary>
    public abstract ErrorType Type { get; }

    /// <summary>
    /// Gets the numeric value of <see cref="Type"/>.
    /// </summary>
    public int NumericType => Type.ToIntType();

    /// <summary>
    /// Gets the metadata.
    /// </summary>
    public ErrorMetadata? Metadata { get; init; }

    /// <summary>
    /// Converts this error to an <see cref="Error"/> with the same code, description, type and metadata instance.
    /// </summary>
    public Error ToError() => Error.Create(Code, Description, Type, Metadata);

    /// <inheritdoc />
    public virtual bool Equals(TypedError? other) =>
        ReferenceEquals(this, other) ||
        other is not null && EqualityContract == other.EqualityContract && ToError().Equals(other.ToError());

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(EqualityContract, ToError());
}
