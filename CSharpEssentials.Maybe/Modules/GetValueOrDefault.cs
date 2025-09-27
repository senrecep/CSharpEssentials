using CSharpEssentials.Core;

namespace CSharpEssentials.Maybe;

public readonly partial struct Maybe<T>
{
    /// <summary>
    /// Returns the value if the Maybe has a value, otherwise returns the default value.
    /// </summary>
    /// <param name="defaultValue"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<T> GetValueOrDefaultAsync(Func<Task<T>> defaultValue, CancellationToken cancellationToken = default)
    {
        if (HasNoValue)
            return await defaultValue().WithCancellation(cancellationToken);

        return Value;
    }

    /// <summary>
    /// Returns the value if the Maybe has a value, otherwise returns the default value.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="selector"></param>
    /// <param name="defaultValue"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<TOut> GetValueOrDefaultAsync<TOut>(Func<T, TOut> selector,
        Func<Task<TOut>> defaultValue, CancellationToken cancellationToken = default)
    {
        if (HasNoValue)
            return await defaultValue().WithCancellation(cancellationToken);

        return selector(Value);
    }

    /// <summary>
    /// Returns the value if the Maybe has a value, otherwise returns the default value.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="selector"></param>
    /// <param name="defaultValue"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<TOut> GetValueOrDefaultAsync<TOut>(Func<T, Task<TOut>> selector,
        TOut defaultValue, CancellationToken cancellationToken = default)
    {
        if (HasNoValue)
            return defaultValue;

        return await selector(Value).WithCancellation(cancellationToken);
    }
}

