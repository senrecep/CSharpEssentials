using System.Globalization;
using CSharpEssentials.Idempotency;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace CSharpEssentials.AspNetCore.Comparison.Benchmarks;

/// <summary>
/// The CSE record stores alone, with a <see cref="PayloadSize"/>-byte payload: the in-memory store (clock moved past
/// retention every 4096 keys, so it stays bounded) and the <c>IDistributedCache</c> store over
/// <c>MemoryDistributedCache</c> (a new cache every 1024 keys, so the 200 MB limit never refuses a write). No competitor
/// exposes a store API on its own, so there is no competitor row.
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class IdempotencyStoreBenchmarks
{
    private const string InMemory = "InMemory";
    private const string Distributed = "Distributed";
    private const string Fingerprint = "fp-1";
    private const int KeysPerWindow = 1024;

    private static readonly TimeSpan Lease = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan Retention = TimeSpan.FromHours(24);
    private static readonly IdempotencyRecordKey HitKey = new("bench", IdempotencyScope.Global, "hit");

    private FakeTimeProvider _clock = null!;
    private InMemoryIdempotencyRecordStore _memory = null!;
    private InMemoryIdempotencyRecordStore _memoryHit = null!;
    private DistributedCacheIdempotencyRecordStore _distributed = null!;
    private DistributedCacheIdempotencyRecordStore _distributedHit = null!;
    private byte[] _payload = null!;
    private long _keys;

    [Params(0, 1024, 65536)]
    public int PayloadSize { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _payload = System.Text.Encoding.ASCII.GetBytes(Payloads.Text(PayloadSize, Payloads.ReplaySeed));
        _clock = new FakeTimeProvider();
        _memory = new InMemoryIdempotencyRecordStore(_clock);
        _memoryHit = new InMemoryIdempotencyRecordStore(new FakeTimeProvider());
        _distributed = NewDistributedStore();
        _distributedHit = NewDistributedStore();
        Complete(_memoryHit, HitKey);
        Complete(_distributedHit, HitKey);

        string completed = $"Completed:{Parity.Of(PayloadSize)}";
        Parity.Same(InMemory,
            ("expected", $"True|{completed}|True"),
            ("CSharpEssentials", $"{Run(CSharpEssentials_InMemory_ReserveComplete())}|{Describe(Run(CSharpEssentials_InMemory_ReserveHit()))}|{Run(CSharpEssentials_InMemory_ReserveRelease())}"));
        Parity.Same(Distributed,
            ("expected", $"True|{completed}"),
            ("CSharpEssentials", $"{Run(CSharpEssentials_Distributed_ReserveComplete())}|{Describe(Run(CSharpEssentials_Distributed_Replay()))}"));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Idempotency/Readme.MD#use-the-store-directly
    [Benchmark(Baseline = true), BenchmarkCategory(InMemory)]
    public async ValueTask<bool> CSharpEssentials_InMemory_ReserveComplete()
    {
        IdempotencyRecordKey key = new("bench", IdempotencyScope.Global, NextKey());
        IdempotencyRecordReservation reservation = await _memory.TryReserveAsync(key, Fingerprint, Lease).ConfigureAwait(false);
        return await _memory.CompleteAsync(key, reservation.Token!, Fingerprint, _payload, Retention).ConfigureAwait(false);
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Idempotency/Readme.MD#use-the-store-directly
    [Benchmark, BenchmarkCategory(InMemory)]
    public ValueTask<IdempotencyRecordReservation> CSharpEssentials_InMemory_ReserveHit() => _memoryHit.TryReserveAsync(HitKey, Fingerprint, Lease);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Idempotency/Readme.MD#use-the-store-directly
    [Benchmark, BenchmarkCategory(InMemory)]
    public async ValueTask<bool> CSharpEssentials_InMemory_ReserveRelease()
    {
        IdempotencyRecordKey key = new("bench", IdempotencyScope.Global, NextKey());
        IdempotencyRecordReservation reservation = await _memory.TryReserveAsync(key, Fingerprint, Lease).ConfigureAwait(false);
        return await _memory.ReleaseAsync(key, reservation.Token!).ConfigureAwait(false);
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Idempotency/Readme.MD#stores
    [Benchmark(Baseline = true), BenchmarkCategory(Distributed)]
    public async ValueTask<bool> CSharpEssentials_Distributed_ReserveComplete()
    {
        long id = Interlocked.Increment(ref _keys);
        if (id % KeysPerWindow == 0)
            _distributed = NewDistributedStore();
        IdempotencyRecordKey key = new("bench", IdempotencyScope.Global, "key-" + id.ToString(CultureInfo.InvariantCulture));
        IdempotencyRecordReservation reservation = await _distributed.TryReserveAsync(key, Fingerprint, Lease).ConfigureAwait(false);
        return await _distributed.CompleteAsync(key, reservation.Token!, Fingerprint, _payload, Retention).ConfigureAwait(false);
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Idempotency/Readme.MD#stores
    [Benchmark, BenchmarkCategory(Distributed)]
    public ValueTask<IdempotencyRecordReservation> CSharpEssentials_Distributed_Replay() => _distributedHit.TryReserveAsync(HitKey, Fingerprint, Lease);

    private static DistributedCacheIdempotencyRecordStore NewDistributedStore() =>
        new(new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions())));

    private static T Run<T>(ValueTask<T> task) => task.AsTask().GetAwaiter().GetResult();

    private static string Describe(IdempotencyRecordReservation reservation) =>
        $"{reservation.Status}:{Parity.Of(reservation.Payload?.Length ?? -1)}";

    private void Complete(IIdempotencyRecordStore store, IdempotencyRecordKey key)
    {
        IdempotencyRecordReservation reservation = Run(store.TryReserveAsync(key, Fingerprint, Lease));
        Run(store.CompleteAsync(key, reservation.Token!, Fingerprint, _payload, Retention));
    }

    private string NextKey()
    {
        long key = Interlocked.Increment(ref _keys);
        if (key % 4096 == 0)
            _clock.Advance(TimeSpan.FromDays(2));
        return "key-" + key.ToString(CultureInfo.InvariantCulture);
    }
}
