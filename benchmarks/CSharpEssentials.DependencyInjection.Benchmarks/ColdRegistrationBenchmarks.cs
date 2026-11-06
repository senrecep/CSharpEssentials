using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;

namespace CSharpEssentials.DependencyInjection.Benchmarks;

/// <summary>
/// Time from a fresh process to every service having been resolved once: new collection, registration,
/// <c>BuildServiceProvider</c>, one scope, one resolve per service, dispose (see <see cref="DiApp.RegisterBuildResolve"/>,
/// which throws unless every service resolves to its own implementation). Each sample is the first and only invocation
/// in a new process, so JIT, assembly loading, reflection scanning and the container's first-resolve work are all inside
/// the measurement. There is no setup step that could warm a variant first.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RunStrategy.ColdStart, launchCount: 15, warmupCount: 0, iterationCount: 1, id: "cold")]
public class ColdRegistrationBenchmarks
{
    [Params(50, 500)]
    public int N { get; set; }

    // https://learn.microsoft.com/dotnet/core/extensions/dependency-injection/service-registration
    [Benchmark(Baseline = true)]
    public int Manual() => DiApp.RegisterBuildResolve(RegistrationVariant.Manual, N);

    [Benchmark]
    public int CseGenerated() => DiApp.RegisterBuildResolve(RegistrationVariant.CseGenerated, N);

    [Benchmark]
    public int CseReflection() => DiApp.RegisterBuildResolve(RegistrationVariant.CseReflection, N);

    // https://github.com/khellang/Scrutor/blob/v7.0.0/README.md#scanning
    // https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/IServiceTypeSelector.cs#L55-L58
    [Benchmark]
    public int Scrutor() => DiApp.RegisterBuildResolve(RegistrationVariant.Scrutor, N);
}
