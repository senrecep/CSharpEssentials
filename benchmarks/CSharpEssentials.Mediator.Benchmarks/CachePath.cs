namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>Which caching path a <see cref="CachingBehaviorBenchmarks"/> row measures.</summary>
public enum CachePath
{
    /// <summary>The entry is stored by one send in setup; every measured send reads it back.</summary>
    Hit,

    /// <summary>Every measured send misses, runs the handler and stores the entry; the row then removes it.</summary>
    Miss,
}
