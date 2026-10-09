using System.Runtime.CompilerServices;
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

    /// <summary>
    /// Runs and awaits an async side effect if the result is a success. On failure the function is never called.
    /// </summary>
    /// <param name="action">The side effect to run on success.</param>
    /// <param name="cancellationToken">Checked before the function call and observed while awaiting it.</param>
    /// <returns>The original result.</returns>
    [OverloadResolutionPriority(1)]
    public Task<Result> TapAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return this.AsTask();
        return TapCoreAsync(this, action, cancellationToken);
    }

    private static async Task<Result> TapCoreAsync(Result result, Func<Task> action, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await action().WithCancellation(cancellationToken).ConfigureAwait(false);
        return result;
    }

#if NET9_0_OR_GREATER
    /// <summary>
    /// Runs and awaits an async side effect if the result is a success. On failure the function is never called.
    /// </summary>
    /// <param name="action">The side effect to run on success.</param>
    /// <param name="cancellationToken">Checked before the function call and observed while awaiting it.</param>
    /// <returns>The original result.</returns>
    public ValueTask<Result> TapAsync(Func<ValueTask> action, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return this.AsValueTask();
        return TapCoreAsync(this, action, cancellationToken);
    }

    private static async ValueTask<Result> TapCoreAsync(Result result, Func<ValueTask> action, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await action().WithCancellation(cancellationToken).ConfigureAwait(false);
        return result;
    }
#endif
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
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
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
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
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
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
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
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
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
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
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
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tap(condition, action);
    }

    /// <summary>
    /// Awaits the Task result, then runs and awaits an async side effect if it is a success. On failure the function is never called.
    /// </summary>
    /// <param name="task">The pending result.</param>
    /// <param name="action">The side effect to run on success.</param>
    /// <param name="cancellationToken">Observed while awaiting the source, checked before the function call and observed while awaiting it.</param>
    /// <returns>The original result.</returns>
    public static async Task<Result> TapAsync(this Task<Result> task, Func<Task> action, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.TapAsync(action, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Awaits the ValueTask result, then runs and awaits an async side effect if it is a success. On failure the function is never called.
    /// </summary>
    /// <param name="task">The pending result.</param>
    /// <param name="action">The side effect to run on success.</param>
    /// <param name="cancellationToken">Observed while awaiting the source, checked before the function call and observed while awaiting it.</param>
    /// <returns>The original result.</returns>
    public static async ValueTask<Result> TapAsync(this ValueTask<Result> task, Func<ValueTask> action, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
            return result;
        cancellationToken.ThrowIfCancellationRequested();
        await action().WithCancellation(cancellationToken).ConfigureAwait(false);
        return result;
    }

    /// <summary>
    /// Awaits the Task result, then runs and awaits an async side effect if it is a success and the condition is true.
    /// </summary>
    /// <param name="task">The pending result.</param>
    /// <param name="condition">Gates the side effect.</param>
    /// <param name="action">The side effect to run.</param>
    /// <param name="cancellationToken">Observed while awaiting the source, checked before the function call and observed while awaiting it.</param>
    /// <returns>The original result.</returns>
    public static async Task<Result> TapAsync(this Task<Result> task, bool condition, Func<Task> action, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        if (!condition)
            return result;
        return await result.TapAsync(action, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Awaits the Task result, then runs and awaits an async side effect if it is a success and the condition returns true.
    /// </summary>
    /// <param name="task">The pending result.</param>
    /// <param name="condition">Gates the side effect. Called only on success.</param>
    /// <param name="action">The side effect to run.</param>
    /// <param name="cancellationToken">Observed while awaiting the source, checked before the function call and observed while awaiting it.</param>
    /// <returns>The original result.</returns>
    public static async Task<Result> TapAsync(this Task<Result> task, Func<bool> condition, Func<Task> action, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        if (result.IsFailure || !condition())
            return result;
        return await result.TapAsync(action, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Awaits the ValueTask result, then runs and awaits an async side effect if it is a success and the condition is true.
    /// </summary>
    /// <param name="task">The pending result.</param>
    /// <param name="condition">Gates the side effect.</param>
    /// <param name="action">The side effect to run.</param>
    /// <param name="cancellationToken">Observed while awaiting the source, checked before the function call and observed while awaiting it.</param>
    /// <returns>The original result.</returns>
    public static async ValueTask<Result> TapAsync(this ValueTask<Result> task, bool condition, Func<ValueTask> action, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        if (result.IsFailure || !condition)
            return result;
        cancellationToken.ThrowIfCancellationRequested();
        await action().WithCancellation(cancellationToken).ConfigureAwait(false);
        return result;
    }

    /// <summary>
    /// Awaits the ValueTask result, then runs and awaits an async side effect if it is a success and the condition returns true.
    /// </summary>
    /// <param name="task">The pending result.</param>
    /// <param name="condition">Gates the side effect. Called only on success.</param>
    /// <param name="action">The side effect to run.</param>
    /// <param name="cancellationToken">Observed while awaiting the source, checked before the function call and observed while awaiting it.</param>
    /// <returns>The original result.</returns>
    public static async ValueTask<Result> TapAsync(this ValueTask<Result> task, Func<bool> condition, Func<ValueTask> action, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        if (result.IsFailure || !condition())
            return result;
        cancellationToken.ThrowIfCancellationRequested();
        await action().WithCancellation(cancellationToken).ConfigureAwait(false);
        return result;
    }
}
