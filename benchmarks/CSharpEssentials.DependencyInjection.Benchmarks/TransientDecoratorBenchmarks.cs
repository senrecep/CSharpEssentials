using BenchmarkDotNet.Attributes;
using CSharpEssentials.DependencyInjection.Benchmarks.Services.S50;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.DependencyInjection.Benchmarks;

/// <summary>
/// One resolve of a decorated transient <c>IService0</c> from the root provider, built once per variant in
/// <see cref="Setup"/> (see <see cref="TransientDecorationApp"/>). The services are transient and not disposable, so the
/// root provider does not track them, and every resolve activates the decorator and its inner service again.
/// <para>
/// Same services, decorators, lifetime and resolve in every row. Manual is a hand-written factory that calls both
/// constructors. CseGenerated calls the decorator constructor from generated code. CseRuntime builds the decorator with
/// <c>ConstructorInfo.Invoke</c> and a new argument array per resolve. Scrutor uses <c>ActivatorUtilities.CreateInstance</c>
/// per resolve. CSE and Scrutor resolve the inner service from a hidden keyed registration; Manual does not, which is
/// part of the gap.
/// </para>
/// <para>
/// <see cref="ResolveBenchmarks"/> resolves scoped decorated services in a new scope per operation. This class drops the
/// scope and uses transients, so the activation cost per resolve is what the rows show.
/// </para>
/// </summary>
[MemoryDiagnoser]
public class TransientDecoratorBenchmarks
{
    private ServiceProvider _manual = null!;
    private ServiceProvider _cseGenerated = null!;
    private ServiceProvider _cseRuntime = null!;
    private ServiceProvider _scrutor = null!;

    [GlobalSetup]
    public void Setup()
    {
        _manual = DiExtSelfCheck.BuildTransientDecorated(DecorationVariant.Manual);
        _cseGenerated = DiExtSelfCheck.BuildTransientDecorated(DecorationVariant.CseGenerated);
        _cseRuntime = DiExtSelfCheck.BuildTransientDecorated(DecorationVariant.CseRuntime);
        _scrutor = DiExtSelfCheck.BuildTransientDecorated(DecorationVariant.Scrutor);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _manual.Dispose();
        _cseGenerated.Dispose();
        _cseRuntime.Dispose();
        _scrutor.Dispose();
    }

    // https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.extensions.servicecollectiondescriptorextensions.replace
    [Benchmark(Baseline = true)]
    public int Manual() => _manual.GetRequiredService<IService0>().Value();

    [Benchmark]
    public int CseGenerated() => _cseGenerated.GetRequiredService<IService0>().Value();

    [Benchmark]
    public int CseRuntime() => _cseRuntime.GetRequiredService<IService0>().Value();

    // https://github.com/khellang/Scrutor/blob/v7.0.0/README.md#decoration
    [Benchmark]
    public int Scrutor() => _scrutor.GetRequiredService<IService0>().Value();
}
