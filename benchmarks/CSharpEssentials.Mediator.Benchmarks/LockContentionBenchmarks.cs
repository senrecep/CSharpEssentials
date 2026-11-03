using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using AsyncKeyedLock;
using BenchmarkDotNet.Attributes;
using CSharpEssentials.Locking;

namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>
/// The keyed lock primitives without a mediator: 8 workers on the thread pool, 256 acquire, increment, release
/// operations each, 2048 per invocation. AsyncKeyedLock (default options: pooling on, pool size 20) is the baseline.
/// Cse is <see cref="InProcessResourceLock"/>, the default <see cref="IResourceLock"/> of <c>AddMediatorLockBehavior</c>.
/// Every row returns the number of increments, which must be 2048; in <see cref="LockKeys.OneKey"/> the increments go to
/// one shared field without atomics, so a lock that does not exclude would lose some.
/// <para>
/// HandWritten is a labelled reference row, not like-for-like: a <c>ConcurrentDictionary</c> of <c>SemaphoreSlim</c>
/// that never removes or disposes a semaphore. In <see cref="LockKeys.Unique"/> it only pays for lookups after the
/// first invocation and keeps 2048 semaphores alive, while Cse and AsyncKeyedLock remove the entry when the last
/// holder releases it. Keys are built in <see cref="Setup"/>, so no row allocates key strings.
/// </para>
/// </summary>
[MemoryDiagnoser]
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "BenchmarkDotNet owns the lifetime: the locker is created in [GlobalSetup] and disposed in [GlobalCleanup].")]
public class LockContentionBenchmarks
{
    public const int Workers = 8;
    public const int OpsPerWorker = 256;
    public const int Operations = Workers * OpsPerWorker;

    private AsyncKeyedLocker<string> _asyncKeyedLocker = null!;
    private readonly InProcessResourceLock _cse = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _handWritten = new();
    private string[][] _keys = null!;
    private bool _shared;
    private long _sharedCount;

    [Params(LockKeys.OneKey, LockKeys.PerWorker, LockKeys.Unique)]
    public LockKeys Keys { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _asyncKeyedLocker = new AsyncKeyedLocker<string>();
        _shared = Keys == LockKeys.OneKey;
        _keys = new string[Workers][];
        for (int w = 0; w < Workers; w++)
        {
            _keys[w] = new string[OpsPerWorker];
            for (int i = 0; i < OpsPerWorker; i++)
            {
                _keys[w][i] = Keys switch
                {
                    LockKeys.OneKey => "k",
                    LockKeys.PerWorker => "k" + w.ToString(CultureInfo.InvariantCulture),
                    LockKeys.Unique => "k" + w.ToString(CultureInfo.InvariantCulture) + "-" + i.ToString(CultureInfo.InvariantCulture),
                    _ => throw new InvalidOperationException($"Unknown key layout {Keys}."),
                };
            }
        }

        BehaviorSelfCheck.VerifyContention(Keys, AsyncKeyedLock, Cse, HandWritten);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        foreach (SemaphoreSlim semaphore in _handWritten.Values)
        {
            semaphore.Dispose();
        }

        _handWritten.Clear();
        _asyncKeyedLocker.Dispose();
    }

    // https://github.com/MarkCiliaVincenti/AsyncKeyedLock/blob/8.1.2/README.md#installation-and-usage
    [Benchmark(Baseline = true, OperationsPerInvoke = Operations)]
    public Task<long> AsyncKeyedLock() => Run(AsyncKeyedLockWorker);

    [Benchmark(OperationsPerInvoke = Operations)]
    public Task<long> Cse() => Run(CseWorker);

    // https://learn.microsoft.com/dotnet/api/system.collections.concurrent.concurrentdictionary-2.getoradd
    // https://learn.microsoft.com/dotnet/api/system.threading.semaphoreslim.waitasync
    [Benchmark(OperationsPerInvoke = Operations)]
    public Task<long> HandWritten() => Run(HandWrittenWorker);

    private async Task<long> Run(Func<int, Task<long>> worker)
    {
        _sharedCount = 0;
        var tasks = new Task<long>[Workers];
        for (int w = 0; w < Workers; w++)
        {
            int index = w;
            tasks[w] = Task.Run(() => worker(index));
        }

        long[] counts = await Task.WhenAll(tasks).ConfigureAwait(false);
        long total = _sharedCount;
        foreach (long count in counts)
        {
            total += count;
        }

        return total;
    }

    private async Task<long> AsyncKeyedLockWorker(int worker)
    {
        string[] keys = _keys[worker];
        long local = 0;
        for (int i = 0; i < OpsPerWorker; i++)
        {
            using (await _asyncKeyedLocker.LockAsync(keys[i]).ConfigureAwait(false))
            {
                Count(ref local);
            }
        }

        return local;
    }

    private async Task<long> CseWorker(int worker)
    {
        string[] keys = _keys[worker];
        long local = 0;
        for (int i = 0; i < OpsPerWorker; i++)
        {
            IAsyncDisposable handle = await _cse.AcquireAsync(keys[i], null).ConfigureAwait(false);
            await using (handle.ConfigureAwait(false))
            {
                Count(ref local);
            }
        }

        return local;
    }

    private async Task<long> HandWrittenWorker(int worker)
    {
        string[] keys = _keys[worker];
        long local = 0;
        for (int i = 0; i < OpsPerWorker; i++)
        {
            SemaphoreSlim semaphore = _handWritten.GetOrAdd(keys[i], static _ => new SemaphoreSlim(1, 1));
            await semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                Count(ref local);
            }
            finally
            {
                semaphore.Release();
            }
        }

        return local;
    }

    private void Count(ref long local)
    {
        if (_shared)
        {
            _sharedCount++;
        }
        else
        {
            local++;
        }
    }
}
