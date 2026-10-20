using System.Net;
using System.Text;

using BenchmarkDotNet.Attributes;

namespace CSharpEssentials.Idempotency.Benchmarks;

/// <summary>
/// The cost of the idempotency middleware on the in-memory store for a small JSON body: a request on a route without
/// idempotency (baseline), a first request with a new key (reserve, execute, capture, complete), and a replay of a
/// completed key.
/// </summary>
[MemoryDiagnoser]
public class MiddlewareOverheadBenchmarks
{
    private static readonly byte[] Body = Encoding.UTF8.GetBytes("""{"orderId":"order-1","amount":25.00,"currency":"EUR"}""");

    private const string ReplayKey = "replay-key";

    private IdempotencyBenchmarkHost _host = null!;

    [GlobalSetup]
    public async Task Setup()
    {
        _host = await IdempotencyBenchmarkHost.StartAsync().ConfigureAwait(false);
        await _host.VerifyAsync(Body, ReplayKey).ConfigureAwait(false);
    }

    [GlobalCleanup]
    public async Task Cleanup() => await _host.DisposeAsync().ConfigureAwait(false);

    [Benchmark(Baseline = true)]
    public Task<HttpStatusCode> NoIdempotency() =>
        _host.SendAsync(IdempotencyBenchmarkHost.PlainPath, Body, null);

    [Benchmark]
    public Task<HttpStatusCode> NewKey() =>
        _host.SendAsync(IdempotencyBenchmarkHost.KeyedPath, Body, _host.NextKey());

    [Benchmark]
    public Task<HttpStatusCode> Replay() =>
        _host.SendAsync(IdempotencyBenchmarkHost.KeyedPath, Body, ReplayKey);
}
