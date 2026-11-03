using System.Diagnostics.CodeAnalysis;

namespace CSharpEssentials.Clone;

public static class Extensions
{
    private const string QueryableMessage =
        "Wrapping the cloned sequence in an EnumerableQuery compiles expression trees at run time. Use Clone<T>(IEnumerable<T>) in trimmed or native AOT applications.";

    private static T Clone<T>(this T source)
        where T : ICloneable<T> => source.Clone();
    public static IEnumerable<T> Clone<T>(this IEnumerable<T> source)
        where T : ICloneable<T> => source.Select(Clone);
    [RequiresUnreferencedCode(QueryableMessage)]
    [RequiresDynamicCode(QueryableMessage)]
    public static IQueryable<T> Clone<T>(this IQueryable<T> source)
        where T : ICloneable<T> => source.Select(Clone).AsQueryable();
}
