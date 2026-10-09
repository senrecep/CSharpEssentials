using System.Runtime.CompilerServices;
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

    [OverloadResolutionPriority(1)]
    public async Task<Maybe<T>> WhereAsync(Func<T, Task<bool>> predicate, CancellationToken cancellationToken = default)
    {
        if (HasNoValue)
            return None;

        if (await predicate(Value).WithCancellation(cancellationToken).ConfigureAwait(false))
            return this;

        return None;
    }

    public async ValueTask<Maybe<T>> WhereAsync(Func<T, ValueTask<bool>> predicate, CancellationToken cancellationToken = default)
    {
        if (HasNoValue)
            return None;

        if (await predicate(Value).WithCancellation(cancellationToken).ConfigureAwait(false))
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
    public static async Task<Maybe<T>> WhereAsync<T>(this Task<Maybe<T>> maybeTask, Func<T, bool> predicate, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken).ConfigureAwait(false);
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
    public static async Task<Maybe<T>> WhereAsync<T>(this Task<Maybe<T>> maybeTask, Func<T, Task<bool>> predicate, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await maybe.WhereAsync(predicate, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Filters the value of the Maybe based on a predicate.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="predicate"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Maybe<T>> WhereAsync<T>(this ValueTask<Maybe<T>> maybeTask, Func<T, bool> predicate, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken).ConfigureAwait(false);
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
    public static async ValueTask<Maybe<T>> WhereAsync<T>(this ValueTask<Maybe<T>> maybeTask, Func<T, ValueTask<bool>> predicate, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await maybe.WhereAsync(predicate, cancellationToken).ConfigureAwait(false);
    }
}
