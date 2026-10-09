using System.Runtime.CompilerServices;
using CSharpEssentials.Core;

namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result<TValue>
{
    /// <summary>
    /// Executes the given function if the result is successful.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="onSuccess"></param>
    /// <returns></returns>
    public Result<T> Then<T>(Func<TValue, Result<T>> onSuccess)
    {
        if (IsFailure)
            return _errors;
        return onSuccess(Value);
    }

    /// <summary>
    /// Executes the given action if the result is successful.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public Result<TValue> ThenDo(Action<TValue> action)
    {
        if (IsFailure)
            return this;
        action(Value);
        return this;
    }

    /// <summary>
    /// Executes the given function if the result is successful.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="onSuccess"></param>
    /// <returns></returns>
    public Result<T> Then<T>(Func<TValue, T> onSuccess)
    {
        if (IsFailure)
            return _errors;
        return onSuccess(Value);
    }

    /// <summary>
    /// Executes the given function if the result is successful.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="onSuccess"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(1)]
    public async Task<Result<T>> ThenAsync<T>(Func<TValue, Task<Result<T>>> onSuccess, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return _errors;
        return await onSuccess(Value).WithCancellation(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the given action if the result is successful.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(1)]
    public async Task<Result<TValue>> ThenDoAsync(Func<TValue, Task> action, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return this;
        await action(Value).WithCancellation(cancellationToken).ConfigureAwait(false);
        return this;
    }

    /// <summary>
    /// Executes the given function if the result is successful.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="onSuccess"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(1)]
    public async Task<Result<T>> ThenAsync<T>(Func<TValue, Task<T>> onSuccess, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return _errors;
        T? result = await onSuccess(Value).WithCancellation(cancellationToken).ConfigureAwait(false);
        return result;
    }

#if NET9_0_OR_GREATER
    /// <summary>
    /// Executes the given ValueTask function if the result is successful.
    /// </summary>
    public async ValueTask<Result<T>> ThenAsync<T>(Func<TValue, ValueTask<Result<T>>> onSuccess, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return _errors;
        return await onSuccess(Value).WithCancellation(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the given ValueTask action if the result is successful.
    /// </summary>
    public async ValueTask<Result<TValue>> ThenDoAsync(Func<TValue, ValueTask> action, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return this;
        await action(Value).WithCancellation(cancellationToken).ConfigureAwait(false);
        return this;
    }

    /// <summary>
    /// Executes the given ValueTask function if the result is successful.
    /// </summary>
    public async ValueTask<Result<T>> ThenAsync<T>(Func<TValue, ValueTask<T>> onSuccess, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return _errors;
        T result = await onSuccess(Value).WithCancellation(cancellationToken).ConfigureAwait(false);
        return result;
    }
#endif
}

public static partial class ResultExtensions
{
    /// <summary>
    /// Executes the given function if the result is successful.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <param name="task"></param>
    /// <param name="onSuccess"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<T>> ThenAsync<TValue, T>(this Task<Result<TValue>> task, Func<TValue, Result<T>> onSuccess, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Then(onSuccess);
    }

    /// <summary>
    /// Executes the given action if the result is successful.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <param name="task"></param>
    /// <param name="onSuccess"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<T>> ThenAsync<TValue, T>(this Task<Result<TValue>> task, Func<TValue, T> onSuccess, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Then(onSuccess);
    }

    /// <summary>
    /// Executes the given function if the result is successful.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<TValue>> ThenDoAsync<TValue>(this Task<Result<TValue>> task, Action<TValue> action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.ThenDo(action);
    }

    /// <summary>
    /// Executes the given function if the result is successful.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <param name="task"></param>
    /// <param name="onSuccess"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<T>> ThenAsync<TValue, T>(this Task<Result<TValue>> task, Func<TValue, Task<Result<T>>> onSuccess, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.ThenAsync(onSuccess, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the given action if the result is successful.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <param name="task"></param>
    /// <param name="onSuccess"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<T>> ThenAsync<TValue, T>(this Task<Result<TValue>> task, Func<TValue, Task<T>> onSuccess, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.ThenAsync(onSuccess, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the given action if the result is successful.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<TValue>> ThenDoAsync<TValue>(this Task<Result<TValue>> task, Func<TValue, Task> action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.ThenDoAsync(action, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the given function if the result is successful.
    /// </summary>
    public static async ValueTask<Result<T>> ThenAsync<TValue, T>(this ValueTask<Result<TValue>> task, Func<TValue, Result<T>> onSuccess, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Then(onSuccess);
    }

    /// <summary>
    /// Executes the given function if the result is successful.
    /// </summary>
    public static async ValueTask<Result<T>> ThenAsync<TValue, T>(this ValueTask<Result<TValue>> task, Func<TValue, T> onSuccess, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Then(onSuccess);
    }

    /// <summary>
    /// Executes the given action if the result is successful.
    /// </summary>
    public static async ValueTask<Result<TValue>> ThenDoAsync<TValue>(this ValueTask<Result<TValue>> task, Action<TValue> action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.ThenDo(action);
    }

    /// <summary>
    /// Executes the given async function if the result is successful.
    /// </summary>
    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result<T>> ThenAsync<TValue, T>(this ValueTask<Result<TValue>> task, Func<TValue, Task<Result<T>>> onSuccess, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.ThenAsync(onSuccess, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the given async function if the result is successful.
    /// </summary>
    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result<T>> ThenAsync<TValue, T>(this ValueTask<Result<TValue>> task, Func<TValue, Task<T>> onSuccess, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.ThenAsync(onSuccess, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the given async action if the result is successful.
    /// </summary>
    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result<TValue>> ThenDoAsync<TValue>(this ValueTask<Result<TValue>> task, Func<TValue, Task> action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.ThenDoAsync(action, cancellationToken).ConfigureAwait(false);
    }

#if NET9_0_OR_GREATER
    /// <summary>
    /// Awaits the ValueTask result, then executes the ValueTask function if it is a success.
    /// </summary>
    public static async ValueTask<Result<T>> ThenAsync<TValue, T>(this ValueTask<Result<TValue>> task, Func<TValue, ValueTask<Result<T>>> onSuccess, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.ThenAsync(onSuccess, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Awaits the ValueTask result, then executes the ValueTask function if it is a success.
    /// </summary>
    public static async ValueTask<Result<T>> ThenAsync<TValue, T>(this ValueTask<Result<TValue>> task, Func<TValue, ValueTask<T>> onSuccess, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.ThenAsync(onSuccess, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Awaits the ValueTask result, then executes the ValueTask action if it is a success.
    /// </summary>
    public static async ValueTask<Result<TValue>> ThenDoAsync<TValue>(this ValueTask<Result<TValue>> task, Func<TValue, ValueTask> action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.ThenDoAsync(action, cancellationToken).ConfigureAwait(false);
    }
#endif
}
