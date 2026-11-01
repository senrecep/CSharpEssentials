using System.Net;

using BenchmarkDotNet.Attributes;

namespace CSharpEssentials.Endpoints.Benchmarks;

/// <summary>
/// One request through an in-process TestServer app per variant (see <see cref="ThroughputHost"/>). The POST rows run
/// on net10.0 and later only, because the Plain and CSE apps validate with the .NET 10 <c>AddValidation</c> engine.
/// Each variant's app is built alone in its benchmark process, so FastEndpoints' once-per-process configuration never
/// leaks between its two variants. Exceptions/op shows the error paths differ: the FastEndpoints invalid POST throws
/// two exceptions per request, the Plain, CSE and Carter invalid POST none.
/// </summary>
[MemoryDiagnoser]
[ExceptionDiagnoser]
public class ThroughputBenchmarks
{
    private ThroughputHost _host = null!;

    [ParamsSource(nameof(Variants))]
    public EndpointVariant Variant { get; set; }

    public static IEnumerable<EndpointVariant> Variants => EndpointVariants.Mapping;

    [GlobalSetup]
    public async Task Setup()
    {
        _host = await ThroughputHost.StartAsync(Variant).ConfigureAwait(false);
        await _host.VerifyAsync().ConfigureAwait(false);
    }

    [GlobalCleanup]
    public async Task Cleanup() => await _host.DisposeAsync().ConfigureAwait(false);

    [Benchmark]
    public Task<HttpStatusCode> GetById() => _host.GetByIdAsync();

#if NET10_0_OR_GREATER
    [Benchmark]
    public Task<HttpStatusCode> PostValid() => _host.PostValidAsync();

    [Benchmark]
    public Task<HttpStatusCode> PostInvalid() => _host.PostInvalidAsync();
#endif

    [Benchmark]
    public Task<HttpStatusCode> Delete() => _host.DeleteAsync();
}
