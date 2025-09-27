namespace CSharpEssentials.Maybe;

public static partial class MaybeExtensions
{
    /// <summary>
    /// Tries to find a value in a dictionary.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="dict"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static Maybe<TValue> TryFind<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dict, TKey key)
    {
        if (dict.ContainsKey(key))
        {
            return dict[key];
        }
        return Maybe.None;
    }
}
