using CSharpEssentials.Core;
namespace CSharpEssentials.Maybe;

public readonly partial struct Maybe<T>
{
    /// <summary>
    /// Executes the specified action if the Maybe has a value. Alias for Execute.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public Maybe<T> Tap(Action<T> action)
    {
        Execute(action);
        return this;
    }

    /// <summary>
    /// Executes the specified action if the Maybe has a value. Alias for Execute.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public Maybe<T> Tap(Action action)
    {
        if (HasValue)
            action();
        return this;
    }

    /// <summary>
    /// Executes the specified action if the Maybe has a value and the condition is true.
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public Maybe<T> TapIf(bool condition, Action<T> action)
    {
        if (HasValue && condition)
            action(Value);
        return this;
    }

    /// <summary>
    /// Executes the specified action if the Maybe has a value and the predicate returns true.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public Maybe<T> TapIf(Func<bool> predicate, Action<T> action)
    {
        if (HasValue && predicate())
            action(Value);
        return this;
    }

    /// <summary>
    /// Executes the specified action if the Maybe has a value and the predicate returns true.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public Maybe<T> TapIf(Func<T, bool> predicate, Action<T> action)
    {
        if (HasValue && predicate(Value))
            action(Value);
        return this;
    }

    /// <summary>
    /// Asynchronously executes the specified action if the Maybe has a value.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Maybe<T>> TapAsync(Func<T, Task> action, CancellationToken cancellationToken = default)
    {
        await Execute(action, cancellationToken);
        return this;
    }

    /// <summary>
    /// Asynchronously executes the specified action if the Maybe has a value.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Maybe<T>> TapAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        if (HasValue)
            await action().WithCancellation(cancellationToken);
        return this;
    }
}

public static partial class MaybeExtensions
{
    /// <summary>
    /// Executes the specified action if the Maybe has a value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Maybe<T>> TapAsync<T>(this Task<Maybe<T>> maybeTask, Action<T> action, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.Tap(action);
    }

    /// <summary>
    /// Executes the specified action if the Maybe has a value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Maybe<T>> TapAsync<T>(this Task<Maybe<T>> maybeTask, Action action, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.Tap(action);
    }

    /// <summary>
    /// Executes the specified action if the Maybe has a value and the condition is true.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="condition"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Maybe<T>> TapIfAsync<T>(this Task<Maybe<T>> maybeTask, bool condition, Action<T> action, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.TapIf(condition, action);
    }

    /// <summary>
    /// Executes the specified action if the Maybe has a value and the predicate returns true.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="predicate"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Maybe<T>> TapIfAsync<T>(this Task<Maybe<T>> maybeTask, Func<bool> predicate, Action<T> action, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.TapIf(predicate, action);
    }

    /// <summary>
    /// Executes the specified action if the Maybe has a value and the predicate returns true.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="predicate"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Maybe<T>> TapIfAsync<T>(this Task<Maybe<T>> maybeTask, Func<T, bool> predicate, Action<T> action, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.TapIf(predicate, action);
    }

    /// <summary>
    /// Executes the specified async action if the Maybe has a value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Maybe<T>> TapAsync<T>(this Task<Maybe<T>> maybeTask, Func<T, Task> action, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return await maybe.TapAsync(action, cancellationToken);
    }

    /// <summary>
    /// Executes the specified async action if the Maybe has a value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Maybe<T>> TapAsync<T>(this Task<Maybe<T>> maybeTask, Func<Task> action, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return await maybe.TapAsync(action, cancellationToken);
    }

    /// <summary>
    /// Executes the specified action if the Maybe has a value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Maybe<T>> TapAsync<T>(this ValueTask<Maybe<T>> maybeTask, Action<T> action, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.Tap(action);
    }

    /// <summary>
    /// Executes the specified action if the Maybe has a value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Maybe<T>> TapAsync<T>(this ValueTask<Maybe<T>> maybeTask, Action action, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.Tap(action);
    }

    /// <summary>
    /// Executes the specified action if the Maybe has a value and the condition is true.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="condition"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Maybe<T>> TapIfAsync<T>(this ValueTask<Maybe<T>> maybeTask, bool condition, Action<T> action, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.TapIf(condition, action);
    }

    /// <summary>
    /// Executes the specified action if the Maybe has a value and the predicate returns true.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="predicate"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Maybe<T>> TapIfAsync<T>(this ValueTask<Maybe<T>> maybeTask, Func<bool> predicate, Action<T> action, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.TapIf(predicate, action);
    }

    /// <summary>
    /// Executes the specified action if the Maybe has a value and the predicate returns true.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="predicate"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Maybe<T>> TapIfAsync<T>(this ValueTask<Maybe<T>> maybeTask, Func<T, bool> predicate, Action<T> action, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.TapIf(predicate, action);
    }

    /// <summary>
    /// Executes the specified async action if the Maybe has a value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Maybe<T>> TapAsync<T>(this ValueTask<Maybe<T>> maybeTask, Func<T, Task> action, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return await maybe.TapAsync(action, cancellationToken);
    }

    /// <summary>
    /// Executes the specified async action if the Maybe has a value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Maybe<T>> TapAsync<T>(this ValueTask<Maybe<T>> maybeTask, Func<Task> action, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return await maybe.TapAsync(action, cancellationToken);
    }
}
