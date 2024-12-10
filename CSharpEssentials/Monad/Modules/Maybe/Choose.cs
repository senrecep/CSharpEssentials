using System.Runtime.CompilerServices;

namespace CSharpEssentials;

public static partial class MaybeExtensions
{
    /// <summary>
    ///  Projects the value of a Maybe into a new form if it has a value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    public static IEnumerable<TOut> Choose<T, TOut>(this IEnumerable<Maybe<T>> source, Func<T, TOut> selector)
    {
        foreach (Maybe<T> item in source)
            if (item.HasValue)
                yield return selector(item.Value);
    }
    /// <summary>
    /// Projects the value of a Maybe into a new form if it has a value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static IEnumerable<T> Choose<T>(this IEnumerable<Maybe<T>> source)
    {
        foreach (Maybe<T> item in source)
            if (item.HasValue)
                yield return item.Value;
    }

    /// <summary>
    /// Projects the value of a Maybe into a new form if it has a value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async IAsyncEnumerable<T> ChooseAsync<T>(this IEnumerable<Task<Maybe<T>>> source, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (Task<Maybe<T>>? task in Task.WhenEach(source).WithCancellation(cancellationToken))
        {
            Maybe<T> result = await task;
            if (result.HasValue)
                yield return result.Value;
        }
    }

    /// <summary>
    /// Projects the value of a Maybe into a new form if it has a value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async IAsyncEnumerable<TOut> ChooseAsync<T, TOut>(this IEnumerable<Task<Maybe<T>>> source, Func<T, TOut> selector, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (Task<Maybe<T>>? task in Task.WhenEach(source).WithCancellation(cancellationToken))
        {
            Maybe<T> result = await task;
            if (result.HasValue)
                yield return selector(result.Value);
        }
    }
}
