namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>Which keys the workers of <see cref="LockContentionBenchmarks"/> lock.</summary>
public enum LockKeys
{
    /// <summary>Every worker locks <c>"k"</c> and increments one shared counter: full contention.</summary>
    OneKey,

    /// <summary>Worker w locks <c>"k{w}"</c> and counts in its own local: parallel, no contention.</summary>
    PerWorker,

    /// <summary>Worker w locks <c>"k{w}-{i}"</c> on its i-th operation: every acquire creates and drops a key entry.</summary>
    Unique,
}
