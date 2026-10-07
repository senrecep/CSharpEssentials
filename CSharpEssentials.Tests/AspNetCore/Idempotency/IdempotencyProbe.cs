namespace CSharpEssentials.Tests.AspNetCore.Idempotency;

/// <summary>
/// Counts handler executions and lets a test hold <c>/slow</c> open.
/// </summary>
internal sealed class IdempotencyProbe
{
    private int _executions;

    public int Executions => Volatile.Read(ref _executions);

    public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public TaskCompletionSource Gate { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public int Execute() => Interlocked.Increment(ref _executions);
}
