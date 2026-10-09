using System.Runtime.CompilerServices;
using CSharpEssentials.Core;
namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result<TValue>
{
    /// <summary>
    /// Executes a function if the result is a success.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public Result<TValue> Tap(Action<TValue> action)
    {
        if (IsSuccess)
            action(Value);
        return this;
    }

    /// <summary>
    /// Executes a function if the result is a success.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public Result<TValue> Tap(Action action)
    {
        if (IsSuccess)
            action();
        return this;
    }

    /// <summary>
    /// Executes a function if the result is a success.
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public Result<TValue> Tap(bool condition, Action<TValue> action)
    {
        if (IsSuccess && condition)
            action(Value);
        return this;
    }

    /// <summary>
    /// Executes a function if the result is a success.
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public Result<TValue> Tap(bool condition, Action action)
    {
        if (IsSuccess && condition)
            action();
        return this;
    }

    /// <summary>
    /// Executes a function if the result is a success.
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public Result<TValue> Tap(Func<bool> condition, Action<TValue> action)
    {
        if (IsSuccess && condition())
            action(Value);
        return this;
    }

    /// <summary>
    /// Executes a function if the result is a success.
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public Result<TValue> Tap(Func<bool> condition, Action action)
    {
        if (IsSuccess && condition())
            action();
        return this;
    }

    public Result<TValue> TapIf(Func<TValue, bool> predicate, Action<TValue> action)
    {
        if (IsSuccess && predicate(Value))
            action(Value);
        return this;
    }

    [OverloadResolutionPriority(1)]
    public async Task<Result<TValue>> TapIfAsync(bool condition, Func<TValue, Task> action, CancellationToken cancellationToken = default)
    {
        if (IsSuccess && condition)
            await action(Value).WithCancellation(cancellationToken).ConfigureAwait(false);
        return this;
    }

    [OverloadResolutionPriority(1)]
    public async Task<Result<TValue>> TapIfAsync(Func<TValue, bool> predicate, Func<TValue, Task> action, CancellationToken cancellationToken = default)
    {
        if (IsSuccess && predicate(Value))
            await action(Value).WithCancellation(cancellationToken).ConfigureAwait(false);
        return this;
    }

    /// <summary>
    /// Runs and awaits an async side effect with the success value. On failure the function is never called.
    /// </summary>
    /// <param name="action">The side effect to run on success.</param>
    /// <param name="cancellationToken">Checked before the function call and observed while awaiting it.</param>
    /// <returns>The original result.</returns>
    [OverloadResolutionPriority(1)]
    public Task<Result<TValue>> TapAsync(Func<TValue, Task> action, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return this.AsTask();
        return TapCoreAsync(this, action, cancellationToken);
    }

    private static async Task<Result<TValue>> TapCoreAsync(Result<TValue> result, Func<TValue, Task> action, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await action(result.Value).WithCancellation(cancellationToken).ConfigureAwait(false);
        return result;
    }

#if NET9_0_OR_GREATER
    /// <summary>
    /// Runs and awaits an async side effect with the success value. On failure the function is never called.
    /// </summary>
    /// <param name="action">The side effect to run on success.</param>
    /// <param name="cancellationToken">Checked before the function call and observed while awaiting it.</param>
    /// <returns>The original result.</returns>
    public ValueTask<Result<TValue>> TapAsync(Func<TValue, ValueTask> action, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return this.AsValueTask();
        return TapCoreAsync(this, action, cancellationToken);
    }

    private static async ValueTask<Result<TValue>> TapCoreAsync(Result<TValue> result, Func<TValue, ValueTask> action, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await action(result.Value).WithCancellation(cancellationToken).ConfigureAwait(false);
        return result;
    }

    /// <summary>
    /// Runs and awaits an async side effect with the success value when the condition is true.
    /// </summary>
    /// <param name="condition">Gates the side effect.</param>
    /// <param name="action">The side effect to run.</param>
    /// <param name="cancellationToken">Observed while awaiting the side effect.</param>
    /// <returns>The original result.</returns>
    public async ValueTask<Result<TValue>> TapIfAsync(bool condition, Func<TValue, ValueTask> action, CancellationToken cancellationToken = default)
    {
        if (IsSuccess && condition)
            await action(Value).WithCancellation(cancellationToken).ConfigureAwait(false);
        return this;
    }

    /// <summary>
    /// Runs and awaits an async side effect with the success value when the predicate holds.
    /// </summary>
    /// <param name="predicate">Gates the side effect. Called only on success.</param>
    /// <param name="action">The side effect to run.</param>
    /// <param name="cancellationToken">Observed while awaiting the side effect.</param>
    /// <returns>The original result.</returns>
    public async ValueTask<Result<TValue>> TapIfAsync(Func<TValue, bool> predicate, Func<TValue, ValueTask> action, CancellationToken cancellationToken = default)
    {
        if (IsSuccess && predicate(Value))
            await action(Value).WithCancellation(cancellationToken).ConfigureAwait(false);
        return this;
    }
#endif
}

