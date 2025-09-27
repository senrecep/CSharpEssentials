namespace CSharpEssentials.Maybe;

public static partial class MaybeExtensions
{
    /// <summary>
    /// Tries to get the last value in a sequence.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static Maybe<T> TryLast<T>(this IEnumerable<T> source)
    {
        Maybe<T> last = Maybe.None;
        foreach (T? item in source)
            last = item;

        return last;
    }

    /// <summary>
    /// Tries to get the last value in a sequence that satisfies a predicate.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static Maybe<T> TryLast<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        Maybe<T> last = Maybe.None;
        foreach (T? item in source)
            if (predicate(item))
                last = item;

        return last;
    }
}
