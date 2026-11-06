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
        if (source is IList<T> list)
            return list.Count == 0 ? Maybe<T>.None : list[list.Count - 1];
        if (source is IReadOnlyList<T> readOnlyList)
            return readOnlyList.Count == 0 ? Maybe<T>.None : readOnlyList[readOnlyList.Count - 1];

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
        if (source is IList<T> list)
        {
            for (int i = list.Count - 1; i >= 0; i--)
                if (predicate(list[i]))
                    return list[i];
            return Maybe<T>.None;
        }
        if (source is IReadOnlyList<T> readOnlyList)
        {
            for (int i = readOnlyList.Count - 1; i >= 0; i--)
                if (predicate(readOnlyList[i]))
                    return readOnlyList[i];
            return Maybe<T>.None;
        }

        Maybe<T> last = Maybe.None;
        foreach (T? item in source)
            if (predicate(item))
                last = item;

        return last;
    }
}
