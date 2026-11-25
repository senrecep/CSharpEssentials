using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

/// <summary>
/// Shared logic of <see cref="Result{T, E1}"/>, <see cref="Result{T, E1, E2}"/> and <see cref="Result{T, E1, E2, E3}"/>.
/// Tag 0 is the <see langword="default"/> value, tag 1 the success value and tag 1 + i the error of slot i.
/// </summary>
internal static class TypedResultLogic
{
    internal const byte DefaultTag = 0;
    internal const byte SuccessTag = 1;

    internal static readonly Error DefaultError = Error.Unexpected(
        code: "Result.Default",
        description: "The typed result was default-initialized and holds neither a value nor an error.");

    internal static TCase NotNull<TCase>(TCase value, string parameterName) =>
        value is null ? throw new ArgumentNullException(parameterName) : value;

    internal static InvalidOperationException DefaultInitialized() =>
        new("Result was default-initialized: it holds neither a value nor an error. Create it from a value or an error.");

    /// <summary>Throws <see cref="DefaultInitialized"/> for the <see langword="default"/> tag.</summary>
    internal static void EnsureInitialized(byte tag)
    {
        if (tag == DefaultTag)
            throw DefaultInitialized();
    }

    /// <summary>Returns the tag of a result that holds no success value, or throws for the <see langword="default"/> tag.</summary>
    internal static byte ErrorTag(byte tag) => tag == DefaultTag ? throw DefaultInitialized() : tag;

    /// <summary>Compares two cases: the tags must match, then the values for the success tag or the errors for an error tag.</summary>
    internal static bool CaseEquals<T>(byte tag, T value, object? error, byte otherTag, T otherValue, object? otherError) =>
        tag == otherTag && (tag == SuccessTag
            ? EqualityComparer<T>.Default.Equals(value, otherValue)
            : Equals(error, otherError));

    /// <summary>Hashes the tag with the value for the success tag or with the error for an error tag.</summary>
    internal static int CaseHashCode<T>(byte tag, T value, object? error) =>
        tag == SuccessTag ? HashCode.Combine(tag, value) : HashCode.Combine(tag, error);

    /// <summary>
    /// Moves the error tags of a source result to the slots of a target result. Tags 0 and 1 stay as they are.
    /// </summary>
    internal static byte Remap(byte tag, byte error1, byte error2 = DefaultTag, byte error3 = DefaultTag) => tag switch
    {
        2 => error1,
        3 => error2,
        4 => error3,
        _ => tag,
    };

    /// <summary>
    /// Converts the error of a slot to an <see cref="Error"/> and never throws: an <see cref="Error"/> stays as it is, any other
    /// <see cref="IError"/> keeps its code, description, type and metadata instance, an exception goes through
    /// <see cref="Error.Exception(System.Exception, ErrorType, ErrorMetadata?)"/>, and anything else (including a default struct error
    /// without a code) becomes an <see cref="ErrorType.Unexpected"/> error named after the slot type.
    /// </summary>
    internal static Error ToError(object error, Type slot) => error switch
    {
        Error { Code: not null, Description: not null } e => e,
        IError { Code: { } code, Description: { } description } e => Error.Create(code, description, e.Type, e.Metadata),
        Exception exception => Error.Exception(exception),
        _ => Error.Unexpected(code: slot.Name, description: "Unmapped typed error."),
    };
}
