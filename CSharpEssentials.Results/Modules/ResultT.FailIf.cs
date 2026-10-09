using System.Runtime.CompilerServices;


using CSharpEssentials.Core;
using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result<TValue>
{
    /// <summary>
    /// Fail if the value is true
    /// </summary>
    /// <param name="onSuccess"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public Result<TValue> FailIf(Func<TValue, bool> onSuccess, Error error)
    {
        if (IsFailure)
            return this;
        return onSuccess(Value) ? error : this;
    }

    /// <summary>
    /// Fail if the value is true
    /// </summary>
    /// <param name="onSuccess"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public Result<TValue> FailIf(Func<TValue, bool> onSuccess, Func<TValue, Error> func)
    {
        if (IsFailure)
            return this;
        return onSuccess(Value) ? func(Value) : this;
    }

    /// <summary>
    ///  Fail if the value is true
    /// </summary>
    /// <param name="onSuccess"></param>
    /// <param name="error"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(1)]
    public async Task<Result<TValue>> FailIfAsync(Func<TValue, Task<bool>> onSuccess, Error error, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return this;
        return await onSuccess(Value).WithCancellation(cancellationToken).ConfigureAwait(false) ? error : this;
    }

    /// <summary>
    /// Fail if the value is true
    /// </summary>
    /// <param name="onSuccess"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(1)]
    public async Task<Result<TValue>> FailIfAsync(Func<TValue, Task<bool>> onSuccess, Func<TValue, Task<Error>> func, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return this;
        return await onSuccess(Value).WithCancellation(cancellationToken).ConfigureAwait(false) ? (await func(Value).WithCancellation(cancellationToken).ConfigureAwait(false)) : this;
    }

#if NET9_0_OR_GREATER
    /// <summary>
    /// Fail if the ValueTask predicate returns true.
    /// </summary>
    /// <param name="onSuccess"></param>
    /// <param name="error"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result<TValue>> FailIfAsync(Func<TValue, ValueTask<bool>> onSuccess, Error error, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return this;
        return await onSuccess(Value).WithCancellation(cancellationToken).ConfigureAwait(false) ? error : this;
    }

    /// <summary>
    /// Fail if the ValueTask predicate returns true.
    /// </summary>
    /// <param name="onSuccess"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result<TValue>> FailIfAsync(Func<TValue, ValueTask<bool>> onSuccess, Func<TValue, ValueTask<Error>> func, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return this;
        return await onSuccess(Value).WithCancellation(cancellationToken).ConfigureAwait(false)
            ? (await func(Value).WithCancellation(cancellationToken).ConfigureAwait(false))
            : this;
    }
#endif
}

public static partial class ResultExtensions
{
    /// <summary>
    /// Fail if the value is true
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="onSuccess"></param>
    /// <param name="error"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<TValue>> FailIfAsync<TValue>(
        this Task<Result<TValue>> task,
        Func<TValue, bool> onSuccess,
        Error error,
        CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.FailIf(onSuccess, error);
    }

    /// <summary>
    /// Fail if the value is true
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="onSuccess"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<TValue>> FailIfAsync<TValue>(
        this Task<Result<TValue>> task,
        Func<TValue, bool> onSuccess,
        Func<TValue, Error> func,
        CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.FailIf(onSuccess, func);
    }

    /// <summary>
    ///  Fail if the value is true
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="onSuccess"></param>
    /// <param name="error"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<TValue>> FailIfAsync<TValue>(
        this Task<Result<TValue>> task,
        Func<TValue, Task<bool>> onSuccess,
        Error error,
        CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.FailIfAsync(onSuccess, error, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Fail if the value is true
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="onSuccess"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<TValue>> FailIfAsync<TValue>(
        this Task<Result<TValue>> task,
        Func<TValue, Task<bool>> onSuccess,
        Func<TValue, Task<Error>> func,
        CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.FailIfAsync(onSuccess, func, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Fail if the value is true.
    /// </summary>
    public static async ValueTask<Result<TValue>> FailIfAsync<TValue>(
        this ValueTask<Result<TValue>> task,
        Func<TValue, bool> onSuccess,
        Error error,
        CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.FailIf(onSuccess, error);
    }

    /// <summary>
    /// Fail if the value is true.
    /// </summary>
    public static async ValueTask<Result<TValue>> FailIfAsync<TValue>(
        this ValueTask<Result<TValue>> task,
        Func<TValue, bool> onSuccess,
        Func<TValue, Error> func,
        CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.FailIf(onSuccess, func);
    }

    /// <summary>
    /// Fail if the value is true.
    /// </summary>
    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result<TValue>> FailIfAsync<TValue>(
        this ValueTask<Result<TValue>> task,
        Func<TValue, Task<bool>> onSuccess,
        Error error,
        CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.FailIfAsync(onSuccess, error, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Fail if the value is true.
    /// </summary>
    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result<TValue>> FailIfAsync<TValue>(
        this ValueTask<Result<TValue>> task,
        Func<TValue, Task<bool>> onSuccess,
        Func<TValue, Task<Error>> func,
        CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.FailIfAsync(onSuccess, func, cancellationToken).ConfigureAwait(false);
    }

#if NET9_0_OR_GREATER
    /// <summary>
    /// Fail if the ValueTask predicate returns true.
    /// </summary>
    public static async ValueTask<Result<TValue>> FailIfAsync<TValue>(
        this ValueTask<Result<TValue>> task,
        Func<TValue, ValueTask<bool>> onSuccess,
        Error error,
        CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.FailIfAsync(onSuccess, error, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Fail if the ValueTask predicate returns true.
    /// </summary>
    public static async ValueTask<Result<TValue>> FailIfAsync<TValue>(
        this ValueTask<Result<TValue>> task,
        Func<TValue, ValueTask<bool>> onSuccess,
        Func<TValue, ValueTask<Error>> func,
        CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.FailIfAsync(onSuccess, func, cancellationToken).ConfigureAwait(false);
    }
#endif
}
