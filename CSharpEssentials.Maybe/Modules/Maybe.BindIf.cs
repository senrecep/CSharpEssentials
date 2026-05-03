using CSharpEssentials.Core;
namespace CSharpEssentials.Maybe;

public readonly partial struct Maybe<T>
{
    /// <summary>
    /// Binds the specified function if the condition is true.
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public Maybe<T> BindIf(bool condition, Func<T, Maybe<T>> func) =>
        condition ? Bind(func) : this;

    /// <summary>
    /// Binds the specified function if the predicate returns true.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public Maybe<T> BindIf(Func<bool> predicate, Func<T, Maybe<T>> func) =>
        predicate() ? Bind(func) : this;

    /// <summary>
    /// Binds the specified function if the predicate returns true.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public Maybe<T> BindIf(Func<T, bool> predicate, Func<T, Maybe<T>> func) =>
        HasValue && predicate(Value) ? Bind(func) : this;

    /// <summary>
    /// Asynchronously binds the specified function if the condition is true.
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Maybe<T>> BindIfAsync(bool condition, Func<T, Task<Maybe<T>>> func, CancellationToken cancellationToken = default) =>
        condition ? await BindAsync(func, cancellationToken) : this;

    /// <summary>
    /// Asynchronously binds the specified function if the predicate returns true.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Maybe<T>> BindIfAsync(Func<bool> predicate, Func<T, Task<Maybe<T>>> func, CancellationToken cancellationToken = default) =>
        predicate() ? await BindAsync(func, cancellationToken) : this;

    /// <summary>
    /// Asynchronously binds the specified function if the predicate returns true.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Maybe<T>> BindIfAsync(Func<T, bool> predicate, Func<T, Task<Maybe<T>>> func, CancellationToken cancellationToken = default) =>
        HasValue && predicate(Value) ? await BindAsync(func, cancellationToken) : this;
}

public static partial class MaybeExtensions
{
    /// <summary>
    /// Binds the specified function if the condition is true.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="condition"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Maybe<T>> BindIfAsync<T>(this Task<Maybe<T>> maybeTask, bool condition, Func<T, Maybe<T>> func, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.BindIf(condition, func);
    }

    /// <summary>
    /// Binds the specified function if the predicate returns true.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="predicate"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Maybe<T>> BindIfAsync<T>(this Task<Maybe<T>> maybeTask, Func<bool> predicate, Func<T, Maybe<T>> func, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.BindIf(predicate, func);
    }

    /// <summary>
    /// Binds the specified function if the predicate returns true.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="predicate"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Maybe<T>> BindIfAsync<T>(this Task<Maybe<T>> maybeTask, Func<T, bool> predicate, Func<T, Maybe<T>> func, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.BindIf(predicate, func);
    }

    /// <summary>
    /// Asynchronously binds the specified function if the condition is true.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="condition"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Maybe<T>> BindIfAsync<T>(this Task<Maybe<T>> maybeTask, bool condition, Func<T, Task<Maybe<T>>> func, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return await maybe.BindIfAsync(condition, func, cancellationToken);
    }

    /// <summary>
    /// Asynchronously binds the specified function if the predicate returns true.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="predicate"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Maybe<T>> BindIfAsync<T>(this Task<Maybe<T>> maybeTask, Func<bool> predicate, Func<T, Task<Maybe<T>>> func, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return await maybe.BindIfAsync(predicate, func, cancellationToken);
    }

    /// <summary>
    /// Asynchronously binds the specified function if the predicate returns true.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="predicate"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Maybe<T>> BindIfAsync<T>(this Task<Maybe<T>> maybeTask, Func<T, bool> predicate, Func<T, Task<Maybe<T>>> func, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return await maybe.BindIfAsync(predicate, func, cancellationToken);
    }

    /// <summary>
    /// Binds the specified function if the condition is true.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="condition"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Maybe<T>> BindIfAsync<T>(this ValueTask<Maybe<T>> maybeTask, bool condition, Func<T, Maybe<T>> func, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.BindIf(condition, func);
    }

    /// <summary>
    /// Binds the specified function if the predicate returns true.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="predicate"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Maybe<T>> BindIfAsync<T>(this ValueTask<Maybe<T>> maybeTask, Func<bool> predicate, Func<T, Maybe<T>> func, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.BindIf(predicate, func);
    }

    /// <summary>
    /// Binds the specified function if the predicate returns true.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="predicate"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Maybe<T>> BindIfAsync<T>(this ValueTask<Maybe<T>> maybeTask, Func<T, bool> predicate, Func<T, Maybe<T>> func, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.BindIf(predicate, func);
    }

    /// <summary>
    /// Asynchronously binds the specified function if the condition is true.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="condition"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Maybe<T>> BindIfAsync<T>(this ValueTask<Maybe<T>> maybeTask, bool condition, Func<T, Task<Maybe<T>>> func, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return await maybe.BindIfAsync(condition, func, cancellationToken);
    }

    /// <summary>
    /// Asynchronously binds the specified function if the predicate returns true.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="predicate"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Maybe<T>> BindIfAsync<T>(this ValueTask<Maybe<T>> maybeTask, Func<bool> predicate, Func<T, Task<Maybe<T>>> func, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return await maybe.BindIfAsync(predicate, func, cancellationToken);
    }

    /// <summary>
    /// Asynchronously binds the specified function if the predicate returns true.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="predicate"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Maybe<T>> BindIfAsync<T>(this ValueTask<Maybe<T>> maybeTask, Func<T, bool> predicate, Func<T, Task<Maybe<T>>> func, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return await maybe.BindIfAsync(predicate, func, cancellationToken);
    }
}
