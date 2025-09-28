using CSharpEssentials.Core;
using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result<TValue>
{
    /// <summary>
    /// If the operation failed, executes a function to generate a single error and returns a new Result with that error.
    /// </summary>
    /// <param name="onFailure"></param>
    /// <returns></returns>
    public Result<TValue> Else(Func<Error[], Error> onFailure)
    {
        if (IsSuccess)
            return Value;
        return onFailure(Errors).ToResult<TValue>();
    }

    /// <summary>
    /// If the operation failed, executes a function to generate multiple errors and returns a new Result with those errors.
    /// </summary>
    /// <param name="onFailure"></param>
    /// <returns></returns>
    public Result<TValue> Else(Func<Error[], Error[]> onFailure)
    {
        if (IsSuccess)
            return Value;
        return onFailure(Errors).ToResult<TValue>();
    }

    /// <summary>
    /// If the operation failed, replaces the current errors with the provided error.
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public Result<TValue> Else(Error error)
    {
        if (IsSuccess)
            return Value;
        return error.ToResult<TValue>();
    }

    /// <summary>
    /// Asynchronously executes a function to generate a single error if the operation failed and returns a new Result with that error.
    /// </summary>
    /// <param name="onFailure"></param>
    /// <returns></returns>
    public Result<TValue> Else(Func<Error[], TValue> onFailure)
    {
        if (IsSuccess)
            return Value;
        return onFailure(Errors);
    }

    /// <summary>
    /// Asynchronously executes a function to generate a single error if the operation failed and returns a new Result with that error.
    /// </summary>
    /// <param name="onFailure"></param>
    /// <returns></returns>
    public Result<TValue> Else(TValue onFailure)
    {
        if (IsSuccess)
            return Value;
        return onFailure;
    }

    /// <summary>
    /// If the operation failed, executes a function to generate a single error and returns a new Result with that error.
    /// </summary>
    /// <param name="onFailure"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result<TValue>> ElseAsync(Func<Error[], Task<TValue>> onFailure, CancellationToken cancellationToken = default)
    {
        if (IsSuccess)
            return Value;
        TValue? result = await onFailure(Errors).WithCancellation(cancellationToken);
        return result;
    }

    /// <summary>
    /// If the operation failed, executes a function to generate a single error and returns a new Result with that error.
    /// </summary>
    /// <param name="onFailure"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result<TValue>> ElseAsync(Func<Error[], Task<Error>> onFailure, CancellationToken cancellationToken = default)
    {
        if (IsSuccess)
            return Value;
        Error result = await onFailure(Errors).WithCancellation(cancellationToken);
        return result.ToResult<TValue>();
    }

    /// <summary>
    /// If the operation failed, executes a function to generate multiple errors and returns a new Result with those errors.
    /// </summary>
    /// <param name="onFailure"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result<TValue>> ElseAsync(Func<Error[], Task<Error[]>> onFailure, CancellationToken cancellationToken = default)
    {
        if (IsSuccess)
            return Value;
        Error[] result = await onFailure(Errors).WithCancellation(cancellationToken);
        return result.ToResult<TValue>();
    }

    /// <summary>
    /// If the operation failed, replaces the current errors with the provided error.
    /// </summary>
    /// <param name="error"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result<TValue>> ElseAsync(Task<Error> error, CancellationToken cancellationToken = default)
    {
        if (IsSuccess)
            return Value;
        Error result = await error.WithCancellation(cancellationToken);
        return result.ToResult<TValue>();
    }

    /// <summary>
    /// Asynchronously executes a function to generate a single error if the operation failed and returns a new Result with that error.
    /// </summary>
    /// <param name="onFailure"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result<TValue>> ElseAsync(Task<TValue> onFailure, CancellationToken cancellationToken = default)
    {
        if (IsSuccess)
            return Value;
        TValue? result = await onFailure.WithCancellation(cancellationToken);
        return result;
    }
}

public static partial class ResultExtensions
{
    /// <summary>
    /// If the operation failed, executes a function to generate a single error and returns a new Result with that error.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="onFailure"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<TValue>> Else<TValue>(this Task<Result<TValue>> task, Func<Error[], TValue> onFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.Else(onFailure);
    }

    /// <summary>
    /// If the operation failed, replaces the current errors with the provided error.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="onFailure"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<TValue>> Else<TValue>(this Task<Result<TValue>> task, TValue onFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.Else(onFailure);
    }

    /// <summary>
    /// If the operation failed, executes a function to generate multiple errors and returns a new Result with those errors.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="onFailure"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<TValue>> ElseAsync<TValue>(this Task<Result<TValue>> task, Func<Error[], Task<TValue>> onFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.ElseAsync(onFailure, cancellationToken);
    }

    /// <summary>
    /// If the operation failed, executes a function to generate multiple errors and returns a new Result with those errors.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="onFailure"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<TValue>> ElseAsync<TValue>(this Task<Result<TValue>> task, Task<TValue> onFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.ElseAsync(onFailure, cancellationToken);
    }

    /// <summary>
    ///  If the operation failed, executes a function to generate a single error and returns a new Result with that error.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="onFailure"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<TValue>> Else<TValue>(this Task<Result<TValue>> task, Func<Error[], Error> onFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.Else(onFailure);
    }

    /// <summary>
    /// If the operation failed, executes a function to generate multiple errors and returns a new Result with those errors.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="onFailure"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<TValue>> Else<TValue>(this Task<Result<TValue>> task, Func<Error[], Error[]> onFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.Else(onFailure);
    }

    /// <summary>
    /// If the operation failed, replaces the current errors with the provided error.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="error"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<TValue>> Else<TValue>(this Task<Result<TValue>> task, Error error, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.Else(error);
    }
    /// <summary>
    /// If the operation failed, executes a function to generate a single error and returns a new Result with that error.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="onFailure"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<TValue>> ElseAsync<TValue>(this Task<Result<TValue>> task, Func<Error[], Task<Error>> onFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.ElseAsync(onFailure, cancellationToken);
    }

    /// <summary>
    /// If the operation failed, executes a function to generate multiple errors and returns a new Result with those errors.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="onFailure"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<TValue>> ElseAsync<TValue>(this Task<Result<TValue>> task, Func<Error[], Task<Error[]>> onFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.ElseAsync(onFailure, cancellationToken);
    }

    /// <summary>
    /// If the operation failed, replaces the current errors with the provided error.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="onFailure"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<TValue>> ElseAsync<TValue>(this Task<Result<TValue>> task, Task<Error> onFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.ElseAsync(onFailure, cancellationToken);
    }
}