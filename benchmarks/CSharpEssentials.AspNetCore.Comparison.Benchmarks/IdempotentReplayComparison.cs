using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CSharpEssentials.Idempotency;
using IdempotentAPI.Cache.DistributedCache.Extensions.DependencyInjection;
using IdempotentAPI.Extensions.DependencyInjection;
using IdempotentAPI.MinimalAPI;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Caching.Distributed;
using ApiIdempotencyOptions = IdempotentAPI.Core.IdempotencyOptions;

namespace CSharpEssentials.AspNetCore.Comparison.Benchmarks;

/// <summary>
/// POST /orders with the same small <c>CreateOrder</c> body on every row; the handler returns
/// <c>TypedResults.Ok(payload)</c>, a <c>ReplayPayload</c> whose JSON is <see cref="ResponseSize"/> bytes of
/// pseudo-random text. Replay sends a key the host completed in setup; NewKey sends a new key each call. The CSE and
/// IdempotentAPI rows store in the same <c>MemoryDistributedCache</c> (200 MB SizeLimit; NewKey rows reach it in a long
/// run and see refused writes and background compaction for both, labelled). Work differs (labelled): CSE stores and
/// replays the response bytes; IdempotentAPI stores the result object as gzipped Newtonsoft JSON and re-serializes it on
/// replay; CSE fingerprints the raw body, IdempotentAPI the bound arguments. InMemoryStore is CSE's default store, not
/// the competitor's. IdempotentAPI sends no replay header, so parity counts handler calls. The counter is captured by the
/// handler instead of injected: IdempotentAPI fingerprints every bound argument, injected services included, so a
/// counter argument whose value changes would turn every retry into a 400.
/// </summary>
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "BenchmarkDotNet owns the lifetime: the hosts are created in [GlobalSetup] and disposed in [GlobalCleanup].")]
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class IdempotentReplayComparison
{
    private const string Replay = "Replay";
    private const string NewKey = "NewKey";
    private const string Path = "/orders";
    private const string ReplayKey = "replay-key";

    private static readonly byte[] RequestBody = """{"orderId":"order-1","amount":25,"currency":"eur"}"""u8.ToArray();

    private BenchHost _plain = null!;
    private BenchHost _cse = null!;
    private BenchHost _idempotentApi = null!;
    private BenchHost _cseInMemory = null!;
    private BenchHost _cseNewKey = null!;
    private BenchHost _idempotentApiNewKey = null!;
    private ReplayPayload _payload = null!;
    private long _keys;

    [Params(1024, 262144)]
    public int ResponseSize { get; set; } = 1024;

    internal string Headers { get; private set; } = string.Empty;

    [GlobalSetup]
    public void Setup()
    {
        int overhead = Payloads.Overhead(new ReplayPayload("replay-1", string.Empty));
        _payload = new ReplayPayload("replay-1", Payloads.Text(ResponseSize - overhead, Payloads.ReplaySeed));
        byte[] expected = Payloads.Json(_payload);
        Parity.True(Replay, expected.Length == ResponseSize, "the payload JSON is not exactly ResponseSize bytes");

        ReplayPayload payload = _payload;
        _plain = BenchHost.Start(static _ => { }, app => MapOrders(app, payload));
        _cse = StartCseDistributed(payload);
        _idempotentApi = StartIdempotentApi(payload);
        _cseInMemory = BenchHost.Start(static b => b.Services.AddIdempotency(static o => o.ScopeResolver = IdempotencyScopes.Global), app =>
        {
            app.UseRouting();
            app.UseIdempotency();
            MapOrders(app, payload).WithIdempotency();
        });
        _cseNewKey = StartCseDistributed(payload);
        _idempotentApiNewKey = StartIdempotentApi(payload);

        Probe plain = _plain.ProbePost(Path, RequestBody);
        Parity.Same(Replay + " baseline", ("expected", Parity.Of(HttpStatusCode.OK, expected, 1)), (nameof(Plain_NoIdempotency), Parity.Of(plain.Status, plain.Body, plain.HandlerCalls)));
        string replayed = $"{Parity.Of(HttpStatusCode.OK, expected, 1)}|{Parity.Of(HttpStatusCode.OK, expected, 0)}";
        Parity.Same(Replay, ("expected", replayed),
            (nameof(CSharpEssentials_Replay), Twice(_cse, ReplayKey, out string cseHeaders)),
            (nameof(IdempotentAPI_Replay), Twice(_idempotentApi, ReplayKey, out string apiHeaders)),
            (nameof(CSharpEssentials_Replay_InMemoryStore), Twice(_cseInMemory, ReplayKey, out _)));
        Parity.Same(NewKey, ("expected", "2"),
            (nameof(CSharpEssentials_NewKey), Parity.Of(NewKeys(_cseNewKey))),
            (nameof(IdempotentAPI_NewKey), Parity.Of(NewKeys(_idempotentApiNewKey))));
        Headers = $"CSE replay headers [{cseHeaders}]; IdempotentAPI replay headers [{apiHeaders}]";
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _plain.Stop();
        _cse.Stop();
        _idempotentApi.Stop();
        _cseInMemory.Stop();
        _cseNewKey.Stop();
        _idempotentApiNewKey.Stop();
    }

    // https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis
    [Benchmark(Baseline = true), BenchmarkCategory(Replay)]
    public Task<HttpStatusCode> Plain_NoIdempotency() => _plain.PostAsync(Path, RequestBody, ReplayKey);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore/Readme.MD#idempotency
    [Benchmark, BenchmarkCategory(Replay)]
    public Task<HttpStatusCode> CSharpEssentials_Replay() => _cse.PostAsync(Path, RequestBody, ReplayKey);

    // https://github.com/ikyriak/IdempotentAPI#using-the-idempotentapi-endpoint-filter-on-minimal-apis and https://github.com/ikyriak/IdempotentAPI#choice-1-default--idempotentapicachedistributedcache
    [Benchmark, BenchmarkCategory(Replay)]
    public Task<HttpStatusCode> IdempotentAPI_Replay() => _idempotentApi.PostAsync(Path, RequestBody, ReplayKey);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore/Readme.MD#idempotency
    [Benchmark, BenchmarkCategory(Replay)]
    public Task<HttpStatusCode> CSharpEssentials_Replay_InMemoryStore() => _cseInMemory.PostAsync(Path, RequestBody, ReplayKey);

    // https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis
    [Benchmark(Baseline = true), BenchmarkCategory(NewKey)]
    public Task<HttpStatusCode> Plain_NoIdempotency_NewKey() => _plain.PostAsync(Path, RequestBody, NextKey());

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore/Readme.MD#idempotency
    [Benchmark, BenchmarkCategory(NewKey)]
    public Task<HttpStatusCode> CSharpEssentials_NewKey() => _cseNewKey.PostAsync(Path, RequestBody, NextKey());

    // https://github.com/ikyriak/IdempotentAPI#using-the-idempotentapi-endpoint-filter-on-minimal-apis
    [Benchmark, BenchmarkCategory(NewKey)]
    public Task<HttpStatusCode> IdempotentAPI_NewKey() => _idempotentApiNewKey.PostAsync(Path, RequestBody, NextKey());

    /// <summary>
    /// Fills a fresh CSE host and a fresh IdempotentAPI host past the 200 MB cache limit with new keys, then sends one more
    /// key twice; returns the handler calls of that pair per library (1 means the replay still worked).
    /// </summary>
    internal string FullCacheProbe(int keys)
    {
        BenchHost cse = StartCseDistributed(_payload);
        BenchHost api = StartIdempotentApi(_payload);
        try
        {
            for (int i = 0; i < keys; i++)
            {
                string key = "fill-" + i.ToString(CultureInfo.InvariantCulture);
                cse.PostAsync(Path, RequestBody, key).GetAwaiter().GetResult();
                api.PostAsync(Path, RequestBody, key).GetAwaiter().GetResult();
            }

            int cseBefore = cse.Counter.Count;
            int apiBefore = api.Counter.Count;
            HttpStatusCode cseFirst = cse.ProbePost(Path, RequestBody, "after-fill").Status;
            HttpStatusCode cseSecond = cse.ProbePost(Path, RequestBody, "after-fill").Status;
            HttpStatusCode apiFirst = api.ProbePost(Path, RequestBody, "after-fill").Status;
            HttpStatusCode apiSecond = api.ProbePost(Path, RequestBody, "after-fill").Status;
            return $"after {keys} keys of {ResponseSize} bytes: CSE {(int)cseFirst}/{(int)cseSecond} handler calls {cse.Counter.Count - cseBefore}; IdempotentAPI {(int)apiFirst}/{(int)apiSecond} handler calls {api.Counter.Count - apiBefore}";
        }
        finally
        {
            cse.Stop();
            api.Stop();
        }
    }

    private static Ok<ReplayPayload> Handle(HandlerCounter counter, ReplayPayload payload)
    {
        counter.Increment();
        return TypedResults.Ok(payload);
    }

    private static RouteHandlerBuilder MapOrders(WebApplication app, ReplayPayload payload)
    {
        HandlerCounter counter = app.Services.GetRequiredService<HandlerCounter>();
        return app.MapPost(Path, (CreateOrder order) => Handle(counter, payload));
    }

    private static BenchHost StartCseDistributed(ReplayPayload payload) => BenchHost.Start(static b =>
    {
        b.Services.AddDistributedMemoryCache();
        b.Services.AddIdempotency(static o =>
        {
            o.ScopeResolver = IdempotencyScopes.Global;
            o.UseDistributedCacheStore();
        });
    }, app =>
    {
        app.UseRouting();
        app.UseIdempotency();
        MapOrders(app, payload).WithIdempotency();
    });

    private static BenchHost StartIdempotentApi(ReplayPayload payload) => BenchHost.Start(static b =>
    {
        b.Services.AddDistributedMemoryCache();
        b.Services.AddIdempotentAPIUsingDistributedCache();
        b.Services.AddIdempotentMinimalAPI(new ApiIdempotencyOptions { HeaderKeyName = "Idempotency-Key" });
    }, app => MapOrders(app, payload).AddEndpointFilter<IdempotentAPIEndpointFilter>());

    private static string Twice(BenchHost host, string key, out string replayHeaders)
    {
        Probe first = host.ProbePost(Path, RequestBody, key);
        Probe second = host.ProbePost(Path, RequestBody, key);
        replayHeaders = second.HeaderNames;
        return $"{Parity.Of(first.Status, first.Body, first.HandlerCalls)}|{Parity.Of(second.Status, second.Body, second.HandlerCalls)}";
    }

    private static int NewKeys(BenchHost host) =>
        host.ProbePost(Path, RequestBody, "parity-new-1").HandlerCalls + host.ProbePost(Path, RequestBody, "parity-new-2").HandlerCalls;

    private string NextKey() => "key-" + Interlocked.Increment(ref _keys).ToString(CultureInfo.InvariantCulture);
}
