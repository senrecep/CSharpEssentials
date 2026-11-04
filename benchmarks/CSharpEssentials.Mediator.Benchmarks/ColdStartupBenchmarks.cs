using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;

namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>
/// Time from a fresh process to the first response: new collection, registration of 50 generated request handlers,
/// <c>BuildServiceProvider</c>, first <c>Send</c>, dispose (see <see cref="StartupBenchmarks"/> for what each library does).
/// Each timed sample is the first invocation in a new process, so JIT, assembly loading, MediatR's reflection scan and
/// its static wrapper cache, and Mediator's first-send pipeline build are all inside the time. The Allocated column comes
/// from BenchmarkDotNet's extra-stats call, which runs the method a second time in the same process, so it shows warm
/// allocations (MediatR's static wrapper cache is already filled). There is no setup step that could warm a variant first;
/// <see cref="StartupBenchmarks"/> runs the self-check.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RunStrategy.ColdStart, launchCount: 15, warmupCount: 0, iterationCount: 1, id: "cold")]
public class ColdStartupBenchmarks
{
    // https://github.com/jbogard/MediatR/blob/v12.5.0/README.md#registering-with-iservicecollection
    // https://github.com/jbogard/MediatR/blob/v12.5.0/src/MediatR/Registration/ServiceRegistrar.cs
    [Benchmark(Baseline = true)]
    public int MediatR() => MediatRApp.StartupFirstSend();

    // https://github.com/martinothamar/Mediator/blob/v3.0.2/README.md#42-add-mediator-to-di-container
    [Benchmark]
    public int Mediator() => MediatorApp.StartupFirstSend();
}
