using CSharpEssentials.Core;
namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result
{
    /// <summary>
    /// Executes a function regardless of the result state.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="func"></param>
    /// <returns></returns>
    public TOut Finally<TOut>(Func<Result, TOut> func) => func(this);

    /// <summary>
    /// Executes an action regardless of the result state.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public Result Finally(Action<Result> action)
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
    /// <typeparam name="TOut"></typeparam>
    /// <param name="task"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<TOut> FinallyAsync<TOut>(this Task<Result> task, Func<Result, TOut> func, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return result.Finally(func);
    }

    /// <summary>
    /// Executes a function regardless of the result state.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="task"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<TOut> FinallyAsync<TOut>(this Task<Result> task, Func<Result, Task<TOut>> func, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return await func(result).WithCancellation(cancellationToken);
    }

    /// <summary>
    /// Executes an action regardless of the result state.
    /// </summary>
    /// <param name="task"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result> FinallyAsync(this Task<Result> task, Action<Result> action, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return result.Finally(action);
    }

    /// <summary>
    /// Executes an action regardless of the result state.
    /// </summary>
    /// <param name="task"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result> FinallyAsync(this Task<Result> task, Func<Result, Task> action, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        await action(result).WithCancellation(cancellationToken);
        return result;
    }

    /// <summary>
    /// Executes a function regardless of the result state.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="task"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<TOut> FinallyAsync<TOut>(this ValueTask<Result> task, Func<Result, TOut> func, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return result.Finally(func);
    }


    /// <summary>
    /// Executes an action regardless of the result state.
    /// </summary>
    /// <param name="task"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result> FinallyAsync(this ValueTask<Result> task, Action<Result> action, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return result.Finally(action);
    }

    /// <summary>
    /// Executes an action regardless of the result state.
    /// </summary>
    /// <param name="task"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result> FinallyAsync(this ValueTask<Result> task, Func<Result, Task> action, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        await action(result).WithCancellation(cancellationToken);
        return result;
    }
}
