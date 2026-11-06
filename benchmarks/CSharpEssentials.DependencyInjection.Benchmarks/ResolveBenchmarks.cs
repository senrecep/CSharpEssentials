using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using CSharpEssentials.DependencyInjection.Benchmarks.Services.S50;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.DependencyInjection.Benchmarks;

/// <summary>
/// One scope plus one resolve from a provider built once per variant in <see cref="Setup"/> (N = 50).
/// <para>
/// Plain resolves IService15, which depends on IService14, from each library's own registration. The descriptors are
/// identical, so these rows should tie. Decorated resolves IService0 after decorating IService0..IService9 on top of the
/// manual registration. Each library leaves a different descriptor shape behind, which is what this category measures.
/// </para>
/// </summary>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class ResolveBenchmarks
{
    private const string Plain = "Plain";
    private const string Decorated = "Decorated";

    private readonly List<ServiceProvider> _providers = [];
    private IServiceProvider _plainManual = null!;
    private IServiceProvider _plainCseGenerated = null!;
    private IServiceProvider _plainScrutor = null!;
    private IServiceProvider _decoratedManual = null!;
    private IServiceProvider _decoratedCseGenerated = null!;
    private IServiceProvider _decoratedCseRuntime = null!;
    private IServiceProvider _decoratedScrutor = null!;

    [GlobalSetup]
    public void Setup()
    {
        _plainManual = Build(RegistrationVariant.Manual);
        _plainCseGenerated = Build(RegistrationVariant.CseGenerated);
        _plainScrutor = Build(RegistrationVariant.Scrutor);
        _decoratedManual = Build(DecorationVariant.Manual);
        _decoratedCseGenerated = Build(DecorationVariant.CseGenerated);
        _decoratedCseRuntime = Build(DecorationVariant.CseRuntime);
        _decoratedScrutor = Build(DecorationVariant.Scrutor);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        foreach (ServiceProvider provider in _providers)
        {
            provider.Dispose();
        }

        _providers.Clear();
    }

    // https://learn.microsoft.com/dotnet/core/extensions/dependency-injection/service-registration
    [Benchmark(Baseline = true)]
    [BenchmarkCategory(Plain)]
    public int PlainManual() => ResolvePlain(_plainManual);

    [Benchmark]
    [BenchmarkCategory(Plain)]
    public int PlainCseGenerated() => ResolvePlain(_plainCseGenerated);

    // https://github.com/khellang/Scrutor/blob/v7.0.0/README.md#scanning
    // https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/IServiceTypeSelector.cs#L55-L58
    [Benchmark]
    [BenchmarkCategory(Plain)]
    public int PlainScrutor() => ResolvePlain(_plainScrutor);

    // https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.extensions.servicecollectiondescriptorextensions.replace
    [Benchmark(Baseline = true)]
    [BenchmarkCategory(Decorated)]
    public int DecoratedManual() => ResolveDecorated(_decoratedManual);

    [Benchmark]
    [BenchmarkCategory(Decorated)]
    public int DecoratedCseGenerated() => ResolveDecorated(_decoratedCseGenerated);

    [Benchmark]
    [BenchmarkCategory(Decorated)]
    public int DecoratedCseRuntime() => ResolveDecorated(_decoratedCseRuntime);

    // https://github.com/khellang/Scrutor/blob/v7.0.0/README.md#decoration
    [Benchmark]
    [BenchmarkCategory(Decorated)]
    public int DecoratedScrutor() => ResolveDecorated(_decoratedScrutor);

    private static int ResolvePlain(IServiceProvider provider)
    {
        using IServiceScope scope = provider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<IService15>().Value();
    }

    private static int ResolveDecorated(IServiceProvider provider)
    {
        using IServiceScope scope = provider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<IService0>().Value();
    }

    private ServiceProvider Build(RegistrationVariant variant)
    {
        IServiceCollection services = DiApp.Register(variant, 50);
        DiSelfCheck.VerifyRegistration(variant, 50, services);
        return Track(services.BuildServiceProvider(), variant, decorated: 0);
    }

    private ServiceProvider Build(DecorationVariant variant) =>
        Track(DiApp.RegisterDecorated(variant).BuildServiceProvider(), variant, DiSelfCheck.DecoratedCount);

    private ServiceProvider Track<TVariant>(ServiceProvider provider, TVariant variant, int decorated)
        where TVariant : struct, Enum
    {
        _providers.Add(provider);
        using IServiceScope scope = provider.CreateScope();
        DiSelfCheck.VerifyValues(variant, 50, decorated, DiApp.Values(50, scope.ServiceProvider));
        return provider;
    }
}
