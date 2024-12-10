namespace CSharpEssentials;

public readonly partial struct Maybe<T>
{
    /// <summary>
    /// Maps the value of the Maybe to a new value.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="selector"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Maybe<TOut>> MapAsync<TOut>(Func<T, Task<TOut>> selector, CancellationToken cancellationToken = default)
    {
        if (HasNoValue)
            return Maybe.None;

        return await selector(Value).WithCancellation(cancellationToken);
    }

    /// <summary>
    /// Maps the value of the Maybe to a new value.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="valueTask"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Maybe<TOut>> MapAsync<TOut>(Func<T, ValueTask<TOut>> valueTask, CancellationToken cancellationToken = default)
    {
        if (HasNoValue)
            return Maybe.None;

        return await valueTask(Value).WithCancellation(cancellationToken);
    }

    /// <summary>
    /// Maps the value of the Maybe to a new value.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="selector"></param>
    /// <returns></returns>
    public Maybe<TOut> Map<TOut>(Func<T, TOut> selector)
    {
        if (HasNoValue)
            return Maybe.None;

        return selector(Value);
    }
}

public static partial class MaybeExtensions
{
    /// <summary>
    /// Maps the value of the Maybe to a new value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="selector"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Maybe<TOut>> MapAsync<T, TOut>(this Task<Maybe<T>> maybeTask, Func<T, TOut> selector, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.Map(selector);
    }

    /// <summary>
    /// Maps the value of the Maybe to a new value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="selector"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Maybe<TOut>> MapAsync<T, TOut>(this Task<Maybe<T>> maybeTask, Func<T, Task<TOut>> selector, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return await maybe.MapAsync(selector, cancellationToken);
    }

    /// <summary>
    /// Maps the value of the Maybe to a new value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="valueTask"></param>
    /// <param name="selector"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Maybe<TOut>> MapAsync<T, TOut>(this ValueTask<Maybe<T>> valueTask, Func<T, TOut> selector, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await valueTask.WithCancellation(cancellationToken);
        return maybe.Map(selector);
    }


    /// <summary>
    /// Maps the value of the Maybe to a new value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="valueTask"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Maybe<TOut>> MapAsync<T, TOut>(this ValueTask<Maybe<T>> maybeTask, Func<T, ValueTask<TOut>> valueTask, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return await maybe.MapAsync(valueTask, cancellationToken);
    }
}