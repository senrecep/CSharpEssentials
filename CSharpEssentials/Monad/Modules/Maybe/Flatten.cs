namespace CSharpEssentials.Monad.Extensions.Maybe;

public static partial class MaybeExtensions
{
    /// <summary>
    /// Flattens a Maybe of a Maybe into a single Maybe.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybe"></param>
    /// <returns></returns>
    public static Maybe<T> Flatten<T>(in this Maybe<Maybe<T>> maybe)
    {
        return maybe.GetValueOrDefault();
    }
}
