namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>Which idempotency path an <see cref="IdempotencyBehaviorBenchmarks"/> row measures.</summary>
public enum IdempotencyPath
{
    /// <summary>Key <c>"replay-1"</c>, Quantity 5, sent once in setup: every measured send replays the stored success.</summary>
    Replay,

    /// <summary>Key <c>"execute-1"</c>, Quantity 0: every send reserves the key, runs the failing handler and releases the key.</summary>
    ExecuteRelease,
}
