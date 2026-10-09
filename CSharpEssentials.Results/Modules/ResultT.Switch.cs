using System.Runtime.CompilerServices;
using CSharpEssentials.Core;
using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result<TValue>
{
    /// <summary>
    /// Executes the provided <paramref name="onSuccess"/> action if the result is a value; otherwise the <paramref name="onError"/> action is executed.
    /// </summary>
    /// <param name="onSuccess"></param>
    /// <param name="onError"></param>
    public void Switch(Action<TValue> onSuccess, Action<Error[]> onError)
    {
        if (IsFailure)
        {
            onError(Errors);
            return;
        }
        onSuccess(Value);
    }

    /// <summary>
    /// Executes the provided <paramref name="onSuccess"/> function if the result is a value; otherwise the <paramref name="onError"/> function is executed.
    /// </summary>
    /// <param name="onSuccess"></param>
    /// <param name="onError"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(1)]
    public async Task SwitchAsync(Func<TValue, Task> onSuccess, Func<Error[], Task> onError, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
        {
            await onError(Errors).WithCancellation(cancellationToken).ConfigureAwait(false);
            return;
        }
        await onSuccess(Value).WithCancellation(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the provided <paramref name="onSuccess"/> action if the result is a value; otherwise the <paramref name="onFirstError"/> action is executed.
    /// </summary>
    /// <param name="onSuccess"></param>
    /// <param name="onFirstError"></param>
    public void SwitchFirst(Action<TValue> onSuccess, Action<Error> onFirstError)
    {
        if (IsFailure)
        {
            onFirstError(FirstError);
            return;
        }
        onSuccess(Value);
    }

    /// <summary>
    /// Executes the provided <paramref name="onSuccess"/> function if the result is a value; otherwise the <paramref name="onFirstError"/> function is executed.
    /// </summary>
    /// <param name="onSuccess"></param>
    /// <param name="onFirstError"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(1)]
    public async Task SwitchFirstAsync(Func<TValue, Task> onSuccess, Func<Error, Task> onFirstError, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
        {
            await onFirstError(FirstError).WithCancellation(cancellationToken).ConfigureAwait(false);
            return;
        }
        await onSuccess(Value).WithCancellation(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///  Executes the provided <paramref name="onSuccess"/> action if the result is a value; otherwise the <paramref name="onLastError"/> action is executed.
    /// </summary>
    /// <param name="onSuccess"></param>
    /// <param name="onLastError"></param>
    public void SwitchLast(Action<TValue> onSuccess, Action<Error> onLastError)
    {
        if (IsFailure)
        {
            onLastError(LastError);
            return;
        }
        onSuccess(Value);
    }

    /// <summary>
    /// Executes the provided <paramref name="onSuccess"/> function if the result is a value; otherwise the <paramref name="onLastError"/> function is executed.
    /// </summary>
    /// <param name="onSuccess"></param>
    /// <param name="onLastError"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(1)]
    public async Task SwitchLastAsync(Func<TValue, Task> onSuccess, Func<Error, Task> onLastError, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
        {
            await onLastError(LastError).WithCancellation(cancellationToken).ConfigureAwait(false);
            return;
        }
        await onSuccess(Value).WithCancellation(cancellationToken).ConfigureAwait(false);
    }

#if NET9_0_OR_GREATER
    /// <summary>
    /// Executes the provided <paramref name="onSuccess"/> ValueTask function if the result is a value; otherwise the <paramref name="onError"/> function is executed.
    /// </summary>
    /// <param name="onSuccess">An async action to execute with the success value.</param>
    /// <param name="onError">An async action to execute with the errors.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A ValueTask representing the asynchronous operation.</returns>
    public async ValueTask SwitchAsync(Func<TValue, ValueTask> onSuccess, Func<Error[], ValueTask> onError, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
        {
            await onError(Errors).WithCancellation(cancellationToken).ConfigureAwait(false);
            return;
        }
        await onSuccess(Value).WithCancellation(cancellationToken).ConfigureAwait(false);
    }
    /// <summary>
    /// Asynchronously switches on the result, executing a corresponding ValueTask action for success or the first encountered error.
    /// </summary>
    /// <param name="onSuccess">An async action to execute with the success value.</param>
    /// <param name="onFirstError">An async action to execute on the first error encountered.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A ValueTask representing the asynchronous operation.</returns>
    public async ValueTask SwitchFirstAsync(Func<TValue, ValueTask> onSuccess, Func<Error, ValueTask> onFirstError, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
        {
            await onFirstError(FirstError).WithCancellation(cancellationToken).ConfigureAwait(false);
            return;
        }
        await onSuccess(Value).WithCancellation(cancellationToken).ConfigureAwait(false);
    }


    /// <summary>
    /// Asynchronously switches on the result, executing a corresponding ValueTask action for success or the last encountered error.
    /// </summary>
    /// <param name="onSuccess">An async action to execute with the success value.</param>
    /// <param name="onLastError">An async action to execute on the last error encountered.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A ValueTask representing the asynchronous operation.</returns>
    public async ValueTask SwitchLastAsync(Func<TValue, ValueTask> onSuccess, Func<Error, ValueTask> onLastError, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
        {
            await onLastError(LastError).WithCancellation(cancellationToken).ConfigureAwait(false);
            return;
        }
        await onSuccess(Value).WithCancellation(cancellationToken).ConfigureAwait(false);
    }
#endif
}

public static partial class ResultExtensions
{
    /// <summary>
    /// Executes the provided <paramref name="onSuccess"/> action if the result is a value; otherwise the <paramref name="onError"/> action is executed.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="onSuccess"></param>
    /// <param name="onError"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task SwitchAsync<TValue>(this Task<Result<TValue>> task, Action<TValue> onSuccess, Action<Error[]> onError, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        result.Switch(onSuccess, onError);
    }

    /// <summary>
    /// Executes the provided <paramref name="onSuccess"/> function if the result is a value; otherwise the <paramref name="onError"/> function is executed.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="onSuccess"></param>
    /// <param name="onError"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task SwitchAsync<TValue>(this Task<Result<TValue>> task, Func<TValue, Task> onSuccess, Func<Error[], Task> onError, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        await result.SwitchAsync(onSuccess, onError, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the provided <paramref name="onSuccess"/> action if the result is a value;
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="onSuccess"></param>
    /// <param name="onError"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task SwitchFirstAsync<TValue>(this Task<Result<TValue>> task, Action<TValue> onSuccess, Action<Error> onError, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        result.SwitchFirst(onSuccess, onError);
    }

    /// <summary>
    /// Executes the provided <paramref name="onSuccess"/> function if the result is a value;
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="onSuccess"></param>
    /// <param name="onError"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task SwitchFirstAsync<TValue>(this Task<Result<TValue>> task, Func<TValue, Task> onSuccess, Func<Error, Task> onError, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        await result.SwitchFirstAsync(onSuccess, onError, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the provided <paramref name="onSuccess"/> action if the result is a value;
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="onSuccess"></param>
    /// <param name="onError"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task SwitchLastAsync<TValue>(this Task<Result<TValue>> task, Action<TValue> onSuccess, Action<Error> onError, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        result.SwitchLast(onSuccess, onError);
    }

    /// <summary>
    /// Executes the provided <paramref name="onSuccess"/> function if the result is a value;
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="onSuccess"></param>
    /// <param name="onError"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task SwitchLastAsync<TValue>(this Task<Result<TValue>> task, Func<TValue, Task> onSuccess, Func<Error, Task> onError, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        await result.SwitchLastAsync(onSuccess, onError, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the provided action if the result is a value; otherwise the error action is executed.
    /// </summary>
    public static async ValueTask SwitchAsync<TValue>(this ValueTask<Result<TValue>> task, Action<TValue> onSuccess, Action<Error[]> onError, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        result.Switch(onSuccess, onError);
    }

    /// <summary>
    /// Executes the provided async function if the result is a value; otherwise the async error function is executed.
    /// </summary>
    [OverloadResolutionPriority(1)]
    public static async ValueTask SwitchAsync<TValue>(this ValueTask<Result<TValue>> task, Func<TValue, Task> onSuccess, Func<Error[], Task> onError, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        await result.SwitchAsync(onSuccess, onError, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the provided action if the result is a value; otherwise the first error action is executed.
    /// </summary>
    public static async ValueTask SwitchFirstAsync<TValue>(this ValueTask<Result<TValue>> task, Action<TValue> onSuccess, Action<Error> onError, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        result.SwitchFirst(onSuccess, onError);
    }

    /// <summary>
    /// Executes the provided async function if the result is a value; otherwise the first error async function is executed.
    /// </summary>
    [OverloadResolutionPriority(1)]
    public static async ValueTask SwitchFirstAsync<TValue>(this ValueTask<Result<TValue>> task, Func<TValue, Task> onSuccess, Func<Error, Task> onError, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        await result.SwitchFirstAsync(onSuccess, onError, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the provided action if the result is a value; otherwise the last error action is executed.
    /// </summary>
    public static async ValueTask SwitchLastAsync<TValue>(this ValueTask<Result<TValue>> task, Action<TValue> onSuccess, Action<Error> onError, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        result.SwitchLast(onSuccess, onError);
    }

    /// <summary>
    /// Executes the provided async function if the result is a value; otherwise the last error async function is executed.
    /// </summary>
    [OverloadResolutionPriority(1)]
    public static async ValueTask SwitchLastAsync<TValue>(this ValueTask<Result<TValue>> task, Func<TValue, Task> onSuccess, Func<Error, Task> onError, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        await result.SwitchLastAsync(onSuccess, onError, cancellationToken).ConfigureAwait(false);
    }

#if NET9_0_OR_GREATER
    /// <summary>
    /// Executes the provided ValueTask function if the result is a value; otherwise the ValueTask error function is executed.
    /// </summary>
    public static async ValueTask SwitchAsync<TValue>(this ValueTask<Result<TValue>> task, Func<TValue, ValueTask> onSuccess, Func<Error[], ValueTask> onError, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        await result.SwitchAsync(onSuccess, onError, cancellationToken).ConfigureAwait(false);
    }
    /// <summary>
    /// Awaits the ValueTask result and executes the matching ValueTask action for success or the first error.
    /// </summary>
    public static async ValueTask SwitchFirstAsync<TValue>(this ValueTask<Result<TValue>> task, Func<TValue, ValueTask> onSuccess, Func<Error, ValueTask> onError, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        await result.SwitchFirstAsync(onSuccess, onError, cancellationToken).ConfigureAwait(false);
    }


    /// <summary>
    /// Awaits the ValueTask result and executes the matching ValueTask action for success or the last error.
    /// </summary>
    public static async ValueTask SwitchLastAsync<TValue>(this ValueTask<Result<TValue>> task, Func<TValue, ValueTask> onSuccess, Func<Error, ValueTask> onError, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        await result.SwitchLastAsync(onSuccess, onError, cancellationToken).ConfigureAwait(false);
    }
#endif
}
