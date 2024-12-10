namespace CSharpEssentials;

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
        return source.FirstOrDefault() is T result ? Maybe.From(result) : Maybe.None;
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
        return source.FirstOrDefault(predicate) is T result ? Maybe.From(result) : Maybe.None;
    }
}
