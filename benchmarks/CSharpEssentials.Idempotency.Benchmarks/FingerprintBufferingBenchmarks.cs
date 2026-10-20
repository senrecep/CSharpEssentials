using System.Net;
using System.Security.Cryptography;

using BenchmarkDotNet.Attributes;

namespace CSharpEssentials.Idempotency.Benchmarks;

/// <summary>
/// The cost of buffering and hashing the request body for the fingerprint, by body size. The fingerprint is computed
/// before the store lookup, so every keyed request pays the full buffering (which spills to a temporary file above the
/// request buffering threshold) and the hash.
/// <list type="bullet">
/// <item><description><see cref="KeyedReplay"/> replays a completed key while the store stays at one record. The plain
/// request reads the body in the handler and the replay never reaches the handler, so both read the body once and the
/// difference from <see cref="Plain"/> is the buffering, the hash, the lookup and the stored response.</description></item>
/// <item><description><see cref="KeyedNewKey"/> sends a new key, so the handler runs and reads the buffered body. The
/// difference from <see cref="Plain"/> is the buffering, the hash, the reservation, the response capture and the
/// completion, plus the sweep amortized every 4096 keys.
/// </description></item>
/// </list>
/// </summary>
[MemoryDiagnoser]
public class FingerprintBufferingBenchmarks
{
    private const string ReplayKey = "fingerprint-key";

    private IdempotencyBenchmarkHost _host = null!;
    private byte[] _body = null!;

    [Params(1024, 64 * 1024, 1024 * 1024)]
    public int BodySize { get; set; }

    [GlobalSetup]
    public async Task Setup()
    {
        _body = new byte[BodySize];
        RandomNumberGenerator.Fill(_body);
        _host = await IdempotencyBenchmarkHost.StartAsync().ConfigureAwait(false);
        await _host.VerifyAsync(_body, ReplayKey).ConfigureAwait(false);
    }

    [GlobalCleanup]
    public async Task Cleanup() => await _host.DisposeAsync().ConfigureAwait(false);

    [Benchmark(Baseline = true)]
    public Task<HttpStatusCode> Plain() =>
        _host.SendAsync(IdempotencyBenchmarkHost.PlainPath, _body, null);

    [Benchmark]
    public Task<HttpStatusCode> KeyedReplay() =>
        _host.SendAsync(IdempotencyBenchmarkHost.KeyedPath, _body, ReplayKey);

    [Benchmark]
    public Task<HttpStatusCode> KeyedNewKey() =>
        _host.SendAsync(IdempotencyBenchmarkHost.KeyedPath, _body, _host.NextKey());
}
