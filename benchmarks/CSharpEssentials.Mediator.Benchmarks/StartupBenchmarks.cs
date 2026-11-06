using BenchmarkDotNet.Attributes;

namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>
/// New collection, registration of 50 generated request handlers, <c>BuildServiceProvider</c>, first <c>Send</c>, dispose,
/// warm and in-process. MediatR scans its 50-handler library by reflection; Mediator calls the <c>AddMediator</c> its source
/// generator wrote for its 50-handler library, and builds every handler's pipeline on the first send.
/// <para>
/// Warm hides MediatR's first-send reflection: it keeps the generic wrapper it builds per request type
/// (<c>MakeGenericType</c> + <c>Activator.CreateInstance</c>) in a static, process-wide cache, so only the first iteration
/// in the process pays it. Assembly scanning also gets the runtime's warm reflection caches here.
/// <see cref="ColdStartupBenchmarks"/> is the fair comparison.
/// </para>
/// </summary>
[MemoryDiagnoser]
public class StartupBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        MediatorSelfCheck.VerifyVersions();
        MediatorSelfCheck.VerifyStartup();
    }

    // https://github.com/jbogard/MediatR/blob/v12.5.0/README.md#registering-with-iservicecollection
    // https://github.com/jbogard/MediatR/blob/v12.5.0/src/MediatR/Registration/ServiceRegistrar.cs
    [Benchmark(Baseline = true)]
    public int MediatR() => MediatRApp.StartupFirstSend();

    // https://github.com/martinothamar/Mediator/blob/v3.0.2/README.md#42-add-mediator-to-di-container
    [Benchmark]
    public int Mediator() => MediatorApp.StartupFirstSend();
}
