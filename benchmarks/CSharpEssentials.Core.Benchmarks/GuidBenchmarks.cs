using BenchmarkDotNet.Attributes;
using CSharpEssentials.Core;

namespace CSharpEssentials.Core.Benchmarks;

/// <summary>
/// One GUID per call on one thread. On net8.0 <see cref="Guider"/> runs its netstandard2.1 asset; on every target
/// <see cref="Guider.NewGuid"/> builds the version 7 layout from a per-thread pool of random bytes.
/// </summary>
[MemoryDiagnoser]
public class GuidBenchmarks
{
    [Benchmark(Baseline = true)]
    public Guid GuidNewGuid() => Guid.NewGuid();

#if NET9_0_OR_GREATER
    [Benchmark]
    public Guid GuidCreateVersion7() => Guid.CreateVersion7();
#endif

    [Benchmark]
    public Guid GuiderNewGuid() => Guider.NewGuid();

    [Benchmark]
    public Guid GuiderNewMonotonicGuid() => Guider.NewMonotonicGuid();
}
