using CSharpEssentials.EntityFrameworkCore.Pagination.Keyset;

namespace CSharpEssentials.Tests.EntityFrameworkCore.Keyset;

/// <summary>Test protector that marks cursors with a prefix and rejects cursors without it.</summary>
public sealed class PrefixCursorProtector : ICursorProtector
{
    public const string Prefix = "sig.";

    public string Protect(string cursor) => Prefix + cursor;

    public bool TryUnprotect(string protectedCursor, out string cursor)
    {
        if (protectedCursor.StartsWith(Prefix, StringComparison.Ordinal))
        {
            cursor = protectedCursor[Prefix.Length..];
            return true;
        }

        cursor = string.Empty;
        return false;
    }
}
