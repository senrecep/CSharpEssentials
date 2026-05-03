using CSharpEssentials.Core;
namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result<TValue>
{
    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public Result<TValue> BindIf(bool condition, Func<TValue, Result<TValue>> func) =>
        condition ? Bind(func) : this;

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public Result<TValue> BindIf(Func<bool> predicate, Func<TValue, Result<TValue>> func) =>
        predicate() ? Bind(func) : this;

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public Result<TValue> BindIf(Func<TValue, bool> predicate, Func<TValue, Result<TValue>> func) =>
        IsSuccess && predicate(Value) ? Bind(func) : this;
}

public static partial class ResultExtensions
{
    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="condition"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<TValue>> BindIfAsync<TValue>(this Task<Result<TValue>> task, bool condition, Func<TValue, Result<TValue>> func, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.BindIf(condition, func);
    }

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="predicate"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<TValue>> BindIfAsync<TValue>(this Task<Result<TValue>> task, Func<bool> predicate, Func<TValue, Result<TValue>> func, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.BindIf(predicate, func);
    }

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="predicate"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<TValue>> BindIfAsync<TValue>(this Task<Result<TValue>> task, Func<TValue, bool> predicate, Func<TValue, Result<TValue>> func, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.BindIf(predicate, func);
    }

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="condition"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result<TValue>> BindIfAsync<TValue>(this ValueTask<Result<TValue>> task, bool condition, Func<TValue, Result<TValue>> func, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.BindIf(condition, func);
    }

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="predicate"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result<TValue>> BindIfAsync<TValue>(this ValueTask<Result<TValue>> task, Func<bool> predicate, Func<TValue, Result<TValue>> func, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.BindIf(predicate, func);
    }

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="predicate"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result<TValue>> BindIfAsync<TValue>(this ValueTask<Result<TValue>> task, Func<TValue, bool> predicate, Func<TValue, Result<TValue>> func, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.BindIf(predicate, func);
    }
}
