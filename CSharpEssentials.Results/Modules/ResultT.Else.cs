using System.Runtime.CompilerServices;
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
        return onFailure(Errors);
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
        return onFailure(Errors);
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
        return error;
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
    [OverloadResolutionPriority(1)]
    public async Task<Result<TValue>> ElseAsync(Func<Error[], Task<TValue>> onFailure, CancellationToken cancellationToken = default)
    {
        if (IsSuccess)
            return Value;
        TValue? result = await onFailure(Errors).WithCancellation(cancellationToken).ConfigureAwait(false);
        return result;
    }

    /// <summary>
    /// If the operation failed, executes a function to generate a single error and returns a new Result with that error.
    /// </summary>
    /// <param name="onFailure"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(1)]
    public async Task<Result<TValue>> ElseAsync(Func<Error[], Task<Error>> onFailure, CancellationToken cancellationToken = default)
    {
        if (IsSuccess)
            return Value;
        Error result = await onFailure(Errors).WithCancellation(cancellationToken).ConfigureAwait(false);
        return result;
    }

    /// <summary>
    /// If the operation failed, executes a function to generate multiple errors and returns a new Result with those errors.
    /// </summary>
    /// <param name="onFailure"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(1)]
    public async Task<Result<TValue>> ElseAsync(Func<Error[], Task<Error[]>> onFailure, CancellationToken cancellationToken = default)
    {
        if (IsSuccess)
            return Value;
        Error[] result = await onFailure(Errors).WithCancellation(cancellationToken).ConfigureAwait(false);
        return result;
    }

    /// <summary>
    /// If the operation failed, replaces the current errors with the provided error.
    /// </summary>
    /// <param name="error"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(1)]
    public async Task<Result<TValue>> ElseAsync(Task<Error> error, CancellationToken cancellationToken = default)
    {
        if (IsSuccess)
            return Value;
        Error result = await error.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result;
    }

    /// <summary>
    /// Asynchronously executes a function to generate a single error if the operation failed and returns a new Result with that error.
    /// </summary>
    /// <param name="onFailure"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(1)]
    public async Task<Result<TValue>> ElseAsync(Task<TValue> onFailure, CancellationToken cancellationToken = default)
    {
        if (IsSuccess)
            return Value;
        TValue? result = await onFailure.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result;
    }

#if NET9_0_OR_GREATER
    /// <summary>
    /// If the operation failed, executes a ValueTask function to generate a value and returns a new Result with that value.
    /// </summary>
    /// <param name="onFailure"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result<TValue>> ElseAsync(Func<Error[], ValueTask<TValue>> onFailure, CancellationToken cancellationToken = default)
    {
        if (IsSuccess)
            return Value;
        TValue? result = await onFailure(Errors).WithCancellation(cancellationToken).ConfigureAwait(false);
        return result;
    }

    /// <summary>
    /// If the operation failed, executes a ValueTask function to generate a single error and returns a new Result with that error.
    /// </summary>
    /// <param name="onFailure"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result<TValue>> ElseAsync(Func<Error[], ValueTask<Error>> onFailure, CancellationToken cancellationToken = default)
    {
        if (IsSuccess)
            return Value;
        Error result = await onFailure(Errors).WithCancellation(cancellationToken).ConfigureAwait(false);
        return result;
    }

    /// <summary>
    /// If the operation failed, executes a ValueTask function to generate multiple errors and returns a new Result with those errors.
    /// </summary>
    /// <param name="onFailure"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result<TValue>> ElseAsync(Func<Error[], ValueTask<Error[]>> onFailure, CancellationToken cancellationToken = default)
    {
        if (IsSuccess)
            return Value;
        Error[] result = await onFailure(Errors).WithCancellation(cancellationToken).ConfigureAwait(false);
        return result;
    }

    /// <summary>
    /// If the operation failed, replaces the current errors with the provided ValueTask error.
    /// </summary>
    /// <param name="error"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result<TValue>> ElseAsync(ValueTask<Error> error, CancellationToken cancellationToken = default)
    {
        if (IsSuccess)
            return Value;
        Error result = await error.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result;
    }

    /// <summary>
    /// If the operation failed, replaces the current errors with the provided ValueTask value.
    /// </summary>
    /// <param name="onFailure"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result<TValue>> ElseAsync(ValueTask<TValue> onFailure, CancellationToken cancellationToken = default)
    {
        if (IsSuccess)
            return Value;
        TValue? result = await onFailure.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result;
    }
