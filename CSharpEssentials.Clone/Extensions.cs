using System.Diagnostics.CodeAnalysis;

namespace CSharpEssentials.Clone;

public static class Extensions
{
    private const string QueryableMessage =
        "Wrapping the cloned sequence in an EnumerableQuery compiles expression trees at run time. Use Clone<T>(IEnumerable<T>) in trimmed or native AOT applications.";

    public static IEnumerable<T> Clone<T>(this IEnumerable<T> source)
        where T : ICloneable<T> => source.Select(static item => item.Clone());
    [RequiresUnreferencedCode(QueryableMessage)]
    [RequiresDynamicCode(QueryableMessage)]
    public static IQueryable<T> Clone<T>(this IQueryable<T> source)
        where T : ICloneable<T> => source.AsEnumerable().Clone().AsQueryable();
}
