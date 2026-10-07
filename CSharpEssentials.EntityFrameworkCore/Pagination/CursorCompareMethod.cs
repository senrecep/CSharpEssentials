using System.Reflection;

namespace CSharpEssentials.EntityFrameworkCore.Pagination;

/// <summary>Caches the <c>CompareTo(TCursor)</c> method once per cursor type for cursor pagination predicates.</summary>
internal static class CursorCompareMethod<TCursor>
    where TCursor : IComparable<TCursor>
{
    public static readonly MethodInfo Value = typeof(TCursor).GetMethod(nameof(IComparable<>.CompareTo), [typeof(TCursor)])
        ?? throw new InvalidOperationException($"{typeof(TCursor)} must expose a public CompareTo({typeof(TCursor)}) method to be used as a pagination cursor.");
}
