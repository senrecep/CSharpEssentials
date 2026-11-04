using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CSharpEssentials.Endpoints.Benchmarks;

/// <summary>
/// The cost of endpoint names (operationId) at N=500: build the app (see <see cref="MappingHost"/>), then read the name of
/// every endpoint. The read is part of the unit because <c>OperationNaming.TypeName</c> resolves its names on the first read.
/// Category Cse maps the generated <c>Cse500</c> library with each naming policy: <c>None</c> (default, no names),
/// <c>TypeName</c> and <c>Custom(type.Name)</c>, which gives the same 500 names as TypeName for these types.
/// Category HandWritten maps the loop-built endpoints of <see cref="LoopEndpoints"/> without names and with
/// <c>WithName(...)</c> per route, which is how a plain Minimal API app (and a Carter <c>ICarterModule</c> app) names
/// endpoints. The categories use different handler instances, so compare rows inside a category, not across.
/// <para>
/// FastEndpoints has no row: it always names its endpoints inside its own mapping and has no switch to turn naming off, so
/// a row would measure its whole startup, which <see cref="StartupBenchmarks"/> already covers. Carter has no naming policy.
/// </para>
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class OperationNamingBenchmarks
{
    private const string Cse = "Cse";
    private const string HandWritten = "HandWritten";

    private static readonly OperationNaming CustomNaming = OperationNaming.Custom(static (type, _) => type.Name);

    [GlobalSetup]
    public async Task Setup()
    {
        HashSet<string> expected = [.. LoopEndpoints.Names];

        await VerifyAsync(nameof(CseNone), MapCseNone, EndpointVariant.CseGenerated, null).ConfigureAwait(false);
        await VerifyAsync(nameof(CseTypeName), MapCseTypeName, EndpointVariant.CseGenerated, expected).ConfigureAwait(false);
        await VerifyAsync(nameof(CseCustom), MapCseCustom, EndpointVariant.CseGenerated, expected).ConfigureAwait(false);
        await VerifyAsync(nameof(Direct), LoopEndpoints.MapDirect, EndpointVariant.Plain, null).ConfigureAwait(false);
        await VerifyAsync(nameof(DirectWithName), LoopEndpoints.MapDirectWithName, EndpointVariant.Plain, expected).ConfigureAwait(false);
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Endpoints/Readme.MD#mapping-options
    [Benchmark(Baseline = true), BenchmarkCategory(Cse)]
    public Task<int> CseNone() => RunAsync(MapCseNone);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Endpoints/Readme.MD#mapping-options
    [Benchmark, BenchmarkCategory(Cse)]
    public Task<int> CseTypeName() => RunAsync(MapCseTypeName);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Endpoints/Readme.MD#mapping-options
    [Benchmark, BenchmarkCategory(Cse)]
    public Task<int> CseCustom() => RunAsync(MapCseCustom);

    // https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/route-handlers
    [Benchmark(Baseline = true), BenchmarkCategory(HandWritten)]
    public Task<int> Direct() => RunAsync(LoopEndpoints.MapDirect);

    // https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.builder.routingendpointconventionbuilderextensions.withname
    [Benchmark, BenchmarkCategory(HandWritten)]
    public Task<int> DirectWithName() => RunAsync(LoopEndpoints.MapDirectWithName);

    internal static void MapCseNone(WebApplication app) => app.MapCse500Endpoints(static o => o.OperationNaming = OperationNaming.None);

    internal static void MapCseTypeName(WebApplication app) => app.MapCse500Endpoints(static o => o.OperationNaming = OperationNaming.TypeName);

    internal static void MapCseCustom(WebApplication app) => app.MapCse500Endpoints(static o => o.OperationNaming = CustomNaming);

    private static async Task<int> RunAsync(Action<WebApplication> map) =>
        MappingHost.CountNames((await MappingHost.BuildAsync(map).ConfigureAwait(false)).Endpoints);

    private static async Task VerifyAsync(string row, Action<WebApplication> map, EndpointVariant variant, HashSet<string>? expectedNames)
    {
        IReadOnlyList<Endpoint> endpoints = (await MappingHost.BuildAsync(map).ConfigureAwait(false)).Endpoints;
        StartupSelfCheck.Verify(variant, LoopEndpoints.Count, endpoints);
        List<string> names = [.. endpoints.Select(static e => e.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName).OfType<string>()];
        bool ok = expectedNames is null ? names.Count == 0 : names.Count == expectedNames.Count && expectedNames.SetEquals(names);
        int counted = MappingHost.CountNames(endpoints);
        if (!ok || counted != names.Count || await RunAsync(map).ConfigureAwait(false) != names.Count)
        {
            throw new InvalidOperationException(
                $"{row} named {names.Count} endpoints instead of {expectedNames?.Count ?? 0}: " + string.Join(", ", names.Take(5)));
        }
    }
}
