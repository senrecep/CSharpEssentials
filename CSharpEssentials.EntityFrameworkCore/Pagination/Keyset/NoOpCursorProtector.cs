namespace CSharpEssentials.EntityFrameworkCore.Pagination.Keyset;

/// <summary>
/// Default <see cref="ICursorProtector"/> that leaves the cursor unchanged. The cursor is then only base64url encoded:
/// clients can read and change it, but a changed cursor that no longer decodes is still rejected.
/// </summary>
public sealed class NoOpCursorProtector : ICursorProtector
{
    /// <summary>The shared instance.</summary>
    public static NoOpCursorProtector Instance { get; } = new();

    /// <summary>Returns <paramref name="cursor"/> unchanged.</summary>
    public string Protect(string cursor) => cursor;

    /// <summary>Returns <paramref name="protectedCursor"/> unchanged and always succeeds.</summary>
    public bool TryUnprotect(string protectedCursor, out string cursor)
    {
        cursor = protectedCursor;
        return true;
    }
}
