using CSharpEssentials.Errors;
using CSharpEssentials.Core;

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
    public async Task SwitchAsync(Func<TValue, Task> onSuccess, Func<Error[], Task> onError, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
        {
            await onError(Errors).WithCancellation(cancellationToken);
            return;
        }
        await onSuccess(Value).WithCancellation(cancellationToken);
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
    public async Task SwitchFirstAsync(Func<TValue, Task> onSuccess, Func<Error, Task> onFirstError, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
        {
            await onFirstError(FirstError).WithCancellation(cancellationToken);
            return;
        }
        await onSuccess(Value).WithCancellation(cancellationToken);
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
    public async Task SwitchLastAsync(Func<TValue, Task> onSuccess, Func<Error, Task> onLastError, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
        {
            await onLastError(LastError).WithCancellation(cancellationToken);
            return;
        }
        await onSuccess(Value).WithCancellation(cancellationToken);
    }
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
    public static async Task Switch<TValue>(this Task<Result<TValue>> task, Action<TValue> onSuccess, Action<Error[]> onError, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
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
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        await result.SwitchAsync(onSuccess, onError, cancellationToken);
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
    public static async Task SwitchFirst<TValue>(this Task<Result<TValue>> task, Action<TValue> onSuccess, Action<Error> onError, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
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
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        await result.SwitchFirstAsync(onSuccess, onError, cancellationToken);
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
    public static async Task SwitchLast<TValue>(this Task<Result<TValue>> task, Action<TValue> onSuccess, Action<Error> onError, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
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
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        await result.SwitchLastAsync(onSuccess, onError, cancellationToken);
    }

    /// <summary>
    /// Executes the provided action if the result is a value; otherwise the error action is executed.
    /// </summary>
    public static async ValueTask Switch<TValue>(this ValueTask<Result<TValue>> task, Action<TValue> onSuccess, Action<Error[]> onError, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        result.Switch(onSuccess, onError);
    }

    /// <summary>
    /// Executes the provided async function if the result is a value; otherwise the async error function is executed.
    /// </summary>
    public static async ValueTask SwitchAsync<TValue>(this ValueTask<Result<TValue>> task, Func<TValue, Task> onSuccess, Func<Error[], Task> onError, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        await result.SwitchAsync(onSuccess, onError, cancellationToken);
    }

    /// <summary>
    /// Executes the provided action if the result is a value; otherwise the first error action is executed.
    /// </summary>
    public static async ValueTask SwitchFirst<TValue>(this ValueTask<Result<TValue>> task, Action<TValue> onSuccess, Action<Error> onError, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        result.SwitchFirst(onSuccess, onError);
    }

    /// <summary>
    /// Executes the provided async function if the result is a value; otherwise the first error async function is executed.
    /// </summary>
    public static async ValueTask SwitchFirstAsync<TValue>(this ValueTask<Result<TValue>> task, Func<TValue, Task> onSuccess, Func<Error, Task> onError, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        await result.SwitchFirstAsync(onSuccess, onError, cancellationToken);
    }

    /// <summary>
    /// Executes the provided action if the result is a value; otherwise the last error action is executed.
    /// </summary>
    public static async ValueTask SwitchLast<TValue>(this ValueTask<Result<TValue>> task, Action<TValue> onSuccess, Action<Error> onError, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        result.SwitchLast(onSuccess, onError);
    }

    /// <summary>
    /// Executes the provided async function if the result is a value; otherwise the last error async function is executed.
    /// </summary>
    public static async ValueTask SwitchLastAsync<TValue>(this ValueTask<Result<TValue>> task, Func<TValue, Task> onSuccess, Func<Error, Task> onError, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        await result.SwitchLastAsync(onSuccess, onError, cancellationToken);
    }
}