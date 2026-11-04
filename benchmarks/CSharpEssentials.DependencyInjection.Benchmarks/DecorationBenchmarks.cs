using BenchmarkDotNet.Attributes;

namespace CSharpEssentials.DependencyInjection.Benchmarks;

/// <summary>
/// Registering 50 services by hand and decorating 10 of them, warm and in-process (see <see cref="DiApp.RegisterDecorated"/>).
/// Every variant starts from the same manual registration and uses the same decorator classes. The self-check in
/// <see cref="Setup"/> resolves every service from each variant and requires exactly the 10 decorated ones to be wrapped once.
/// </summary>
[MemoryDiagnoser]
public class DecorationBenchmarks
{
    [GlobalSetup]
    public void Setup() => DiSelfCheck.VerifyDecorations();

    // https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.extensions.servicecollectiondescriptorextensions.replace
    [Benchmark(Baseline = true)]
    public int Manual() => DiApp.RegisterDecorated(DecorationVariant.Manual).Count;

    [Benchmark]
    public int CseGenerated() => DiApp.RegisterDecorated(DecorationVariant.CseGenerated).Count;

    [Benchmark]
    public int CseRuntime() => DiApp.RegisterDecorated(DecorationVariant.CseRuntime).Count;

    // https://github.com/khellang/Scrutor/blob/v7.0.0/README.md#decoration
    [Benchmark]
    public int Scrutor() => DiApp.RegisterDecorated(DecorationVariant.Scrutor).Count;
}
