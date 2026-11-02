using BenchmarkDotNet.Attributes;

namespace CSharpEssentials.DependencyInjection.Benchmarks;

/// <summary>
/// Registering N scoped services into a new <c>ServiceCollection</c>, warm and in-process (see <see cref="DiApp.Register"/>).
/// Baseline is the empty collection. The self-check in <see cref="Setup"/> runs every variant once first.
/// <para>
/// Warm is not like-for-like: after the first scan the runtime has already loaded every type, filled its reflection
/// caches (interfaces, constructors) and JIT-compiled the scanning code, so the reflection rows (CseReflection, Scrutor)
/// look cheaper here than in a real application, which registers once per process.
/// <see cref="ColdRegistrationBenchmarks"/> is the fair comparison.
/// </para>
/// <para>
/// Scrutor's attribute mode (<c>[ServiceDescriptor]</c> + <c>UsingAttributes()</c>) is not measured: it needs Scrutor's
/// own attributes on the shared input classes, so all variants could no longer use identical inputs.
/// </para>
/// </summary>
[MemoryDiagnoser]
public class RegistrationBenchmarks
{
    [Params(50, 500)]
    public int N { get; set; }

    [GlobalSetup]
    public void Setup() => DiSelfCheck.VerifyRegistrations(N);

    [Benchmark(Baseline = true)]
    public int Baseline() => DiApp.Register(RegistrationVariant.Baseline, N).Count;

    // https://learn.microsoft.com/dotnet/core/extensions/dependency-injection/service-registration
    [Benchmark]
    public int Manual() => DiApp.Register(RegistrationVariant.Manual, N).Count;

    [Benchmark]
    public int CseGenerated() => DiApp.Register(RegistrationVariant.CseGenerated, N).Count;

    [Benchmark]
    public int CseReflection() => DiApp.Register(RegistrationVariant.CseReflection, N).Count;

    // https://github.com/khellang/Scrutor/blob/v7.0.0/README.md#scanning
    // https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/IServiceTypeSelector.cs#L55-L58
    [Benchmark]
    public int Scrutor() => DiApp.Register(RegistrationVariant.Scrutor, N).Count;
}
