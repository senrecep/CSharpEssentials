

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
    public async Task<Result<TValue>> FailIfAsync(Func<TValue, Task<bool>> onSuccess, Error error, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return this;
        return await onSuccess(Value).WithCancellation(cancellationToken) ? error : this;
    }

    /// <summary>
    /// Fail if the value is true
    /// </summary>
    /// <param name="onSuccess"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result<TValue>> FailIfAsync(Func<TValue, Task<bool>> onSuccess, Func<TValue, Task<Error>> func, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return this;
        return await onSuccess(Value).WithCancellation(cancellationToken) ? (await func(Value).WithCancellation(cancellationToken)) : this;
    }
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
    public static async Task<Result<TValue>> FailIf<TValue>(
        this Task<Result<TValue>> task,
        Func<TValue, bool> onSuccess,
        Error error,
        CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
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
    public static async Task<Result<TValue>> FailIf<TValue>(
        this Task<Result<TValue>> task,
        Func<TValue, bool> onSuccess,
        Func<TValue, Error> func,
        CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
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
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.FailIfAsync(onSuccess, error, cancellationToken);
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
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.FailIfAsync(onSuccess, func, cancellationToken);
    }
}