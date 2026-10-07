namespace CSharpEssentials.EntityFrameworkCore.Pagination.Responses;

/// <summary>
/// A page of a keyset pagination. <see cref="Items"/> are always in the canonical order of the keys, for
/// <c>Before</c> pages too. Pass <see cref="NextCursor"/> as <c>After</c> and <see cref="PreviousCursor"/> as
/// <c>Before</c> to move between pages; each cursor is non-null exactly when the matching <c>Has*</c> flag is set.
/// </summary>
public sealed record KeysetPaginationResponse<T>(
    IReadOnlyList<T> Items,
    string? NextCursor,
    string? PreviousCursor)
{
    /// <summary>Whether a following page exists; <see cref="NextCursor"/> is then set.</summary>
    public bool HasNext => NextCursor is not null;

    /// <summary>Whether a preceding page exists; <see cref="PreviousCursor"/> is then set.</summary>
    public bool HasPrevious => PreviousCursor is not null;
}
