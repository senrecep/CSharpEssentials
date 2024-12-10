namespace CSharpEssentials.Clone;

public static class Extensions
{
    private static T Clone<T>(this T source)
        where T : ICloneable<T> => source.Clone();
    public static IEnumerable<T> Clone<T>(this IEnumerable<T> source)
        where T : ICloneable<T> => source.Select(Clone);

    public static IQueryable<T> Clone<T>(this IQueryable<T> source)
        where T : ICloneable<T> => source.Select(Clone).AsQueryable();
}
