using CSharpEssentials.Core;
namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result
{
    /// <summary>
    /// Executes an action if the result is a success.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public Result Tap(Action action)
    {
        if (IsSuccess)
            action();
        return this;
    }

    /// <summary>
    /// Executes an action if the result is a success.
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public Result Tap(bool condition, Action action)
    {
        if (IsSuccess && condition)
            action();
        return this;
    }

    /// <summary>
    /// Executes an action if the result is a success.
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public Result Tap(Func<bool> condition, Action action)
    {
        if (IsSuccess && condition())
            action();
        return this;
    }
}

public static partial class ResultExtensions
{
    /// <summary>
    /// Executes an action if the result is a success.
    /// </summary>
    /// <param name="task"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result> TapAsync(this Task<Result> task, Action action, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return result.Tap(action);
    }

    /// <summary>
    /// Executes an action if the result is a success.
    /// </summary>
    /// <param name="task"></param>
    /// <param name="condition"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result> TapAsync(this Task<Result> task, bool condition, Action action, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return result.Tap(condition, action);
    }

    /// <summary>
    /// Executes an action if the result is a success.
    /// </summary>
    /// <param name="task"></param>
    /// <param name="condition"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result> TapAsync(this Task<Result> task, Func<bool> condition, Action action, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return result.Tap(condition, action);
    }

    /// <summary>
    /// Executes an action if the result is a success.
    /// </summary>
    /// <param name="task"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result> TapAsync(this ValueTask<Result> task, Action action, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return result.Tap(action);
    }

    /// <summary>
    /// Executes an action if the result is a success.
    /// </summary>
    /// <param name="task"></param>
    /// <param name="condition"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result> TapAsync(this ValueTask<Result> task, bool condition, Action action, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return result.Tap(condition, action);
    }

    /// <summary>
    /// Executes an action if the result is a success.
    /// </summary>
    /// <param name="task"></param>
    /// <param name="condition"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result> TapAsync(this ValueTask<Result> task, Func<bool> condition, Action action, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return result.Tap(condition, action);
    }
}
