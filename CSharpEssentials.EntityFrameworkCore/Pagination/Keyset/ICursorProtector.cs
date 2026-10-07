namespace CSharpEssentials.EntityFrameworkCore.Pagination.Keyset;

/// <summary>
/// Protects keyset cursors against tampering, for example by wrapping an ASP.NET Core <c>IDataProtector</c>.
/// The protected value is sent to clients as is, so it should be URL-safe.
/// </summary>
public interface ICursorProtector
{
    /// <summary>Protects an encoded cursor.</summary>
    string Protect(string cursor);

    /// <summary>
    /// Returns <see langword="true"/> and the original cursor when <paramref name="protectedCursor"/> is valid.
    /// Must return <see langword="false"/> instead of throwing for a tampered or malformed value.
    /// </summary>
    bool TryUnprotect(string protectedCursor, out string cursor);
}
