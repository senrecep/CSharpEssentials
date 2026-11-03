namespace CSharpEssentials.AspNetCore.Comparison.Benchmarks;

/// <summary>Counts handler invocations of one host, so parity can tell an executed request from a replayed or short-circuited one.</summary>
public sealed class HandlerCounter
{
    private int _count;

    /// <summary>The number of handler invocations so far.</summary>
    public int Count => Volatile.Read(ref _count);

    /// <summary>Records one handler invocation.</summary>
    public void Increment() => Interlocked.Increment(ref _count);
}
