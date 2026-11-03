using System.Globalization;

using BenchmarkDotNet.Attributes;

using Microsoft.AspNetCore.Builder;

namespace CSharpEssentials.Endpoints.Benchmarks;

/// <summary>
/// The cost of one <c>MapGroup(string.Empty)</c> per endpoint at N=500, the wrapper CSE adds around every endpoint type.
/// Both rows map the same loop-built routes and handler instances from <see cref="LoopEndpoints"/>, build the app (see
/// <see cref="MappingHost"/>) and return the endpoint count, so the group is the only difference. The whole CSE mapping
/// cost at N=500 is in <see cref="StartupBenchmarks"/> (Plain against CseGenerated); this class isolates one part of it.
/// </summary>
[MemoryDiagnoser]
public class EmptyPrefixGroupBenchmarks
{
    [GlobalSetup]
    public async Task Setup()
    {
        await VerifyAsync(nameof(Direct), LoopEndpoints.MapDirect, expectedDataSources: 1).ConfigureAwait(false);
        await VerifyAsync(nameof(EmptyGroup), LoopEndpoints.MapEmptyGroup, expectedDataSources: LoopEndpoints.Count).ConfigureAwait(false);
    }

    // https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/route-handlers
    [Benchmark(Baseline = true)]
    public Task<int> Direct() => RunAsync(LoopEndpoints.MapDirect);

    // https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/route-handlers#route-groups
    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Endpoints/Readme.MD#mapping
    [Benchmark]
    public Task<int> EmptyGroup() => RunAsync(LoopEndpoints.MapEmptyGroup);

    private static async Task<int> RunAsync(Action<WebApplication> map) =>
        (await MappingHost.BuildAsync(map).ConfigureAwait(false)).Endpoints.Count;

    private static async Task VerifyAsync(string row, Action<WebApplication> map, int expectedDataSources)
    {
        (IReadOnlyList<Microsoft.AspNetCore.Http.Endpoint> endpoints, int dataSources) = await MappingHost.BuildAsync(map).ConfigureAwait(false);
        StartupSelfCheck.Verify(EndpointVariant.Plain, LoopEndpoints.Count, endpoints);
        if (dataSources != expectedDataSources)
            throw new InvalidOperationException($"{row} has {dataSources} endpoint data sources instead of {expectedDataSources}.");

        string last = (LoopEndpoints.Count - 1).ToString(CultureInfo.InvariantCulture);
        string path = "/items" + last + "/7";
        string expected = "{\"id\":7,\"name\":\"item" + last + "\"}";
        (int status, string body) = await MappingHost.GetAsync(map, path).ConfigureAwait(false);
        if (status != 200 || !string.Equals(body, expected, StringComparison.Ordinal))
            throw new InvalidOperationException($"{row}: GET {path} returned {status} '{body}' instead of 200 '{expected}'.");
    }
}
