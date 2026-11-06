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
        if (source is IList<T> list)
            return list.Count == 0 ? Maybe<T>.None : list[0];
        if (source is IReadOnlyList<T> readOnlyList)
            return readOnlyList.Count == 0 ? Maybe<T>.None : readOnlyList[0];

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
        if (source is T[] array)
        {
            foreach (T item in array)
                if (predicate(item))
                    return item;
            return Maybe.None;
        }
        if (source is List<T> concreteList)
        {
            for (int i = 0; i < concreteList.Count; i++)
            {
                T item = concreteList[i];
                if (predicate(item))
                    return item;
            }
            return Maybe.None;
        }
        if (source is IList<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                T item = list[i];
                if (predicate(item))
                    return item;
            }
            return Maybe.None;
        }
        if (source is IReadOnlyList<T> readOnlyList)
        {
            for (int i = 0; i < readOnlyList.Count; i++)
            {
                T item = readOnlyList[i];
                if (predicate(item))
                    return item;
            }
            return Maybe.None;
        }

        foreach (T? item in source)
            if (predicate(item))
                return item;

        return Maybe.None;
    }
}
