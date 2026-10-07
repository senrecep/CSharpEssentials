using CSharpEssentials.Errors;

namespace CSharpEssentials.EntityFrameworkCore.Pagination.Keyset;

/// <summary>
/// Codes of the <see cref="ErrorType.Validation"/> errors that keyset pagination returns for a bad request.
/// The error metadata carries the request parameter under <see cref="ParameterKey"/> (<c>after</c> or <c>before</c>).
/// </summary>
public static class KeysetCursorErrors
{
    /// <summary>Metadata key holding the request parameter of a cursor error: <c>after</c> or <c>before</c>.</summary>
    public const string ParameterKey = "parameter";

    /// <summary>The cursor is too long, malformed, tampered with, or rejected by the <see cref="ICursorProtector"/>.</summary>
    public const string InvalidCode = "KeysetPagination.InvalidCursor";

    /// <summary>The cursor was written by another cursor format version.</summary>
    public const string UnsupportedVersionCode = "KeysetPagination.UnsupportedCursorVersion";

    /// <summary>A <c>NextCursor</c> was sent as <c>Before</c>, or a <c>PreviousCursor</c> as <c>After</c>.</summary>
    public const string DirectionMismatchCode = "KeysetPagination.CursorDirectionMismatch";

    /// <summary>The cursor was issued for another entity type or another set of keys, key types or directions.</summary>
    public const string KeyMismatchCode = "KeysetPagination.CursorKeyMismatch";

    /// <summary>Both <c>After</c> and <c>Before</c> were set.</summary>
    public const string AfterAndBeforeCode = "KeysetPagination.AfterAndBefore";

    internal static Error Invalid(string parameter) =>
        Create(InvalidCode, $"The '{parameter}' cursor is malformed or has been tampered with.", parameter);

    internal static Error UnsupportedVersion(string parameter) =>
        Create(UnsupportedVersionCode, $"The '{parameter}' cursor was created by an unsupported cursor format version.", parameter);

    internal static Error DirectionMismatch(string parameter) =>
        Create(DirectionMismatchCode, $"The '{parameter}' cursor was issued for the other paging direction.", parameter);

    internal static Error KeyMismatch(string parameter) =>
        Create(KeyMismatchCode, $"The '{parameter}' cursor was issued for a different ordering.", parameter);

    internal static Error AfterAndBefore() =>
        Error.Validation(AfterAndBeforeCode, "Only one of the 'after' and 'before' cursors can be set.");

    private static Error Create(string code, string description, string parameter) =>
        Error.Validation(code, description, new ErrorMetadata(ParameterKey, parameter));
}
