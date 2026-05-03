using CSharpEssentials.Core;
namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result<TValue>
{
    /// <summary>
    /// Executes a function regardless of the result state.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="func"></param>
    /// <returns></returns>
    public TOut Finally<TOut>(Func<Result<TValue>, TOut> func) => func(this);

    /// <summary>
    /// Executes an action regardless of the result state.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public Result<TValue> Finally(Action<Result<TValue>> action)
    {
        action(this);
        return this;
    }
}

public static partial class ResultExtensions
{
    /// <summary>
    /// Executes a function regardless of the result state.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="task"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<TOut> FinallyAsync<TValue, TOut>(this Task<Result<TValue>> task, Func<Result<TValue>, TOut> func, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.Finally(func);
    }

    /// <summary>
    /// Executes a function regardless of the result state.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="task"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<TOut> FinallyAsync<TValue, TOut>(this Task<Result<TValue>> task, Func<Result<TValue>, Task<TOut>> func, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await func(result).WithCancellation(cancellationToken);
    }

    /// <summary>
    /// Executes an action regardless of the result state.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<TValue>> FinallyAsync<TValue>(this Task<Result<TValue>> task, Action<Result<TValue>> action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.Finally(action);
    }

    /// <summary>
    /// Executes an action regardless of the result state.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<TValue>> FinallyAsync<TValue>(this Task<Result<TValue>> task, Func<Result<TValue>, Task> action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        await action(result).WithCancellation(cancellationToken);
        return result;
    }

    /// <summary>
    /// Executes a function regardless of the result state.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="task"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<TOut> FinallyAsync<TValue, TOut>(this ValueTask<Result<TValue>> task, Func<Result<TValue>, TOut> func, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.Finally(func);
    }


    /// <summary>
    /// Executes an action regardless of the result state.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result<TValue>> FinallyAsync<TValue>(this ValueTask<Result<TValue>> task, Action<Result<TValue>> action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.Finally(action);
    }

    /// <summary>
    /// Executes an action regardless of the result state.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result<TValue>> FinallyAsync<TValue>(this ValueTask<Result<TValue>> task, Func<Result<TValue>, Task> action, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        await action(result).WithCancellation(cancellationToken);
        return result;
    }
}
