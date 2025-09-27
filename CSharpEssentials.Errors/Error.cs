using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace CSharpEssentials.Errors;

public readonly record struct Error : IError
{
    [JsonConstructor]
    private Error(string code, string description, ErrorType type, ErrorMetadata? metadata)
    {
        Code = code;
        Description = description;
        Type = type;
        NumericType = type.ToIntType();
        Metadata = metadata;
    }

    /// <summary>
    /// Gets the unique error code.
    /// </summary>
    public readonly string Code { get; }

    /// <summary>
    /// Gets the error description.
    /// </summary>
    public readonly string Description { get; }

    /// <summary>
    /// Gets the error type.
    /// </summary>
    public readonly ErrorType Type { get; }

    /// <summary>
    /// Gets the numeric value of the type.
    /// </summary>
    public readonly int NumericType { get; }

    /// <summary>
    /// Gets the metadata.
    /// </summary>
    public readonly ErrorMetadata? Metadata { get; }

    /// <summary>
    /// Creates an <see cref="Error"/> from a code and description.
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error[] CreateMany(params IEnumerable<Error> errors) => [.. errors];
    /// <summary>
    /// Creates an <see cref="Error"/> from a code and description.
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error[] CreateMany(params IEnumerable<IEnumerable<Error>> errors) => [.. errors.SelectMany(x => x)];


    /// <summary>
    /// Creates an <see cref="Error"/> of type <see cref="ErrorType.Failure"/> from a code and description.
    /// </summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="description">The error description.</param>
    /// <param name="metadata">A dictionary which provides optional space for information.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error Failure(
        string code = nameof(Failure),
        string description = "A failure has occurred.",
        ErrorMetadata? metadata = null) =>
            new(code, description, ErrorType.Failure, metadata);

    /// <summary>
    /// Creates an <see cref="Error"/> of type <see cref="ErrorType.Unexpected"/> from a code and description.
    /// </summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="description">The error description.</param>
    /// <param name="metadata">A dictionary which provides optional space for information.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error Unexpected(
        string code = nameof(Unexpected),
        string description = "An unexpected error has occurred.",
        ErrorMetadata? metadata = null) =>
            new(code, description, ErrorType.Unexpected, metadata);

    /// <summary>
    /// Creates an <see cref="Error"/> of type <see cref="ErrorType.Validation"/> from a code and description.
    /// </summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="description">The error description.</param>
    /// <param name="metadata">A dictionary which provides optional space for information.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error Validation(
        string code = nameof(Validation),
        string description = "A validation error has occurred.",
        ErrorMetadata? metadata = null) =>
            new(code, description, ErrorType.Validation, metadata);

    /// <summary>
    /// Creates an <see cref="Error"/> of type <see cref="ErrorType.Conflict"/> from a code and description.
    /// </summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="description">The error description.</param>
    /// <param name="metadata">A dictionary which provides optional space for information.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error Conflict(
        string code = nameof(Conflict),
        string description = "A conflict error has occurred.",
        ErrorMetadata? metadata = null) =>
            new(code, description, ErrorType.Conflict, metadata);


    /// <summary>
    /// Creates an <see cref="Error"/> of type <see cref="ErrorType.NotFound"/> from a code and description.
    /// </summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="description">The error description.</param>
    /// <param name="metadata">A dictionary which provides optional space for information.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error NotFound(
        string code = nameof(NotFound),
        string description = "A 'Not Found' error has occurred.",
        ErrorMetadata? metadata = null) =>
            new(code, description, ErrorType.NotFound, metadata);

    /// <summary>
    /// Creates an <see cref="Error"/> of type <see cref="ErrorType.Unauthorized"/> from a code and description.
    /// </summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="description">The error description.</param>
    /// <param name="metadata">A dictionary which provides optional space for information.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error Unauthorized(
        string code = nameof(Unauthorized),
        string description = "An 'Unauthorized' error has occurred.",
        ErrorMetadata? metadata = null) =>
            new(code, description, ErrorType.Unauthorized, metadata);

    /// <summary>
    /// Creates an <see cref="Error"/> of type <see cref="ErrorType.Forbidden"/> from a code and description.
    /// </summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="description">The error description.</param>
    /// <param name="metadata">A dictionary which provides optional space for information.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error Forbidden(
        string code = nameof(Forbidden),
        string description = "A 'Forbidden' error has occurred.",
        ErrorMetadata? metadata = null) =>
        new(code, description, ErrorType.Forbidden, metadata);

    /// <summary>
    /// Creates an <see cref="Error"/> of type <see cref="ErrorType.Failure"/> from a code and description.
    /// </summary>
    /// <param name="exception"></param>
    /// <param name="type"></param>
    /// <param name="metadata"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error Exception(
        Exception exception,
        ErrorType type = ErrorType.Failure,
        ErrorMetadata? metadata = null) =>
        new(exception.GetType().Name, exception.Message, type, ErrorMetadata.CreateWithExceptionDetailed(exception).Combine(metadata));
    /// <summary>
    /// Creates an <see cref="Error"/> of type <see cref="ErrorType.Failure"/> from a code and description.
    /// </summary>
    /// <param name="code"></param>
    /// <param name="exception"></param>
    /// <param name="type"></param>
    /// <param name="metadata"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error Exception(
        string code,
        Exception exception,
        ErrorType type = ErrorType.Failure,
        ErrorMetadata? metadata = null) =>
            new(code, exception.Message, type, ErrorMetadata.CreateWithExceptionDetailed(exception).Combine(metadata));

    /// <summary>
    /// Creates an <see cref="Error"/> of type <see cref="ErrorType.Failure"/> from a code and description.
    /// </summary>
    /// <param name="code"></param>
    /// <param name="description"></param>
    /// <param name="exception"></param>
    /// <param name="type"></param>
    /// <param name="metadata"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error Exception(
        string code,
        string description,
        Exception exception,
        ErrorType type = ErrorType.Failure,
        ErrorMetadata? metadata = null) =>
            new(code, description, type, ErrorMetadata.CreateWithExceptionDetailed(exception).Combine(metadata));

    public static readonly Error NoFirstError = Unexpected(
        code: "Result.NoFirstError",
        description: "First error cannot be retrieved from a successful Result.");

    public static readonly Error NoLastError = Unexpected(
        code: "Result.NoLastError",
        description: "Last error cannot be retrieved from a successful Result.");

    public static readonly Error NoErrors = Unexpected(
        code: "Result.NoErrors",
        description: "Error array cannot be retrieved from a successful Result.");

    public static readonly Error False = Validation(
        code: nameof(False),
        description: "A false error has occurred.");

    public bool Equals(Error other)
    {
        if (Type != other.Type ||
            NumericType != other.NumericType ||
            Code != other.Code ||
            Description != other.Description)
            return false;

        if (Metadata is null)
            return other.Metadata is null;

        return other.Metadata is not null && CompareMetadata(Metadata, other.Metadata);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        hashCode.Add(Code);
        hashCode.Add(Description);
        hashCode.Add(Type);
        hashCode.Add(NumericType);

        if (Metadata is null)
            return hashCode.ToHashCode();

        foreach (KeyValuePair<string, object?> keyValuePair in Metadata)
        {
            hashCode.Add(keyValuePair.Key);
            hashCode.Add(keyValuePair.Value);
        }

        return hashCode.ToHashCode();
    }


    private static bool CompareMetadata(ErrorMetadata metadata, ErrorMetadata otherMetadata)
    {
        if (ReferenceEquals(metadata, otherMetadata))
            return true;

        if (metadata.Count != otherMetadata.Count)
            return false;

        foreach (KeyValuePair<string, object?> keyValuePair in metadata)
            if (!otherMetadata.TryGetValue(keyValuePair.Key, out object? otherValue) ||
                !Equals(keyValuePair.Value, otherValue))
                return false;

        return true;
    }
}
