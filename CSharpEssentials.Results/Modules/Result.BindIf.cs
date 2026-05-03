using CSharpEssentials.Core;
namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result
{
    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public Result BindIf(bool condition, Func<Result> func) =>
        condition ? Bind(func) : this;

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public Result BindIf(Func<bool> predicate, Func<Result> func) =>
        predicate() ? Bind(func) : this;
}

public static partial class ResultExtensions
{
    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <param name="task"></param>
    /// <param name="condition"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result> BindIfAsync(this Task<Result> task, bool condition, Func<Result> func, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return result.BindIf(condition, func);
    }

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <param name="task"></param>
    /// <param name="predicate"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result> BindIfAsync(this Task<Result> task, Func<bool> predicate, Func<Result> func, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return result.BindIf(predicate, func);
    }

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <param name="task"></param>
    /// <param name="condition"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result> BindIfAsync(this ValueTask<Result> task, bool condition, Func<Result> func, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return result.BindIf(condition, func);
    }

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <param name="task"></param>
    /// <param name="predicate"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result> BindIfAsync(this ValueTask<Result> task, Func<bool> predicate, Func<Result> func, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return result.BindIf(predicate, func);
    }
}
