using System.Collections.Concurrent;

namespace CSharpEssentials.Tests.AspNetCore.Idempotency;

/// <summary>
/// Counts handler executions, reports when tracked requests finish, and lets a test hold <c>/slow</c> open.
/// </summary>
internal sealed class IdempotencyProbe
{
    public const string TraceHeader = "X-Trace";

    private readonly ConcurrentDictionary<string, TaskCompletionSource> _finished = new();
    private int _executions;

    public int Executions => Volatile.Read(ref _executions);

    public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public TaskCompletionSource Gate { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public int Execute() => Interlocked.Increment(ref _executions);

    /// <summary>Called by the outermost middleware when a request has left the pipeline.</summary>
    public void Finish(string trace) => Track(trace).TrySetResult();

    /// <summary>Completes when the server has finished the request with this trace, also when its client saw an abort.</summary>
    public Task Finished(string trace) => Track(trace).Task.WaitAsync(TimeSpan.FromSeconds(10));

    private TaskCompletionSource Track(string trace) =>
        _finished.GetOrAdd(trace, static _ => new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously));
}
