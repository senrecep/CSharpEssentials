using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;

namespace CSharpEssentials.Endpoints.Benchmarks;

/// <summary>
/// Time from a fresh process to every endpoint having answered once: builder, services, Build, mapping, TestServer start,
/// one <c>GET</c> per endpoint, stop, dispose (see <see cref="StartupApp.ServeAllAsync"/>). Each sample is the first and
/// only invocation in a new process, so JIT, assembly scanning, RequestDelegateFactory and every library's lazy
/// first-request work are all inside the measurement. There is no setup step that could warm a variant first.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RunStrategy.ColdStart, launchCount: 15, warmupCount: 0, iterationCount: 1, id: "cold")]
public class ColdStartupBenchmarks
{
    [Params(10, 100, 500)]
    public int N { get; set; }

    [Benchmark(Baseline = true)]
    public Task<int> Plain() => StartupApp.ServeAllAsync(EndpointVariant.Plain, N);

    [Benchmark]
    public Task<int> CseGenerated() => StartupApp.ServeAllAsync(EndpointVariant.CseGenerated, N);

    [Benchmark]
    public Task<int> CseReflection() => StartupApp.ServeAllAsync(EndpointVariant.CseReflection, N);

#if NET10_0_OR_GREATER
    // https://github.com/CarterCommunity/Carter/blob/10.0.0/README.md
    [Benchmark]
    public Task<int> Carter() => StartupApp.ServeAllAsync(EndpointVariant.Carter, N);
#endif

    // https://fast-endpoints.com/docs/get-started
    [Benchmark]
    public Task<int> FastEndpoints() => StartupApp.ServeAllAsync(EndpointVariant.FastEndpoints, N);

    // https://fast-endpoints.com/docs/configuration-settings#source-generator-based-startup
    [Benchmark]
    public Task<int> FastEndpointsSourceGen() => StartupApp.ServeAllAsync(EndpointVariant.FastEndpointsSourceGen, N);
}
