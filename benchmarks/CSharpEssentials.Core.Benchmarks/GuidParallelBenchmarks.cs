using BenchmarkDotNet.Attributes;
using CSharpEssentials.Core;

namespace CSharpEssentials.Core.Benchmarks;

/// <summary>
/// 6 threads that each create 20,000 GUIDs; the reported time is per GUID. Shows the cost of the shared value that
/// <see cref="Guider.NewMonotonicGuid"/> advances with a compare-and-swap.
/// </summary>
[MemoryDiagnoser]
public class GuidParallelBenchmarks
{
    private const int _threads = 6, _perThread = 20_000, _total = _threads * _perThread;

    [Benchmark(Baseline = true, OperationsPerInvoke = _total)]
    public int GuidNewGuid() => Run(Guid.NewGuid);

#if NET9_0_OR_GREATER
    [Benchmark(OperationsPerInvoke = _total)]
    public int GuidCreateVersion7() => Run(Guid.CreateVersion7);
#endif

    [Benchmark(OperationsPerInvoke = _total)]
    public int GuiderNewGuid() => Run(Guider.NewGuid);

    [Benchmark(OperationsPerInvoke = _total)]
    public int GuiderNewMonotonicGuid() => Run(Guider.NewMonotonicGuid);

    private static int Run(Func<Guid> create)
    {
        int[] sinks = new int[_threads];
        Parallel.For(0, _threads, new ParallelOptions { MaxDegreeOfParallelism = _threads }, t =>
        {
            int sink = 0;
            for (int i = 0; i < _perThread; i++)
                sink ^= create().GetHashCode();
            sinks[t] = sink;
        });
        return sinks.Aggregate(0, (a, b) => a ^ b);
    }
}
