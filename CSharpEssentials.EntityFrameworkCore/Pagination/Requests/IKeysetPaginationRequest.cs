namespace CSharpEssentials.EntityFrameworkCore.Pagination.Requests;

/// <summary>
/// A keyset pagination request. Set at most one of <see cref="After"/> and <see cref="Before"/>; with neither set the
/// first page is returned.
/// </summary>
public interface IKeysetPaginationRequest
{
    /// <summary>Requested page size. It is raised to 1 and lowered to the configured maximum limit.</summary>
    int Limit { get; }

    /// <summary>A <c>NextCursor</c> from an earlier page: returns the rows after it.</summary>
    string? After { get; }

    /// <summary>A <c>PreviousCursor</c> from an earlier page: returns the rows before it.</summary>
    string? Before { get; }
}
