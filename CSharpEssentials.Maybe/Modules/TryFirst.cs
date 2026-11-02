namespace CSharpEssentials.Maybe;

public static partial class MaybeExtensions
{
    /// <summary>
    /// Tries to get the first value in a sequence.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static Maybe<T> TryFirst<T>(this IEnumerable<T> source)
    {
        using IEnumerator<T> enumerator = source.GetEnumerator();
        return enumerator.MoveNext() ? enumerator.Current : Maybe.None;
    }

    /// <summary>
    /// Tries to get the first value in a sequence that satisfies a predicate.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static Maybe<T> TryFirst<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        foreach (T? item in source)
            if (predicate(item))
                return item;

        return Maybe.None;
    }
}
