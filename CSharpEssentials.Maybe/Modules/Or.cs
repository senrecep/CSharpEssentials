using CSharpEssentials.Core;

namespace CSharpEssentials.Maybe;


public readonly partial struct Maybe<T>
{
    /// <summary>
    /// Returns the value if it exists, otherwise returns the fallback value.
    /// </summary>
    /// <param name="fallbackOperation"></param>
    /// <returns></returns>
    public Maybe<T> Or(Func<T> fallbackOperation) =>
        HasNoValue ? (Maybe<T>)fallbackOperation() : this;

    /// <summary>
    /// Returns the value if it exists, otherwise returns the fallback value.
    /// </summary>
    /// <param name="fallback"></param>
    /// <returns></returns>
    public Maybe<T> Or(Maybe<T> fallback) =>
        HasNoValue ? fallback : this;

    /// <summary>
    /// Returns the value if it exists, otherwise returns the fallback value.
    /// </summary>
    /// <param name="fallbackOperation"></param>
    /// <returns></returns>
    public Maybe<T> Or(Func<Maybe<T>> fallbackOperation) =>
        HasNoValue ? fallbackOperation() : this;

    /// <summary>
    /// Returns the value if it exists, otherwise returns the fallback value.
    /// </summary>
    /// <param name="fallbackOperation"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Maybe<T>> Or(Func<Task<T>> fallbackOperation, CancellationToken cancellationToken = default)
    {
        if (HasNoValue)
            return await fallbackOperation().WithCancellation(cancellationToken);

        return this;
    }

    /// <summary>
    /// Returns the value if it exists, otherwise returns the fallback value.
    /// </summary>
    /// <param name="fallback"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Maybe<T>> Or(Task<Maybe<T>> fallback, CancellationToken cancellationToken = default)
    {
        if (HasNoValue)
            return await fallback.WithCancellation(cancellationToken);

        return this;
    }

    /// <summary>
    /// Returns the value if it exists, otherwise returns the fallback value.
    /// </summary>
    /// <param name="fallbackOperation"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Maybe<T>> Or(Func<Task<Maybe<T>>> fallbackOperation, CancellationToken cancellationToken = default)
    {
        if (HasNoValue)
            return await fallbackOperation().WithCancellation(cancellationToken);

        return this;
    }

    /// <summary>
    /// Returns the value if it exists, otherwise returns the fallback value.
    /// </summary>
    /// <param name="valueTaskFallbackOperation"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Maybe<T>> Or(Func<ValueTask<T>> valueTaskFallbackOperation, CancellationToken cancellationToken = default)
    {
        if (HasNoValue)
            return await valueTaskFallbackOperation().WithCancellation(cancellationToken);

        return this;
    }

    /// <summary>
    /// Returns the value if it exists, otherwise returns the fallback value.
    /// </summary>
    /// <param name="valueTaskFallback"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Maybe<T>> Or(ValueTask<Maybe<T>> valueTaskFallback, CancellationToken cancellationToken = default)
    {
        if (HasNoValue)
            return await valueTaskFallback.WithCancellation(cancellationToken);

        return this;
    }

    /// <summary>
    /// Returns the value if it exists, otherwise returns the fallback value.
    /// </summary>
    /// <param name="valueTaskFallbackOperation"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Maybe<T>> Or(Func<ValueTask<Maybe<T>>> valueTaskFallbackOperation, CancellationToken cancellationToken = default)
    {
        if (HasNoValue)
            return await valueTaskFallbackOperation().WithCancellation(cancellationToken);

        return this;
    }

}

public static partial class MaybeExtensions
{

    /// <summary>
    /// Returns the value if it exists, otherwise returns the fallback value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="fallback"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Maybe<T>> Or<T>(this Task<Maybe<T>> maybeTask, T fallback, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);

        if (maybe.HasNoValue)
            return fallback;

