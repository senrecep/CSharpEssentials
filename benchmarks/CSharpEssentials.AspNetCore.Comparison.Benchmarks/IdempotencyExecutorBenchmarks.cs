using System.Collections.Concurrent;
using System.Globalization;
using CSharpEssentials.Idempotency;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace CSharpEssentials.AspNetCore.Comparison.Benchmarks;

/// <summary>
/// The transport-independent executor over the in-memory store, with an operation that returns 42. No competitor offers
/// an executor without HTTP, so the baselines are the operation alone and a hand-written
/// <c>ConcurrentDictionary.GetOrAdd</c> dedupe (labelled: no lease, fingerprint, retention or mismatch detection).
/// FirstRun and Inbox_New use a new key per call on their own store, whose clock moves past retention every 4096 keys so
/// the store stays bounded; Replay, InFlight and KeyReused use keys prepared in setup on a store whose clock never moves.
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class IdempotencyExecutorBenchmarks
{
    private const string Execute = "Execute";
    private const string Inbox = "Inbox";
    private const string Fingerprint = "fp-1";
    private const int KeysPerRetentionWindow = 4096;

    private static readonly Func<CancellationToken, ValueTask<int>> Operation = static _ => ValueTask.FromResult(42);
    private static readonly IIdempotencyResultCodec<int> Codec = IdempotencyResultCodec.Create<int>(static v => BitConverter.GetBytes(v), static p => BitConverter.ToInt32(p.Span));
    private static readonly IdempotencyExecutionOptions ModeOff = new() { Mode = IdempotencyMode.Off };
    private static readonly IdempotencyRecordKey ReplayKey = new("bench", IdempotencyScope.Global, "replay");
    private static readonly IdempotencyRecordKey InFlightKey = new("bench", IdempotencyScope.Global, "in-flight");

    private readonly ConcurrentDictionary<string, Task<int>> _dictionary = new(StringComparer.Ordinal);
    private FakeTimeProvider _clock = null!;
    private IdempotencyExecutor _fresh = null!;
    private IdempotencyExecutor _prepared = null!;
    private IdempotencyInbox _inbox = null!;
    private long _keys;

    [GlobalSetup]
    public void Setup()
    {
        using ServiceProvider services = new ServiceCollection().AddIdempotencyExecutor().BuildServiceProvider();
        IOptionsMonitor<IdempotencyExecutorOptions> options = services.GetRequiredService<IOptionsMonitor<IdempotencyExecutorOptions>>();
        _clock = new FakeTimeProvider();
        _fresh = new IdempotencyExecutor(new InMemoryIdempotencyRecordStore(_clock), options);
        _inbox = new IdempotencyInbox(_fresh);
        InMemoryIdempotencyRecordStore preparedStore = new(new FakeTimeProvider());
        _prepared = new IdempotencyExecutor(preparedStore, options);
        Run(_prepared.ExecuteAsync(ReplayKey, Fingerprint, null, Operation, Codec));
        Parity.Same(Execute + " reservation", ("expected", "Reserved"), ("in-flight", Run(preparedStore.TryReserveAsync(InFlightKey, Fingerprint, TimeSpan.FromDays(365))).Status.ToString()));

        Parity.Same(Execute, ("expected", "42"), (nameof(Direct_Call), Parity.Of(Run(Direct_Call()))), (nameof(HandWritten_ConcurrentDictionary), Parity.Of(Run(HandWritten_ConcurrentDictionary()))));
        Parity.Same(Execute + " status",
            ("expected", "Executed:42|Replayed:42|InFlight|KeyReused|Executed:42"),
            ("CSharpEssentials", string.Join("|", Describe(Run(CSharpEssentials_FirstRun())), Describe(Run(CSharpEssentials_Replay())), Describe(Run(CSharpEssentials_InFlight())), Describe(Run(CSharpEssentials_KeyReused())), Describe(Run(CSharpEssentials_ModeOff())))));
        string message = NextKey();
        Parity.Same(Inbox,
            ("expected", "Executed:42|Replayed:42"),
            ("CSharpEssentials", Describe(Run(_inbox.ProcessAsync("billing", message, IdempotencyScope.Global, Operation, Codec))) + "|" + Describe(Run(_inbox.ProcessAsync("billing", message, IdempotencyScope.Global, Operation, Codec)))));
        Run(_inbox.ProcessAsync("billing", "duplicate", IdempotencyScope.Global, Operation, Codec));
        Parity.Same(Inbox + " duplicate", ("expected", "Replayed:42"), (nameof(CSharpEssentials_Inbox_Duplicate), Describe(Run(CSharpEssentials_Inbox_Duplicate()))));
    }

    // https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask-1
    [Benchmark(Baseline = true), BenchmarkCategory(Execute)]
    public ValueTask<int> Direct_Call() => Operation(CancellationToken.None);

    // https://learn.microsoft.com/dotnet/api/system.collections.concurrent.concurrentdictionary-2.getoradd
    [Benchmark, BenchmarkCategory(Execute)]
    public async ValueTask<int> HandWritten_ConcurrentDictionary() =>
        await _dictionary.GetOrAdd("replay", static _ => Task.FromResult(42)).ConfigureAwait(false);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Idempotency/Readme.MD#run-an-operation-once
    [Benchmark, BenchmarkCategory(Execute)]
    public ValueTask<IdempotencyExecution<int>> CSharpEssentials_FirstRun() =>
        _fresh.ExecuteAsync(new IdempotencyRecordKey("bench", IdempotencyScope.Global, NextKey()), Fingerprint, null, Operation, Codec);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Idempotency/Readme.MD#run-an-operation-once
    [Benchmark, BenchmarkCategory(Execute)]
    public ValueTask<IdempotencyExecution<int>> CSharpEssentials_Replay() => _prepared.ExecuteAsync(ReplayKey, Fingerprint, null, Operation, Codec);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Idempotency/Readme.MD#run-an-operation-once
    [Benchmark, BenchmarkCategory(Execute)]
    public ValueTask<IdempotencyExecution<int>> CSharpEssentials_InFlight() => _prepared.ExecuteAsync(InFlightKey, Fingerprint, null, Operation, Codec);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Idempotency/Readme.MD#run-an-operation-once
    [Benchmark, BenchmarkCategory(Execute)]
    public ValueTask<IdempotencyExecution<int>> CSharpEssentials_KeyReused() => _prepared.ExecuteAsync(ReplayKey, "fp-2", null, Operation, Codec);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Idempotency/Readme.MD#guarantee-modes
    [Benchmark, BenchmarkCategory(Execute)]
    public ValueTask<IdempotencyExecution<int>> CSharpEssentials_ModeOff() => _prepared.ExecuteAsync(ReplayKey, Fingerprint, ModeOff, Operation, Codec);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Idempotency/Readme.MD#process-a-message-once
    [Benchmark(Baseline = true), BenchmarkCategory(Inbox)]
    public ValueTask<IdempotencyExecution<int>> CSharpEssentials_Inbox_New() =>
        _inbox.ProcessAsync("billing", NextKey(), IdempotencyScope.Global, Operation, Codec);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Idempotency/Readme.MD#process-a-message-once
    [Benchmark, BenchmarkCategory(Inbox)]
    public ValueTask<IdempotencyExecution<int>> CSharpEssentials_Inbox_Duplicate() =>
        _inbox.ProcessAsync("billing", "duplicate", IdempotencyScope.Global, Operation, Codec);

    private static T Run<T>(ValueTask<T> task) => task.AsTask().GetAwaiter().GetResult();

    private static string Describe(IdempotencyExecution<int> execution) =>
        execution.HasValue ? $"{execution.Status}:{Parity.Of(execution.Value)}" : execution.Status.ToString();

    private string NextKey()
    {
        long key = Interlocked.Increment(ref _keys);
        if (key % KeysPerRetentionWindow == 0)
            _clock.Advance(TimeSpan.FromDays(8));
        return "key-" + key.ToString(CultureInfo.InvariantCulture);
    }
}
