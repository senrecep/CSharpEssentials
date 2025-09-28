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
    public async Task<Result> ThenAsync(Func<Task<Result>> onSuccess, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return this;

        return await onSuccess().WithCancellation(cancellationToken);
    }

    /// <summary>
    /// Asynchronously executes the provided async action if the result is successful.
    /// </summary>
    /// <param name="action">An async action to execute on success.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task<Result> ThenDoAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return this;

        await action().WithCancellation(cancellationToken);

        return this;
    }
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
    public static async Task<Result> Then(this Task<Result> task, Func<Result> onSuccess, CancellationToken cancellationToken)
    {
        Result result = await task.WithCancellation(cancellationToken);
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
    public static async Task<Result> ThenDo(this Task<Result> task, Action action, CancellationToken cancellationToken)
    {
        Result result = await task.WithCancellation(cancellationToken);
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
    public static async Task<Result> ThenAsync(this Task<Result> task, Func<Task<Result>> onSuccess, CancellationToken cancellationToken)
    {
        Result result = await task.WithCancellation(cancellationToken);

        return await result.ThenAsync(onSuccess, cancellationToken);
    }

    /// <summary>
    /// Awaits the result of a task, then asynchronously executes the specified action if the result represents success.
    /// Returns the current result unchanged.
    /// </summary>
    /// <param name="task">The task that produces a <see cref="Result"/>.</param>
    /// <param name="action">The async action to execute if the result is successful.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The current <see cref="Result"/>.</returns>
    public static async Task<Result> ThenDoAsync(this Task<Result> task, Func<Task> action, CancellationToken cancellationToken)
    {
        Result result = await task.WithCancellation(cancellationToken);

        return await result.ThenDoAsync(action, cancellationToken);
    }
}
