using CSharpEssentials.Core;

namespace CSharpEssentials.Maybe;

public readonly partial struct Maybe<T>
{
    public Maybe<T> Where(Func<T, bool> predicate)
    {
        if (HasNoValue)
            return None;

        if (predicate(Value))
            return this;

        return None;
    }

    public async Task<Maybe<T>> Where(Func<T, Task<bool>> predicate, CancellationToken cancellationToken = default)
    {
        if (HasNoValue)
            return None;

        if (await predicate(Value).WithCancellation(cancellationToken))
            return this;

        return None;
    }

    public async ValueTask<Maybe<T>> Where(Func<T, ValueTask<bool>> predicate, CancellationToken cancellationToken = default)
    {
        if (HasNoValue)
            return None;

        if (await predicate(Value).WithCancellation(cancellationToken))
            return this;

        return None;
    }
}

public static partial class MaybeExtensions
{
    /// <summary>
    /// Filters the value of the Maybe based on a predicate.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="predicate"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Maybe<T>> Where<T>(this Task<Maybe<T>> maybeTask, Func<T, bool> predicate, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.Where(predicate);
    }

    /// <summary>
    /// Filters the value of the Maybe based on a predicate.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="predicate"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Maybe<T>> Where<T>(this Task<Maybe<T>> maybeTask, Func<T, Task<bool>> predicate, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return await maybe.Where(predicate, cancellationToken);
    }

    /// <summary>
    /// Filters the value of the Maybe based on a predicate.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="predicate"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Maybe<T>> Where<T>(this ValueTask<Maybe<T>> maybeTask, Func<T, bool> predicate, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.Where(predicate);
    }

    /// <summary>
    /// Filters the value of the Maybe based on a predicate.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="predicate"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Maybe<T>> Where<T>(this ValueTask<Maybe<T>> maybeTask, Func<T, ValueTask<bool>> predicate, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return await maybe.Where(predicate, cancellationToken);
    }
}