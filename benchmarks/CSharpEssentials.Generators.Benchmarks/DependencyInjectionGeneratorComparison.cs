using System.Collections.Immutable;
using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Microsoft.CodeAnalysis;

namespace CSharpEssentials.Generators.Benchmarks;

/// <summary>
/// Service registration for 100 services in a web app of <see cref="Files"/> files. Both libraries use the generic
/// explicit-service attribute form, so each emits one descriptor per service (IServiceNNN to ServiceNNN) with the same
/// lifetime (n % 3: Scoped, Singleton, Transient). Cold runs a fresh compilation and driver; Incremental runs a warm driver
/// after a method-body edit in a filler file.
/// <para>
/// Semantic differences: CSE emits a per-assembly marker with an idempotent Add{Assembly}Services method and, in an
/// executable, an AddAllServices aggregate. Injectio emits post-initialization attribute sources, an Add{Assembly} method
/// with a tags parameter and, because the shared framework references Microsoft.Extensions.Hosting.Abstractions, an
/// IHostApplicationBuilder overload. EmptyDriver_Cold is compilation and driver object overhead, not a floor for generator work.
/// </para>
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class DependencyInjectionGeneratorComparison
{
    public const int Services = 100;

    private const string Cold = "Cold";
    private const string Incremental = "Incremental";

    private static readonly Regex s_names = new(@"\bService\d{3}\b", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    private static readonly Regex s_lifetimes = new(
        @"ServiceDescriptor\.(?<lifetime>Scoped|Singleton|Transient)(?:<|\(typeof\()global::Bench\.App\.Services\.IService(?<id>\d{3})",
        RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1));

    private GeneratorRow _empty = null!;
    private GeneratorRow _cse = null!;
    private GeneratorRow _injectio = null!;

    [Params(200, 1000)]
    public int Files { get; set; } = 200;

    [GlobalSetup]
    public void Setup()
    {
        var cseShape = ProjectShape.Create(Files, Feature("CSharpEssentials.DependencyInjection"));
        var injectioShape = ProjectShape.Create(Files, Feature("Injectio.Attributes"));
        _empty = new GeneratorRow("EmptyDriver", false, [], cseShape);
        _cse = new GeneratorRow("CSharpEssentials", true, GeneratorSet.CseDependencyInjection, cseShape);
        _injectio = new GeneratorRow("Injectio", false, GeneratorSet.Injectio, injectioShape);

        string[] expected = [.. Enumerable.Range(1, Services).Select(static n => $"Service{n:000}")];
        string[] lifetimes = [.. Enumerable.Range(1, Services).Select(static n => $"{n:000}={Lifetime(n)}")];
        foreach (GeneratorRow row in Rows())
        {
            GeneratorDriverRunResult result = GeneratorParity.Check(nameof(DependencyInjectionGeneratorComparison), Files, row, s_names, expected, checkErrors: true);
            GeneratorParity.SameNames(
                $"{nameof(DependencyInjectionGeneratorComparison)}.{row.Name} lifetimes (Files={Files})",
                lifetimes,
                GeneratorParity.NameSet(result, s_lifetimes, static m => $"{m.Groups["id"].Value}={m.Groups["lifetime"].Value}"));
        }
    }

    public IEnumerable<GeneratorRow> Rows() => [_cse, _injectio];

    // https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.cookbook.md
    [Benchmark, BenchmarkCategory(Cold)]
    public GeneratorDriver EmptyDriver_Cold() => _empty.Cold();

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.DependencyInjection/Readme.MD#registering-services
    [Benchmark(Baseline = true), BenchmarkCategory(Cold)]
    public GeneratorDriver CSharpEssentials_Cold() => _cse.Cold();

    // https://github.com/loresoft/Injectio#generic-attributes
    [Benchmark, BenchmarkCategory(Cold)]
    public GeneratorDriver Injectio_Cold() => _injectio.Cold();

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.DependencyInjection/Readme.MD#registering-services
    [Benchmark(Baseline = true), BenchmarkCategory(Incremental)]
    public GeneratorDriver CSharpEssentials_Incremental() => _cse.Incremental();

    // https://github.com/loresoft/Injectio#generic-attributes
    [Benchmark, BenchmarkCategory(Incremental)]
    public GeneratorDriver Injectio_Incremental() => _injectio.Incremental();

    public static string Lifetime(int n) => (n % 3) switch
    {
        0 => "Scoped",
        1 => "Singleton",
        _ => "Transient",
    };

    private static ImmutableArray<(string Name, string Text)> Feature(string attributeNamespace) =>
        [.. Enumerable.Range(1, Services).Select(n => ($"Service{n:000}", Service(attributeNamespace, $"{n:000}", n)))];

    private static string Service(string attributeNamespace, string id, int n) => $$"""
        using {{attributeNamespace}};

        namespace Bench.App.Services;

        public interface IService{{id}}
        {
            int Get();
        }

        [Register{{Lifetime(n)}}<IService{{id}}>]
        public sealed class Service{{id}} : IService{{id}}
        {
            public int Get() => {{n}};
        }
        """;
}
