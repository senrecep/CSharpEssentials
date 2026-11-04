using System.Collections.Immutable;
using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Microsoft.CodeAnalysis;

namespace CSharpEssentials.Generators.Benchmarks;

/// <summary>
/// Endpoint discovery for 50 endpoints in a web app of <see cref="Files"/> files. Same project, each library's documented
/// idiom: CSE reads types that implement IEndpoint, Immediate.Apis reads [Handler] plus [MapGet] types. Routes and results
/// are the same. Cold runs a fresh compilation and driver; Incremental runs a warm driver after a method-body edit in a
/// filler file.
/// <para>
/// Semantic differences: ImmediateApis is the head-to-head row (endpoint discovery and mapping emit). ImmediateApisWithHandlers
/// is the documented setup, which also emits a handler class and DI registration per endpoint. ImmediateApis alone does not
/// compile without the Handlers output, so its output is not compile-checked. EmptyDriver_Cold is compilation and driver
/// object overhead, not a floor for generator work.
/// </para>
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class EndpointsGeneratorComparison
{
    public const int Endpoints = 50;

    private const string Cold = "Cold";
    private const string Incremental = "Incremental";

    private static readonly Regex s_names = new(@"\bEndpoint\d{3}\b", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    private GeneratorRow _empty = null!;
    private GeneratorRow _cse = null!;
    private GeneratorRow _immediateApis = null!;
    private GeneratorRow _immediateApisWithHandlers = null!;

    [Params(200, 1000)]
    public int Files { get; set; } = 200;

    [GlobalSetup]
    public void Setup()
    {
        var cseShape = ProjectShape.Create(Files, Feature(CseEndpoint));
        var immediateShape = ProjectShape.Create(Files, Feature(ImmediateEndpoint));
        _empty = new GeneratorRow("EmptyDriver", false, [], cseShape);
        _cse = new GeneratorRow("CSharpEssentials", true, GeneratorSet.CseEndpoints, cseShape);
        _immediateApis = new GeneratorRow("ImmediateApis", false, GeneratorSet.ImmediateApis, immediateShape);
        _immediateApisWithHandlers = new GeneratorRow("ImmediateApisWithHandlers", false, GeneratorSet.ImmediateApisWithHandlers, immediateShape);

        string[] expected = [.. Enumerable.Range(1, Endpoints).Select(static n => $"Endpoint{n:000}")];
        GeneratorParity.Check(nameof(EndpointsGeneratorComparison), Files, _cse, s_names, expected, checkErrors: true);
        GeneratorParity.Check(nameof(EndpointsGeneratorComparison), Files, _immediateApis, s_names, expected, checkErrors: false);
        GeneratorParity.Check(nameof(EndpointsGeneratorComparison), Files, _immediateApisWithHandlers, s_names, expected, checkErrors: true);
    }

    public IEnumerable<GeneratorRow> Rows() => [_cse, _immediateApis, _immediateApisWithHandlers];

    // https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.cookbook.md
    [Benchmark, BenchmarkCategory(Cold)]
    public GeneratorDriver EmptyDriver_Cold() => _empty.Cold();

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Endpoints/Readme.MD#endpoints-and-groups
    [Benchmark(Baseline = true), BenchmarkCategory(Cold)]
    public GeneratorDriver CSharpEssentials_Cold() => _cse.Cold();

    // https://github.com/ImmediatePlatform/Immediate.Apis#creating-an-endpoint
    [Benchmark, BenchmarkCategory(Cold)]
    public GeneratorDriver ImmediateApis_Cold() => _immediateApis.Cold();

    // https://github.com/ImmediatePlatform/Immediate.Apis#creating-an-endpoint
    // https://github.com/ImmediatePlatform/Immediate.Handlers#creating-handlers
    [Benchmark, BenchmarkCategory(Cold)]
    public GeneratorDriver ImmediateApisWithHandlers_Cold() => _immediateApisWithHandlers.Cold();

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Endpoints/Readme.MD#endpoints-and-groups
    [Benchmark(Baseline = true), BenchmarkCategory(Incremental)]
    public GeneratorDriver CSharpEssentials_Incremental() => _cse.Incremental();

    // https://github.com/ImmediatePlatform/Immediate.Apis#creating-an-endpoint
    [Benchmark, BenchmarkCategory(Incremental)]
    public GeneratorDriver ImmediateApis_Incremental() => _immediateApis.Incremental();

    // https://github.com/ImmediatePlatform/Immediate.Apis#creating-an-endpoint
    // https://github.com/ImmediatePlatform/Immediate.Handlers#creating-handlers
    [Benchmark, BenchmarkCategory(Incremental)]
    public GeneratorDriver ImmediateApisWithHandlers_Incremental() => _immediateApisWithHandlers.Incremental();

    private static ImmutableArray<(string Name, string Text)> Feature(Func<string, string> source) =>
        [.. Enumerable.Range(1, Endpoints).Select(n => ($"Endpoint{n:000}", source($"{n:000}")))];

    private static string CseEndpoint(string n) => $$"""
        using CSharpEssentials.Endpoints;

        namespace Bench.App.Endpoints;

        public sealed class Endpoint{{n}} : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/endpoint{{n}}/{id:int}", static (int id) => TypedResults.Ok(id));
        }
        """;

    private static string ImmediateEndpoint(string n) => $$"""
        using Immediate.Apis.Shared;
        using Immediate.Handlers.Shared;

        namespace Bench.App.Endpoints;

        [Handler]
        [MapGet("/endpoint{{n}}/{id:int}")]
        public static partial class Endpoint{{n}}
        {
            public sealed record Query(int Id);

            private static ValueTask<int> HandleAsync(Query query, CancellationToken token) => ValueTask.FromResult(query.Id);
        }
        """;
}