        return maybe;
    }

    /// <summary>
    /// Returns the value if it exists, otherwise returns the fallback value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="fallbackOperation"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Maybe<T>> Or<T>(this Task<Maybe<T>> maybeTask, Func<T> fallbackOperation, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);

        if (maybe.HasNoValue)
            return fallbackOperation();

        return maybe;
    }

    /// <summary>
    /// Returns the value if it exists, otherwise returns the fallback value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="fallback"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Maybe<T>> Or<T>(this Task<Maybe<T>> maybeTask, Maybe<T> fallback, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);

        if (maybe.HasNoValue)
            return fallback;

        return maybe;
    }


    /// <summary>
    /// Returns the value if it exists, otherwise returns the fallback value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="fallback"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Maybe<T>> Or<T>(this Task<Maybe<T>> maybeTask, Task<T> fallback, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);

        if (maybe.HasNoValue)
        {
            T? value = await fallback.WithCancellation(cancellationToken);
            return value;
        }

        return maybe;
    }

    /// <summary>
    /// Returns the value if it exists, otherwise returns the fallback value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="fallbackOperation"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Maybe<T>> Or<T>(this Task<Maybe<T>> maybeTask, Func<Task<T>> fallbackOperation, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);

        if (maybe.HasNoValue)
        {
            T? value = await fallbackOperation().WithCancellation(cancellationToken);

            return value;
        }

        return maybe;
    }

    /// <summary>
    /// Returns the value if it exists, otherwise returns the fallback value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="fallbackOperation"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Maybe<T>> Or<T>(this Task<Maybe<T>> maybeTask, Func<Maybe<T>> fallbackOperation, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);

        if (maybe.HasNoValue)
            return fallbackOperation();

        return maybe;
    }

    /// <summary>
    /// Returns the value if it exists, otherwise returns the fallback value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="fallbackOperation"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Maybe<T>> Or<T>(this Task<Maybe<T>> maybeTask, Func<Task<Maybe<T>>> fallbackOperation, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);

        if (maybe.HasNoValue)
            return await fallbackOperation().WithCancellation(cancellationToken);

        return maybe;
    }

    /// <summary>
    /// Returns the value if it exists, otherwise returns the fallback value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="fallback"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Maybe<T>> Or<T>(this ValueTask<Maybe<T>> maybeTask, T fallback, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);

        if (maybe.HasNoValue)
            return fallback;

        return maybe;
    }

    /// <summary>
    /// Returns the value if it exists, otherwise returns the fallback value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="fallbackOperation"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Maybe<T>> Or<T>(this ValueTask<Maybe<T>> maybeTask, Func<T> fallbackOperation, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);

        if (maybe.HasNoValue)
            return fallbackOperation();

        return maybe;
    }

    /// <summary>
    /// Returns the value if it exists, otherwise returns the fallback value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="fallback"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Maybe<T>> Or<T>(this ValueTask<Maybe<T>> maybeTask, Maybe<T> fallback, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);

        if (maybe.HasNoValue)
            return fallback;

        return maybe;
    }

    /// <summary>
    /// Returns the value if it exists, otherwise returns the fallback value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="fallbackOperation"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Maybe<T>> Or<T>(this ValueTask<Maybe<T>> maybeTask, Func<Maybe<T>> fallbackOperation, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);

        if (maybe.HasNoValue)
            return fallbackOperation();

        return maybe;
    }

    /// <summary>
    /// Returns the value if it exists, otherwise returns the fallback value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="fallback"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Maybe<T>> Or<T>(this ValueTask<Maybe<T>> maybeTask, ValueTask<T> fallback, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);

        if (maybe.HasNoValue)
        {
            T? value = await fallback.WithCancellation(cancellationToken);
            return value;
        }

        return maybe;
    }

    /// <summary>
    /// Returns the value if it exists, otherwise returns the fallback value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="fallbackOperation"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Maybe<T>> Or<T>(this ValueTask<Maybe<T>> maybeTask, Func<ValueTask<T>> fallbackOperation, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);

        if (maybe.HasNoValue)
        {
            T? value = await fallbackOperation().WithCancellation(cancellationToken);

            return value;
        }

        return maybe;
    }

    /// <summary>
    /// Returns the value if it exists, otherwise returns the fallback value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="fallbackOperation"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Maybe<T>> Or<T>(this ValueTask<Maybe<T>> maybeTask, Func<ValueTask<Maybe<T>>> fallbackOperation, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);

        if (maybe.HasNoValue)
            return await fallbackOperation().WithCancellation(cancellationToken);

        return maybe;
    }
}