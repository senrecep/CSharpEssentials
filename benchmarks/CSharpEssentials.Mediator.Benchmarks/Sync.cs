namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>
/// Reads the result of a send or publish in place. Every handler completes synchronously, so the benchmark methods stay
/// synchronous for both libraries and no async state machine of the benchmark itself blurs the Task / ValueTask difference.
/// </summary>
public static class Sync
{
    /// <summary>Returns the result of a MediatR task.</summary>
    /// <typeparam name="T">The response type.</typeparam>
    /// <param name="task">The task.</param>
    /// <returns>The result.</returns>
    public static T Run<T>(Task<T> task) => task.GetAwaiter().GetResult();

    /// <summary>Waits for a MediatR task.</summary>
    /// <param name="task">The task.</param>
    public static void Run(Task task) => task.GetAwaiter().GetResult();

    /// <summary>Returns the result of a Mediator value task, through a task only if it has not completed.</summary>
    /// <typeparam name="T">The response type.</typeparam>
    /// <param name="task">The value task.</param>
    /// <returns>The result.</returns>
    public static T Run<T>(ValueTask<T> task)
    {
        if (task.IsCompletedSuccessfully)
        {
            return task.Result;
        }

        return task.AsTask().GetAwaiter().GetResult();
    }

    /// <summary>Waits for a Mediator value task, through a task only if it has not completed.</summary>
    /// <param name="task">The value task.</param>
    public static void Run(ValueTask task)
    {
        if (!task.IsCompletedSuccessfully)
        {
            task.AsTask().GetAwaiter().GetResult();
        }
    }
}
