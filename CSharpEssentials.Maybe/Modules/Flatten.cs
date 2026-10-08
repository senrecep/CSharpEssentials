namespace CSharpEssentials.Maybe;

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

    /// <summary>
    /// Turns a nullable Maybe into a Maybe: <see langword="null"/> becomes <see cref="Maybe{T}.None"/>.
    /// </summary>
    /// <typeparam name="T">The value type of the Maybe.</typeparam>
    /// <param name="maybe">The nullable Maybe, for example a <c>Maybe&lt;T&gt;?</c> property mapped to a nullable column.</param>
    /// <returns>The Maybe, or <see cref="Maybe{T}.None"/> when <paramref name="maybe"/> is <see langword="null"/>.</returns>
    public static Maybe<T> Flatten<T>(this Maybe<T>? maybe) => maybe ?? Maybe<T>.None;
}