#endif
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
    public static async Task<Result<TValue>> ElseAsync<TValue>(this Task<Result<TValue>> task, Func<Error[], TValue> onFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
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
    public static async Task<Result<TValue>> ElseAsync<TValue>(this Task<Result<TValue>> task, TValue onFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
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
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.ElseAsync(onFailure, cancellationToken).ConfigureAwait(false);
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
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.ElseAsync(onFailure, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///  If the operation failed, executes a function to generate a single error and returns a new Result with that error.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="onFailure"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<TValue>> ElseAsync<TValue>(this Task<Result<TValue>> task, Func<Error[], Error> onFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
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
    public static async Task<Result<TValue>> ElseAsync<TValue>(this Task<Result<TValue>> task, Func<Error[], Error[]> onFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
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
    public static async Task<Result<TValue>> ElseAsync<TValue>(this Task<Result<TValue>> task, Error error, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
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
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.ElseAsync(onFailure, cancellationToken).ConfigureAwait(false);
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
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.ElseAsync(onFailure, cancellationToken).ConfigureAwait(false);
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
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.ElseAsync(onFailure, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// If the operation failed, executes a function to generate a single error and returns a new Result with that error.
    /// </summary>
    public static async ValueTask<Result<TValue>> ElseAsync<TValue>(this ValueTask<Result<TValue>> task, Func<Error[], TValue> onFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Else(onFailure);
    }

    /// <summary>
    /// If the operation failed, replaces the current errors with the provided value.
    /// </summary>
    public static async ValueTask<Result<TValue>> ElseAsync<TValue>(this ValueTask<Result<TValue>> task, TValue onFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Else(onFailure);
    }

    /// <summary>
    /// If the operation failed, executes an async function to generate a value and returns a new Result with that value.
    /// </summary>
    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result<TValue>> ElseAsync<TValue>(this ValueTask<Result<TValue>> task, Func<Error[], Task<TValue>> onFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.ElseAsync(onFailure, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// If the operation failed, replaces the current errors with the provided async value.
    /// </summary>
    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result<TValue>> ElseAsync<TValue>(this ValueTask<Result<TValue>> task, Task<TValue> onFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.ElseAsync(onFailure, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// If the operation failed, executes a function to generate a single error and returns a new Result with that error.
    /// </summary>
    public static async ValueTask<Result<TValue>> ElseAsync<TValue>(this ValueTask<Result<TValue>> task, Func<Error[], Error> onFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Else(onFailure);
    }

    /// <summary>
    /// If the operation failed, executes a function to generate multiple errors and returns a new Result with those errors.
    /// </summary>
    public static async ValueTask<Result<TValue>> ElseAsync<TValue>(this ValueTask<Result<TValue>> task, Func<Error[], Error[]> onFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Else(onFailure);
    }

    /// <summary>
    /// If the operation failed, replaces the current errors with the provided error.
    /// </summary>
    public static async ValueTask<Result<TValue>> ElseAsync<TValue>(this ValueTask<Result<TValue>> task, Error error, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Else(error);
    }

    /// <summary>
    /// If the operation failed, executes an async function to generate a single error and returns a new Result with that error.
    /// </summary>
    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result<TValue>> ElseAsync<TValue>(this ValueTask<Result<TValue>> task, Func<Error[], Task<Error>> onFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.ElseAsync(onFailure, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// If the operation failed, executes an async function to generate multiple errors and returns a new Result with those errors.
    /// </summary>
    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result<TValue>> ElseAsync<TValue>(this ValueTask<Result<TValue>> task, Func<Error[], Task<Error[]>> onFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.ElseAsync(onFailure, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// If the operation failed, replaces the current errors with the provided async error.
    /// </summary>
    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result<TValue>> ElseAsync<TValue>(this ValueTask<Result<TValue>> task, Task<Error> onFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.ElseAsync(onFailure, cancellationToken).ConfigureAwait(false);
    }

#if NET9_0_OR_GREATER
    /// <summary>
    /// If the operation failed, executes a ValueTask function to generate a value and returns a new Result with that value.
    /// </summary>
    public static async ValueTask<Result<TValue>> ElseAsync<TValue>(this ValueTask<Result<TValue>> task, Func<Error[], ValueTask<TValue>> onFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.ElseAsync(onFailure, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// If the operation failed, replaces the current errors with the provided ValueTask value.
    /// </summary>
    public static async ValueTask<Result<TValue>> ElseAsync<TValue>(this ValueTask<Result<TValue>> task, ValueTask<TValue> onFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.ElseAsync(onFailure, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// If the operation failed, executes a ValueTask function to generate a single error and returns a new Result with that error.
    /// </summary>
    public static async ValueTask<Result<TValue>> ElseAsync<TValue>(this ValueTask<Result<TValue>> task, Func<Error[], ValueTask<Error>> onFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.ElseAsync(onFailure, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// If the operation failed, executes a ValueTask function to generate multiple errors and returns a new Result with those errors.
    /// </summary>
    public static async ValueTask<Result<TValue>> ElseAsync<TValue>(this ValueTask<Result<TValue>> task, Func<Error[], ValueTask<Error[]>> onFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.ElseAsync(onFailure, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// If the operation failed, replaces the current errors with the provided ValueTask error.
    /// </summary>
    public static async ValueTask<Result<TValue>> ElseAsync<TValue>(this ValueTask<Result<TValue>> task, ValueTask<Error> onFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.ElseAsync(onFailure, cancellationToken).ConfigureAwait(false);
    }
#endif
}
