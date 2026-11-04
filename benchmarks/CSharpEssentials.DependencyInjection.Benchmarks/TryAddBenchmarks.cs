using BenchmarkDotNet.Attributes;

namespace CSharpEssentials.DependencyInjection.Benchmarks;

/// <summary>
/// Registering N scoped services with TryAdd semantics into a new <c>ServiceCollection</c>, warm and in-process (see
/// <see cref="TryAddApp.Register"/>). ManualTryAdd (Microsoft <c>TryAddScoped</c> per service) is the baseline.
/// CseGenerated runs the generated registry of a T{N} library, whose classes use
/// <c>[RegisterScoped(Strategy = RegistrationStrategy.TryAdd)]</c>; CseReflection scans the same assembly; Scrutor scans
/// it with <c>RegistrationStrategy.Skip</c>. ManualAdd is a reference row only: it has no duplicate check.
/// <para>
/// Every TryAdd row scans the collection once per added descriptor, so on an empty collection all four are O(N^2):
/// Microsoft <c>TryAdd</c> loops the collection, Scrutor <c>Skip</c> calls Microsoft <c>TryAdd</c>, and CSE loops it in
/// <c>ServiceRegistration.FindFirst</c>. N = 2000 shows that growth.
/// </para>
/// <para>
/// Warm is not like-for-like for the reflection rows: after the first scan the runtime has already loaded every type
/// and filled its reflection caches, so CseReflection and Scrutor look cheaper here than in a real application, which
/// registers once per process. The self-check in <see cref="Setup"/> checks every variant's descriptors and resolves,
/// and checks that each TryAdd variant keeps a registration that already exists.
/// </para>
/// </summary>
[MemoryDiagnoser]
public class TryAddBenchmarks
{
    [Params(50, 500, 2000)]
    public int N { get; set; }

    [GlobalSetup]
    public void Setup() => DiExtSelfCheck.VerifyTryAdd(N);

    // https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.extensions.servicecollectiondescriptorextensions.tryaddscoped
    [Benchmark(Baseline = true)]
    public int ManualTryAdd() => TryAddApp.Register(TryAddVariant.ManualTryAdd, N).Count;

    [Benchmark]
    public int CseGenerated() => TryAddApp.Register(TryAddVariant.CseGenerated, N).Count;

    [Benchmark]
    public int CseReflection() => TryAddApp.Register(TryAddVariant.CseReflection, N).Count;

    // https://github.com/khellang/Scrutor/blob/v7.0.0/README.md#scanning
    // https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/IServiceTypeSelector.cs#L79
    // https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/RegistrationStrategy.cs#L47-L50
    [Benchmark]
    public int Scrutor() => TryAddApp.Register(TryAddVariant.Scrutor, N).Count;

    // https://learn.microsoft.com/dotnet/core/extensions/dependency-injection/service-registration
    [Benchmark]
    public int ManualAdd() => TryAddApp.Register(TryAddVariant.ManualAdd, N).Count;
}