public static partial class MaybeExtensions
{
    /// <summary>
    /// Returns the value if the Maybe has a value, otherwise returns the default value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="defaultValue"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<T> GetValueOrDefaultAsync<T>(this Task<Maybe<T>> maybeTask, Func<T> defaultValue, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.GetValueOrDefault(defaultValue);
    }
    /// <summary>
    /// Returns the value if the Maybe has a value, otherwise returns the default value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="selector"></param>
    /// <param name="defaultValue"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<TOut> GetValueOrDefaultAsync<T, TOut>(this Task<Maybe<T>> maybeTask, Func<T, TOut> selector, TOut defaultValue, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.GetValueOrDefault(selector, defaultValue);
    }
    /// <summary>
    /// Returns the value if the Maybe has a value, otherwise returns the default value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="selector"></param>
    /// <param name="defaultValue"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<TOut> GetValueOrDefaultAsync<T, TOut>(this Task<Maybe<T>> maybeTask, Func<T, TOut> selector, Func<TOut> defaultValue, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.GetValueOrDefault(selector, defaultValue);
    }

    /// <summary>
    /// Returns the value if the Maybe has a value, otherwise returns the default value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="maybe"></param>
    /// <param name="selector"></param>
    /// <param name="defaultValue"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>

    public static async Task<TOut> GetValueOrDefaultAsync<T, TOut>(this Maybe<T> maybe, Func<T, Task<TOut>> selector,
        Func<Task<TOut>> defaultValue, CancellationToken cancellationToken = default)
    {
        if (maybe.HasNoValue)
            return await defaultValue().WithCancellation(cancellationToken);

        return await selector(maybe.Value).WithCancellation(cancellationToken);
    }

    public static async Task<T> GetValueOrDefaultAsync<T>(this Task<Maybe<T>> maybeTask, Func<Task<T>> defaultValue, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return await maybe.GetValueOrDefaultAsync(defaultValue, cancellationToken);
    }

    public static async Task<TOut> GetValueOrDefaultAsync<T, TOut>(this Task<Maybe<T>> maybeTask, Func<T, Task<TOut>> selector,
        TOut defaultValue, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return await maybe.GetValueOrDefaultAsync(selector, defaultValue, cancellationToken);
    }

    public static async Task<TOut> GetValueOrDefaultAsync<T, TOut>(this Task<Maybe<T>> maybeTask, Func<T, Task<TOut>> selector,
        Func<Task<TOut>> defaultValue, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return await maybe.GetValueOrDefaultAsync(selector, defaultValue, cancellationToken);
    }

    public static async ValueTask<T> GetValueOrDefaultAsync<T>(this ValueTask<Maybe<T>> maybeTask, Func<T> defaultValue, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.GetValueOrDefault(defaultValue);
    }

    public static async ValueTask<TOut> GetValueOrDefaultAsync<T, TOut>(this ValueTask<Maybe<T>> maybeTask, Func<T, TOut> selector,
        TOut defaultValue, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.GetValueOrDefault(selector, defaultValue);
    }

    public static async ValueTask<TOut> GetValueOrDefaultAsync<T, TOut>(this ValueTask<Maybe<T>> maybeTask, Func<T, TOut> selector,
        Func<TOut> defaultValue, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.GetValueOrDefault(selector, defaultValue);
    }

    public static async ValueTask<T> GetValueOrDefaultAsync<T>(this Maybe<T> maybe, Func<ValueTask<T>> valueTask, CancellationToken cancellationToken = default)
    {
        if (maybe.HasNoValue)
            return await valueTask().WithCancellation(cancellationToken);

        return maybe.Value;
    }

    public static async ValueTask<TOut> GetValueOrDefaultAsync<T, TOut>(this Maybe<T> maybe, Func<T, TOut> selector,
        Func<ValueTask<TOut>> valueTask, CancellationToken cancellationToken = default)
    {
        if (maybe.HasNoValue)
            return await valueTask().WithCancellation(cancellationToken);

        return selector(maybe.Value);
    }

    public static async ValueTask<TOut> GetValueOrDefaultAsync<T, TOut>(this Maybe<T> maybe, Func<T, ValueTask<TOut>> valueTask,
        TOut defaultValue, CancellationToken cancellationToken = default)
    {
        if (maybe.HasNoValue)
            return defaultValue;

        return await valueTask(maybe.Value).WithCancellation(cancellationToken);
    }

    public static async ValueTask<TOut> GetValueOrDefaultAsync<T, TOut>(this Maybe<T> maybe, Func<T, ValueTask<TOut>> valueTask,
        Func<ValueTask<TOut>> defaultValue, CancellationToken cancellationToken = default)
    {
        if (maybe.HasNoValue)
            return await defaultValue().WithCancellation(cancellationToken);

        return await valueTask(maybe.Value).WithCancellation(cancellationToken);
    }

    public static async ValueTask<T> GetValueOrDefaultAsync<T>(this ValueTask<Maybe<T>> maybeTask, Func<ValueTask<T>> defaultValue, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask;
        return await maybe.GetValueOrDefaultAsync(defaultValue, cancellationToken);
    }

    public static async ValueTask<TOut> GetValueOrDefaultAsync<T, TOut>(this ValueTask<Maybe<T>> maybeTask, Func<T, ValueTask<TOut>> selector,
        TOut defaultValue, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return await maybe.GetValueOrDefaultAsync(selector, defaultValue, cancellationToken);
    }

    public static async ValueTask<TOut> GetValueOrDefaultAsync<T, TOut>(this ValueTask<Maybe<T>> maybeTask, Func<T, ValueTask<TOut>> selector,
        Func<ValueTask<TOut>> defaultValue, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return await maybe.GetValueOrDefaultAsync(selector, defaultValue, cancellationToken);
    }

    public static T GetValueOrDefault<T>(in this Maybe<T> maybe, Func<T> defaultValue)
    {
        if (maybe.HasNoValue)
            return defaultValue();

        return maybe.Value;
    }

    public static TOut GetValueOrDefault<T, TOut>(in this Maybe<T> maybe, Func<T, TOut> selector, TOut defaultValue)
    {
        if (maybe.HasNoValue)
            return defaultValue;

        return selector(maybe.Value);
    }

    public static TOut GetValueOrDefault<T, TOut>(in this Maybe<T> maybe, Func<T, TOut> selector, Func<TOut> defaultValue)
    {
        if (maybe.HasNoValue)
            return defaultValue();

        return selector(maybe.Value);
    }

}