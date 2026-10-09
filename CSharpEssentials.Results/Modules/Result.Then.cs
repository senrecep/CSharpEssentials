using System.Runtime.CompilerServices;
using CSharpEssentials.Core;

namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result
{
    /// <summary>
    /// Executes the provided function if the result is successful.
    /// </summary>
    /// <param name="onSuccess">A function to execute on success.</param>
    /// <returns>A new result.</returns>
    public Result Then(Func<Result> onSuccess)
    {
        if (IsFailure)
            return this;

        return onSuccess();
    }

    /// <summary>
    /// Executes an action if the result is successful.
    /// </summary>
    /// <param name="action">An action to execute on success.</param>
    public Result ThenDo(Action action)
    {
        if (IsFailure)
            return this;

        action();

        return this;
    }


    /// <summary>
    /// Asynchronously executes the provided async function if the result is successful.
    /// </summary>
    /// <param name="onSuccess">An async function to execute on success.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation that returns a new result.</returns>
    [OverloadResolutionPriority(1)]
    public async Task<Result> ThenAsync(Func<Task<Result>> onSuccess, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return this;

        return await onSuccess().WithCancellation(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously executes the provided async action if the result is successful.
    /// </summary>
    /// <param name="action">An async action to execute on success.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [OverloadResolutionPriority(1)]
    public async Task<Result> ThenDoAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return this;

        await action().WithCancellation(cancellationToken).ConfigureAwait(false);

        return this;
    }

#if NET9_0_OR_GREATER
    /// <summary>
    /// Asynchronously executes the provided ValueTask function if the result is successful.
    /// </summary>
    /// <param name="onSuccess">An async function to execute on success.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>The new result, or the original failure.</returns>
    public async ValueTask<Result> ThenAsync(Func<ValueTask<Result>> onSuccess, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return this;
        return await onSuccess().WithCancellation(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously executes the provided ValueTask action if the result is successful.
    /// </summary>
    /// <param name="action">An async action to execute on success.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>The original result.</returns>
    public async ValueTask<Result> ThenDoAsync(Func<ValueTask> action, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return this;
        await action().WithCancellation(cancellationToken).ConfigureAwait(false);
        return this;
    }
#endif
}

public static partial class ResultExtensions
{
    /// <summary>
    /// Awaits the result of a task, then executes the specified function if the result represents success,
    /// and returns its result.
    /// </summary>
    /// <param name="task">The task that produces a <see cref="Result"/>.</param>
    /// <param name="onSuccess">The function to execute if the result is successful.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A new <see cref="Result"/> based on the provided function or the current failure result.</returns>
    public static async Task<Result> ThenAsync(this Task<Result> task, Func<Result> onSuccess, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Then(onSuccess);
    }

    /// <summary>
    /// Awaits the result of a task, then executes the specified action if the result represents success.
    /// Returns the current result unchanged.
    /// </summary>
    /// <param name="task">The task that produces a <see cref="Result"/>.</param>
    /// <param name="action">The action to execute if the result is successful.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The current <see cref="Result"/>.</returns>
    public static async Task<Result> ThenDoAsync(this Task<Result> task, Action action, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.ThenDo(action);
    }

    /// <summary>
    /// Awaits the result of a task, then asynchronously executes the specified function if the result represents success,
    /// and returns its result.
    /// </summary>
    /// <param name="task">The task that produces a <see cref="Result"/>.</param>
    /// <param name="onSuccess">The async function to execute if the result is successful.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A new <see cref="Result"/> based on the provided function or the current failure result.</returns>
    public static async Task<Result> ThenAsync(this Task<Result> task, Func<Task<Result>> onSuccess, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);

        return await result.ThenAsync(onSuccess, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Awaits the result of a task, then asynchronously executes the specified action if the result represents success.
    /// Returns the current result unchanged.
    /// </summary>
    /// <param name="task">The task that produces a <see cref="Result"/>.</param>
    /// <param name="action">The async action to execute if the result is successful.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The current <see cref="Result"/>.</returns>
    public static async Task<Result> ThenDoAsync(this Task<Result> task, Func<Task> action, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);

        return await result.ThenDoAsync(action, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Awaits the result of a ValueTask, then executes the specified function if the result represents success.
    /// </summary>
    public static async ValueTask<Result> ThenAsync(this ValueTask<Result> task, Func<Result> onSuccess, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Then(onSuccess);
    }

    /// <summary>
    /// Awaits the result of a ValueTask, then executes the specified action if the result represents success.
    /// </summary>
    public static async ValueTask<Result> ThenDoAsync(this ValueTask<Result> task, Action action, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.ThenDo(action);
    }

    /// <summary>
    /// Awaits the result of a ValueTask, then asynchronously executes the specified function if the result represents success.
    /// </summary>
    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result> ThenAsync(this ValueTask<Result> task, Func<Task<Result>> onSuccess, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.ThenAsync(onSuccess, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Awaits the result of a ValueTask, then asynchronously executes the specified action if the result represents success.
    /// </summary>
    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result> ThenDoAsync(this ValueTask<Result> task, Func<Task> action, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.ThenDoAsync(action, cancellationToken).ConfigureAwait(false);
    }

#if NET9_0_OR_GREATER
    /// <summary>
    /// Awaits the ValueTask result, then executes the ValueTask function if it is a success.
    /// </summary>
    public static async ValueTask<Result> ThenAsync(this ValueTask<Result> task, Func<ValueTask<Result>> onSuccess, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.ThenAsync(onSuccess, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Awaits the ValueTask result, then executes the ValueTask action if it is a success.
    /// </summary>
    public static async ValueTask<Result> ThenDoAsync(this ValueTask<Result> task, Func<ValueTask> action, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.ThenDoAsync(action, cancellationToken).ConfigureAwait(false);
    }
#endif
}