public static partial class ResultExtensions
{
    /// <summary>
    /// Executes a function if the result is a success.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<TValue>> TapAsync<TValue>(this Task<Result<TValue>> task, Action<TValue> action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tap(action);
    }

    /// <summary>
    /// Executes a function if the result is a success.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<TValue>> TapAsync<TValue>(this Task<Result<TValue>> task, Action action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tap(action);
    }

    /// <summary>
    /// Executes a function if the result is a success.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="condition"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<TValue>> TapAsync<TValue>(this Task<Result<TValue>> task, bool condition, Action<TValue> action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tap(condition, action);
    }

    /// <summary>
    /// Executes a function if the result is a success.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="condition"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<TValue>> TapAsync<TValue>(this Task<Result<TValue>> task, Func<bool> condition, Action<TValue> action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tap(condition, action);
    }

    /// <summary>
    /// Executes a function if the result is a success.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result<TValue>> TapAsync<TValue>(this ValueTask<Result<TValue>> task, Action<TValue> action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tap(action);
    }

    /// <summary>
    /// Executes a function if the result is a success.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result<TValue>> TapAsync<TValue>(this ValueTask<Result<TValue>> task, Action action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tap(action);
    }

    /// <summary>
    /// Executes a function if the result is a success.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="condition"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result<TValue>> TapAsync<TValue>(this ValueTask<Result<TValue>> task, bool condition, Action<TValue> action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tap(condition, action);
    }

    /// <summary>
    /// Executes a function if the result is a success.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="condition"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result<TValue>> TapAsync<TValue>(this ValueTask<Result<TValue>> task, Func<bool> condition, Action<TValue> action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tap(condition, action);
    }

    public static async Task<Result<TValue>> TapIfAsync<TValue>(this Task<Result<TValue>> task, Func<TValue, bool> predicate, Action<TValue> action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.TapIf(predicate, action);
    }

    public static async Task<Result<TValue>> TapIfAsync<TValue>(this Task<Result<TValue>> task, bool condition, Func<TValue, Task> action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.TapIfAsync(condition, action, cancellationToken).ConfigureAwait(false);
    }

    public static async Task<Result<TValue>> TapIfAsync<TValue>(this Task<Result<TValue>> task, Func<TValue, bool> predicate, Func<TValue, Task> action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.TapIfAsync(predicate, action, cancellationToken).ConfigureAwait(false);
    }

    public static async ValueTask<Result<TValue>> TapIfAsync<TValue>(this ValueTask<Result<TValue>> task, Func<TValue, bool> predicate, Action<TValue> action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.TapIf(predicate, action);
    }

    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result<TValue>> TapIfAsync<TValue>(this ValueTask<Result<TValue>> task, bool condition, Func<TValue, Task> action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.TapIfAsync(condition, action, cancellationToken).ConfigureAwait(false);
    }

    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result<TValue>> TapIfAsync<TValue>(this ValueTask<Result<TValue>> task, Func<TValue, bool> predicate, Func<TValue, Task> action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.TapIfAsync(predicate, action, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Awaits the Task result, then runs and awaits an async side effect with the success value. On failure the function is never called.
    /// </summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="action">The side effect to run on success.</param>
    /// <param name="cancellationToken">Observed while awaiting the source, checked before the function call and observed while awaiting it.</param>
    /// <returns>The original result.</returns>
    public static async Task<Result<TValue>> TapAsync<TValue>(this Task<Result<TValue>> task, Func<TValue, Task> action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.TapAsync(action, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Awaits the Task result, then runs and awaits an async side effect if it is a success. On failure the function is never called.
    /// </summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="action">The side effect to run on success.</param>
    /// <param name="cancellationToken">Observed while awaiting the source, checked before the function call and observed while awaiting it.</param>
    /// <returns>The original result.</returns>
    public static async Task<Result<TValue>> TapAsync<TValue>(this Task<Result<TValue>> task, Func<Task> action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
            return result;
        cancellationToken.ThrowIfCancellationRequested();
        await action().WithCancellation(cancellationToken).ConfigureAwait(false);
        return result;
    }

    /// <summary>
    /// Awaits the ValueTask result, then runs and awaits an async side effect with the success value. On failure the function is never called.
    /// </summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="action">The side effect to run on success.</param>
    /// <param name="cancellationToken">Observed while awaiting the source, checked before the function call and observed while awaiting it.</param>
    /// <returns>The original result.</returns>
    public static async ValueTask<Result<TValue>> TapAsync<TValue>(this ValueTask<Result<TValue>> task, Func<TValue, ValueTask> action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
            return result;
        cancellationToken.ThrowIfCancellationRequested();
        await action(result.Value).WithCancellation(cancellationToken).ConfigureAwait(false);
        return result;
    }

    /// <summary>
    /// Awaits the ValueTask result, then runs and awaits an async side effect if it is a success. On failure the function is never called.
    /// </summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="action">The side effect to run on success.</param>
    /// <param name="cancellationToken">Observed while awaiting the source, checked before the function call and observed while awaiting it.</param>
    /// <returns>The original result.</returns>
    public static async ValueTask<Result<TValue>> TapAsync<TValue>(this ValueTask<Result<TValue>> task, Func<ValueTask> action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
            return result;
        cancellationToken.ThrowIfCancellationRequested();
        await action().WithCancellation(cancellationToken).ConfigureAwait(false);
        return result;
    }

    /// <summary>
    /// Awaits the Task result, then runs and awaits an async side effect with the success value when the condition is true.
    /// </summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="condition">Gates the side effect.</param>
    /// <param name="action">The side effect to run.</param>
    /// <param name="cancellationToken">Observed while awaiting the source, checked before the function call and observed while awaiting it.</param>
    /// <returns>The original result.</returns>
    public static async Task<Result<TValue>> TapAsync<TValue>(this Task<Result<TValue>> task, bool condition, Func<TValue, Task> action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        if (!condition)
            return result;
        return await result.TapAsync(action, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Awaits the Task result, then runs and awaits an async side effect with the success value when the condition returns true.
    /// </summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="condition">Gates the side effect. Called only on success.</param>
    /// <param name="action">The side effect to run.</param>
    /// <param name="cancellationToken">Observed while awaiting the source, checked before the function call and observed while awaiting it.</param>
    /// <returns>The original result.</returns>
    public static async Task<Result<TValue>> TapAsync<TValue>(this Task<Result<TValue>> task, Func<bool> condition, Func<TValue, Task> action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        if (result.IsFailure || !condition())
            return result;
        return await result.TapAsync(action, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Awaits the ValueTask result, then runs and awaits an async side effect with the success value when the condition is true.
    /// </summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="condition">Gates the side effect.</param>
    /// <param name="action">The side effect to run.</param>
    /// <param name="cancellationToken">Observed while awaiting the source, checked before the function call and observed while awaiting it.</param>
    /// <returns>The original result.</returns>
    public static async ValueTask<Result<TValue>> TapAsync<TValue>(this ValueTask<Result<TValue>> task, bool condition, Func<TValue, ValueTask> action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        if (result.IsFailure || !condition)
            return result;
        cancellationToken.ThrowIfCancellationRequested();
        await action(result.Value).WithCancellation(cancellationToken).ConfigureAwait(false);
        return result;
    }

    /// <summary>
    /// Awaits the ValueTask result, then runs and awaits an async side effect with the success value when the condition returns true.
    /// </summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="condition">Gates the side effect. Called only on success.</param>
    /// <param name="action">The side effect to run.</param>
    /// <param name="cancellationToken">Observed while awaiting the source, checked before the function call and observed while awaiting it.</param>
    /// <returns>The original result.</returns>
    public static async ValueTask<Result<TValue>> TapAsync<TValue>(this ValueTask<Result<TValue>> task, Func<bool> condition, Func<TValue, ValueTask> action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        if (result.IsFailure || !condition())
            return result;
        cancellationToken.ThrowIfCancellationRequested();
        await action(result.Value).WithCancellation(cancellationToken).ConfigureAwait(false);
        return result;
    }

#if NET9_0_OR_GREATER
    /// <summary>
    /// Awaits the ValueTask result, then runs and awaits an async side effect with the success value when the condition is true.
    /// </summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="condition">Gates the side effect.</param>
    /// <param name="action">The side effect to run.</param>
    /// <param name="cancellationToken">Observed while awaiting the source and the side effect.</param>
    /// <returns>The original result.</returns>
    public static async ValueTask<Result<TValue>> TapIfAsync<TValue>(this ValueTask<Result<TValue>> task, bool condition, Func<TValue, ValueTask> action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.TapIfAsync(condition, action, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Awaits the ValueTask result, then runs and awaits an async side effect with the success value when the predicate holds.
    /// </summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Gates the side effect. Called only on success.</param>
    /// <param name="action">The side effect to run.</param>
    /// <param name="cancellationToken">Observed while awaiting the source and the side effect.</param>
    /// <returns>The original result.</returns>
    public static async ValueTask<Result<TValue>> TapIfAsync<TValue>(this ValueTask<Result<TValue>> task, Func<TValue, bool> predicate, Func<TValue, ValueTask> action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.TapIfAsync(predicate, action, cancellationToken).ConfigureAwait(false);
    }
#endif
}
