using CSharpEssentials.Core;
using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result
{
    /// <summary>
    /// Switches on the result, executing a corresponding action for success or failure.
    /// </summary>
    /// <param name="onSuccess">An action to execute on success.</param>
    /// <param name="onFailure">An action to execute on failure, providing an array of errors.</param>
    public void Switch(Action onSuccess, Action<Error[]> onFailure)
    {
        if (IsFailure)
        {
            onFailure(Errors);
            return;
        }
        onSuccess();
    }

    /// <summary>
    /// Asynchronously switches on the result, executing a corresponding async action for success or failure.
    /// </summary>
    /// <param name="onSuccess">An async action to execute on success.</param>
    /// <param name="onFailure">An async action to execute on failure, providing an array of errors.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SwitchAsync(Func<Task> onSuccess, Func<Error[], Task> onFailure, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
        {
            await onFailure(Errors).WithCancellation(cancellationToken);
            return;
        }
        await onSuccess().WithCancellation(cancellationToken);
    }

    /// <summary>
    /// Switches on the result, executing a corresponding action for success or the first encountered error.
    /// </summary>
    /// <param name="onSuccess">An action to execute on success.</param>
    /// <param name="onFirstError">An action to execute on the first error encountered.</param>
    public void SwitchFirst(Action onSuccess, Action<Error> onFirstError)
    {
        if (IsFailure)
        {
            onFirstError(FirstError);
            return;
        }

        onSuccess();
    }

    /// <summary>
    /// Asynchronously switches on the result, executing a corresponding async action for success or the first encountered error.
    /// </summary>
    /// <param name="onSuccess">An async action to execute on success.</param>
    /// <param name="onFirstError">An async action to execute on the first error encountered.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SwitchFirstAsync(Func<Task> onSuccess, Func<Error, Task> onFirstError, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
        {
            await onFirstError(FirstError).WithCancellation(cancellationToken);
            return;
        }

        await onSuccess().WithCancellation(cancellationToken);
    }

    /// <summary>
    /// Switches on the result, executing a corresponding action for success or the last encountered error.
    /// </summary>
    /// <param name="onSuccess">An action to execute on success.</param>
    /// <param name="onLastError">An action to execute on the last error encountered.</param>
    public void SwitchLast(Action onSuccess, Action<Error> onLastError)
    {
        if (IsFailure)
        {
            onLastError(LastError);
            return;
        }

        onSuccess();
    }

    /// <summary>
    /// Asynchronously switches on the result, executing a corresponding async action for success or the last encountered error.
    /// </summary>
    /// <param name="onSuccess">An async action to execute on success.</param>
    /// <param name="onLastError">An async action to execute on the last error encountered.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SwitchLastAsync(Func<Task> onSuccess, Func<Error, Task> onLastError, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
        {
            await onLastError(LastError).WithCancellation(cancellationToken);
            return;
        }

        await onSuccess().WithCancellation(cancellationToken);
    }
}

public static partial class ResultExtensions
{
    /// <summary>
    /// Executes the appropriate action based on whether the task's result is successful or contains errors.
    /// </summary>
    public static async Task Switch(this Task<Result> task, Action onSuccess, Action<Error[]> onFailure, CancellationToken cancellationToken)
    {
        Result result = await task.WithCancellation(cancellationToken);
        result.Switch(onSuccess, onFailure);
    }

    /// <summary>
    /// Asynchronously executes the appropriate action based on whether the task's result is successful or contains errors.
    /// </summary>
    public static async Task SwitchAsync(this Task<Result> task, Func<Task> onSuccess, Func<Error[], Task> onFailure, CancellationToken cancellationToken)
    {
        Result result = await task.WithCancellation(cancellationToken);
        await result.SwitchAsync(onSuccess, onFailure, cancellationToken);
    }

    /// <summary>
    /// Executes the appropriate action based on whether the task's result is successful or contains the first error.
    /// </summary>
    public static async Task SwitchFirst(this Task<Result> task, Action onSuccess, Action<Error> onFailure, CancellationToken cancellationToken)
    {
        Result result = await task.WithCancellation(cancellationToken);
        result.SwitchFirst(onSuccess, onFailure);
    }

    /// <summary>
    /// Asynchronously executes the appropriate action based on whether the task's result is successful or contains the first error.
    /// </summary>
    public static async Task SwitchFirstAsync(this Task<Result> task, Func<Task> onSuccess, Func<Error, Task> onFailure, CancellationToken cancellationToken)
    {
        Result result = await task.WithCancellation(cancellationToken);
        await result.SwitchFirstAsync(onSuccess, onFailure, cancellationToken);
    }

    /// <summary>
    /// Executes the appropriate action based on whether the task's result is successful or contains the last error.
    /// </summary>
    public static async Task SwitchLast(this Task<Result> task, Action onSuccess, Action<Error> onFailure, CancellationToken cancellationToken)
    {
        Result result = await task.WithCancellation(cancellationToken);
        result.SwitchLast(onSuccess, onFailure);
    }

    /// <summary>
    /// Asynchronously executes the appropriate action based on whether the task's result is successful or contains the last error.
    /// </summary>
    public static async Task SwitchLastAsync(this Task<Result> task, Func<Task> onSuccess, Func<Error, Task> onFailure, CancellationToken cancellationToken)
    {
        Result result = await task.WithCancellation(cancellationToken);
        await result.SwitchLastAsync(onSuccess, onFailure, cancellationToken);
    }
}
