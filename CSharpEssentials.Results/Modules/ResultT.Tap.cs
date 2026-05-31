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

    public async Task<Result<TValue>> TapIfAsync(bool condition, Func<TValue, Task> action, CancellationToken cancellationToken = default)
    {
        if (IsSuccess && condition)
            await action(Value).WithCancellation(cancellationToken);
        return this;
    }

    public async Task<Result<TValue>> TapIfAsync(Func<TValue, bool> predicate, Func<TValue, Task> action, CancellationToken cancellationToken = default)
    {
        if (IsSuccess && predicate(Value))
            await action(Value).WithCancellation(cancellationToken);
        return this;
    }
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
        Result<TValue> result = await task.WithCancellation(cancellationToken);
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
        Result<TValue> result = await task.WithCancellation(cancellationToken);
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
        Result<TValue> result = await task.WithCancellation(cancellationToken);
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
        Result<TValue> result = await task.WithCancellation(cancellationToken);
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
        Result<TValue> result = await task.WithCancellation(cancellationToken);
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
        Result<TValue> result = await task.WithCancellation(cancellationToken);
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
        Result<TValue> result = await task.WithCancellation(cancellationToken);
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
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.Tap(condition, action);
    }

    public static async Task<Result<TValue>> TapIfAsync<TValue>(this Task<Result<TValue>> task, Func<TValue, bool> predicate, Action<TValue> action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.TapIf(predicate, action);
    }

    public static async Task<Result<TValue>> TapIfAsync<TValue>(this Task<Result<TValue>> task, bool condition, Func<TValue, Task> action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.TapIfAsync(condition, action, cancellationToken);
    }

    public static async Task<Result<TValue>> TapIfAsync<TValue>(this Task<Result<TValue>> task, Func<TValue, bool> predicate, Func<TValue, Task> action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.TapIfAsync(predicate, action, cancellationToken);
    }

    public static async ValueTask<Result<TValue>> TapIfAsync<TValue>(this ValueTask<Result<TValue>> task, Func<TValue, bool> predicate, Action<TValue> action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.TapIf(predicate, action);
    }

    public static async ValueTask<Result<TValue>> TapIfAsync<TValue>(this ValueTask<Result<TValue>> task, bool condition, Func<TValue, Task> action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.TapIfAsync(condition, action, cancellationToken);
    }

    public static async ValueTask<Result<TValue>> TapIfAsync<TValue>(this ValueTask<Result<TValue>> task, Func<TValue, bool> predicate, Func<TValue, Task> action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.TapIfAsync(predicate, action, cancellationToken);
    }
}
