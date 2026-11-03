using System.Globalization;
using CSharpEssentials.Idempotency;
using Microsoft.Extensions.Time.Testing;

namespace CSharpEssentials.AspNetCore.Comparison.Benchmarks;

/// <summary>
/// 1024 in-memory store operations per invocation on <see cref="Threads"/> workers: SameKey reserves one completed key
/// from every worker, DistinctKeys reserves and completes a new key per operation (clock moved past retention every
/// 4096 keys). Labelled noisy: the result depends on the machine's core count and scheduler.
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class IdempotencyStoreContentionBenchmarks
{
    private const string Contended = "Contended";
    private const string Fingerprint = "fp-1";
    private const int Operations = 1024;

    private static readonly TimeSpan Lease = TimeSpan.FromMinutes(1);
    private static readonly IdempotencyRecordKey HitKey = new("bench", IdempotencyScope.Global, "hit");
    private static readonly byte[] Payload = System.Text.Encoding.ASCII.GetBytes(Payloads.Text(1024, Payloads.ReplaySeed));

    private FakeTimeProvider _clock = null!;
    private InMemoryIdempotencyRecordStore _store = null!;
    private ParallelOptions _parallel = null!;
    private long _keys;

    [Params(8, 32)]
    public int Threads { get; set; } = 8;

    [GlobalSetup]
    public void Setup()
    {
        _clock = new FakeTimeProvider();
        _store = new InMemoryIdempotencyRecordStore(_clock);
        _parallel = new ParallelOptions { MaxDegreeOfParallelism = Threads };
        IdempotencyRecordReservation reservation = Run(_store.TryReserveAsync(HitKey, Fingerprint, TimeSpan.FromDays(365), CancellationToken.None));
        Run(_store.CompleteAsync(HitKey, reservation.Token!, Fingerprint, Payload, TimeSpan.FromDays(365), CancellationToken.None));
        Parity.Same(Contended, ("expected", $"{Operations}|{Operations}"), ("CSharpEssentials", $"{CSharpEssentials_InMemory_Contended_SameKey()}|{CSharpEssentials_InMemory_Contended_DistinctKeys()}"));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Idempotency/Readme.MD#use-the-store-directly
    [Benchmark(Baseline = true, OperationsPerInvoke = Operations), BenchmarkCategory(Contended)]
    public int CSharpEssentials_InMemory_Contended_SameKey()
    {
        int completed = 0;
        Parallel.For(0, Operations, _parallel, _ =>
        {
            if (Run(_store.TryReserveAsync(HitKey, Fingerprint, Lease, CancellationToken.None)).Status == IdempotencyReservationStatus.Completed)
                Interlocked.Increment(ref completed);
        });
        return completed;
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Idempotency/Readme.MD#use-the-store-directly
    [Benchmark(OperationsPerInvoke = Operations), BenchmarkCategory(Contended)]
    public int CSharpEssentials_InMemory_Contended_DistinctKeys()
    {
        int completed = 0;
        Parallel.For(0, Operations, _parallel, _ =>
        {
            IdempotencyRecordKey key = new("bench", IdempotencyScope.Global, NextKey());
            IdempotencyRecordReservation reservation = Run(_store.TryReserveAsync(key, Fingerprint, Lease, CancellationToken.None));
            if (Run(_store.CompleteAsync(key, reservation.Token!, Fingerprint, Payload, TimeSpan.FromHours(24), CancellationToken.None)))
                Interlocked.Increment(ref completed);
        });
        return completed;
    }

    private static T Run<T>(ValueTask<T> task)
    {
        if (task.IsCompletedSuccessfully)
            return task.Result;
        return task.AsTask().GetAwaiter().GetResult();
    }

    private string NextKey()
    {
        long key = Interlocked.Increment(ref _keys);
        if (key % 4096 == 0)
            _clock.Advance(TimeSpan.FromDays(2));
        return "key-" + key.ToString(CultureInfo.InvariantCulture);
    }
}
