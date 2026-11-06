using BenchmarkDotNet.Attributes;

namespace CSharpEssentials.Endpoints.Benchmarks;

/// <summary>
/// App startup for N endpoints per variant: builder, services, Build, mapping, endpoint enumeration, dispose (see
/// <see cref="StartupApp"/>). Baseline is the same unit with no endpoints. BenchmarkDotNet runs each case in its own
/// process, so the self-check in <see cref="Setup"/> and the warmup iterations leave every variant in the same warm,
/// process-wide state (loaded assemblies, JIT, FastEndpoints' static caches) before measuring. The self-check also
/// starts each variant on a TestServer once and requests its last route, so every variant answers the same body.
/// <para>
/// The FastEndpoints and FastEndpointsSourceGen rows leave per-endpoint binder and validator setup to the first request
/// (see <see cref="StartupApp"/>), so this warm mapping cost is not comparable on its own; <see cref="ColdStartupBenchmarks"/>
/// measures a fresh process until every endpoint has answered once. Outside BenchmarkDotNet, with N=500,
/// warming one route and then hitting the other 499 twice took about 140-155 ms on the first pass and 41-51 ms on the
/// second for FastEndpoints, against about 81-83 ms and 35-36 ms for Plain.
/// </para>
/// </summary>
[MemoryDiagnoser]
public class StartupBenchmarks
{
    [Params(10, 100, 500)]
    public int N { get; set; }

    [GlobalSetup]
    public Task Setup() => VerifyAsync(EndpointVariants.Mapping);

    [Benchmark(Baseline = true)]
    public Task<int> Baseline() => RunAsync(EndpointVariant.Baseline);

    [Benchmark]
    public Task<int> Plain() => RunAsync(EndpointVariant.Plain);

    [Benchmark]
    public Task<int> CseGenerated() => RunAsync(EndpointVariant.CseGenerated);

    [Benchmark]
    public Task<int> CseReflection() => RunAsync(EndpointVariant.CseReflection);

#if NET10_0_OR_GREATER
    // https://github.com/CarterCommunity/Carter/blob/10.0.0/README.md
    [Benchmark]
    public Task<int> Carter() => RunAsync(EndpointVariant.Carter);
#endif

    // https://fast-endpoints.com/docs/get-started
    [Benchmark]
    public Task<int> FastEndpoints() => RunAsync(EndpointVariant.FastEndpoints);

    // https://fast-endpoints.com/docs/configuration-settings#source-generator-based-startup
    [Benchmark]
    public Task<int> FastEndpointsSourceGen() => RunAsync(EndpointVariant.FastEndpointsSourceGen);

    private async Task VerifyAsync(IReadOnlyList<EndpointVariant> variants)
    {
        StartupSelfCheck.Verify(EndpointVariant.Baseline, N, await StartupApp.BuildAsync(EndpointVariant.Baseline, N).ConfigureAwait(false));
        foreach (EndpointVariant variant in variants)
        {
            StartupSelfCheck.Verify(variant, N, await StartupApp.BuildAsync(variant, N).ConfigureAwait(false));
            await StartupSelfCheck.VerifyResponseAsync(variant, N).ConfigureAwait(false);
        }
    }

    private async Task<int> RunAsync(EndpointVariant variant) =>
        (await StartupApp.BuildAsync(variant, N).ConfigureAwait(false)).Count;
}
