using CSharpEssentials.Maybe;

namespace CSharpEssentials.Locking.Testing;

/// <summary>
/// Behavior every <see cref="IResourceLock"/> must have, as scenarios that run against your lock and throw
/// <see cref="ResourceLockContractException"/> when it breaks the contract. It references no test framework: call
/// <see cref="RunAsync"/> from one parameterized test over <see cref="ScenarioNames"/>, or call the scenario methods from
/// tests of your own. Each scenario uses keys of its own, so the scenarios can share one database or cache. The waits are
/// real: a lock that polls should poll more often than <see cref="ShortWait"/>.
/// </summary>
public sealed class ResourceLockContract
{
    private const int ParallelAcquirers = 8;

    private static readonly Dictionary<string, Func<ResourceLockContract, Task>> Scenarios = new(StringComparer.Ordinal)
    {
        [nameof(TryAcquireReturnsAHandleForAFreeKeyAsync)] = static c => c.TryAcquireReturnsAHandleForAFreeKeyAsync(),
        [nameof(TryAcquireReturnsNoneWhileTheKeyIsHeldAsync)] = static c => c.TryAcquireReturnsNoneWhileTheKeyIsHeldAsync(),
        [nameof(DisposingTheHandleReleasesTheKeyAsync)] = static c => c.DisposingTheHandleReleasesTheKeyAsync(),
        [nameof(DifferentKeysDoNotBlockEachOtherAsync)] = static c => c.DifferentKeysDoNotBlockEachOtherAsync(),
        [nameof(AcquireThrowsTimeoutExceptionWhenTheKeyStaysHeldAsync)] = static c => c.AcquireThrowsTimeoutExceptionWhenTheKeyStaysHeldAsync(),
        [nameof(AcquireWithZeroTimeoutThrowsTimeoutExceptionWhenTheKeyIsHeldAsync)] = static c => c.AcquireWithZeroTimeoutThrowsTimeoutExceptionWhenTheKeyIsHeldAsync(),
        [nameof(AcquireWaitsUntilTheHolderReleasesAsync)] = static c => c.AcquireWaitsUntilTheHolderReleasesAsync(),
        [nameof(AcquireThrowsOperationCanceledExceptionWhenCancelledWhileWaitingAsync)] = static c => c.AcquireThrowsOperationCanceledExceptionWhenCancelledWhileWaitingAsync(),
        [nameof(DisposingAHandleTwiceDoesNotReleaseTheNextHolderAsync)] = static c => c.DisposingAHandleTwiceDoesNotReleaseTheNextHolderAsync(),
        [nameof(ParallelAcquirersHoldTheKeyOneAtATimeAsync)] = static c => c.ParallelAcquirersHoldTheKeyOneAtATimeAsync(),
        [nameof(EmptyOrNullKeysAreRejectedAsync)] = static c => c.EmptyOrNullKeysAreRejectedAsync(),
    };

    /// <summary>Creates the scenarios for <paramref name="resourceLock"/>.</summary>
    /// <param name="resourceLock">The lock under test.</param>
    /// <param name="shortWait">A timeout the scenarios expect to pass while the key is held elsewhere. Default: 300 ms.</param>
    /// <param name="longWait">The longest the scenarios wait for an acquire that must succeed. Default: 10 seconds.</param>
    public ResourceLockContract(IResourceLock resourceLock, TimeSpan? shortWait = null, TimeSpan? longWait = null)
    {
        ArgumentNullException.ThrowIfNull(resourceLock);
        Lock = resourceLock;
        ShortWait = shortWait ?? TimeSpan.FromMilliseconds(300);
        LongWait = longWait ?? TimeSpan.FromSeconds(10);
    }

    /// <summary>The names of every scenario, for a parameterized test that calls <see cref="RunAsync"/>.</summary>
    public static IReadOnlyCollection<string> ScenarioNames => Scenarios.Keys;

    /// <summary>The lock under test.</summary>
    public IResourceLock Lock { get; }

    /// <summary>A timeout the scenarios expect to pass while the key is held elsewhere.</summary>
    public TimeSpan ShortWait { get; }

    /// <summary>The longest the scenarios wait for an acquire that must succeed.</summary>
    public TimeSpan LongWait { get; }

    /// <summary>Runs the scenario named <paramref name="scenario"/>, one of <see cref="ScenarioNames"/>.</summary>
    /// <param name="scenario">The scenario name.</param>
    /// <returns>A task that fails with <see cref="ResourceLockContractException"/> when the lock breaks the contract.</returns>
    /// <exception cref="ArgumentException">The name is not one of <see cref="ScenarioNames"/>.</exception>
    public Task RunAsync(string scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        return Scenarios.TryGetValue(scenario, out Func<ResourceLockContract, Task>? run)
            ? run(this)
            : throw new ArgumentException($"Unknown resource lock scenario '{scenario}'.", nameof(scenario));
    }

    /// <summary>TryAcquire returns a handle for a free key.</summary>
    /// <returns>A task that completes when the scenario passes.</returns>
    public async Task TryAcquireReturnsAHandleForAFreeKeyAsync()
    {
        Maybe<IAsyncDisposable> handle = await Lock.TryAcquireAsync(NewKey()).ConfigureAwait(false);

        Check(handle.HasValue, "TryAcquireAsync returned no handle for a free key.");
        await handle.Value.DisposeAsync().ConfigureAwait(false);
    }

    /// <summary>TryAcquire returns none while the key is held.</summary>
    /// <returns>A task that completes when the scenario passes.</returns>
    public async Task TryAcquireReturnsNoneWhileTheKeyIsHeldAsync()
    {
        string key = NewKey();
        IAsyncDisposable held = await Lock.AcquireAsync(key, LongWait).ConfigureAwait(false);
        await using (held.ConfigureAwait(false))
        {
            Maybe<IAsyncDisposable> second = await Lock.TryAcquireAsync(key).ConfigureAwait(false);

            Check(!second.HasValue, "TryAcquireAsync returned a handle for a key that is held.");
        }
    }

    /// <summary>Disposing the handle releases the key.</summary>
    /// <returns>A task that completes when the scenario passes.</returns>
    public async Task DisposingTheHandleReleasesTheKeyAsync()
    {
        string key = NewKey();
        IAsyncDisposable held = await Lock.AcquireAsync(key, LongWait).ConfigureAwait(false);
        await held.DisposeAsync().ConfigureAwait(false);

        Maybe<IAsyncDisposable> again = await Lock.TryAcquireAsync(key).ConfigureAwait(false);

        Check(again.HasValue, "The key was still held after its handle was disposed.");
        await again.Value.DisposeAsync().ConfigureAwait(false);
    }

    /// <summary>Different keys do not block each other.</summary>
    /// <returns>A task that completes when the scenario passes.</returns>
    public async Task DifferentKeysDoNotBlockEachOtherAsync()
    {
        IAsyncDisposable first = await Lock.AcquireAsync(NewKey(), LongWait).ConfigureAwait(false);
        await using (first.ConfigureAwait(false))
        {
            Maybe<IAsyncDisposable> other = await Lock.TryAcquireAsync(NewKey()).ConfigureAwait(false);

            Check(other.HasValue, "Holding one key blocked another key.");
            await other.Value.DisposeAsync().ConfigureAwait(false);
        }
    }

    /// <summary>Acquire throws <see cref="TimeoutException"/> when the key stays held.</summary>
    /// <returns>A task that completes when the scenario passes.</returns>
    public Task AcquireThrowsTimeoutExceptionWhenTheKeyStaysHeldAsync() => ExpectTimeoutWhileHeldAsync(ShortWait);

    /// <summary>Acquire with a zero timeout throws <see cref="TimeoutException"/> when the key is held.</summary>
    /// <returns>A task that completes when the scenario passes.</returns>
    public Task AcquireWithZeroTimeoutThrowsTimeoutExceptionWhenTheKeyIsHeldAsync() => ExpectTimeoutWhileHeldAsync(TimeSpan.Zero);

    /// <summary>Acquire waits until the holder releases.</summary>
    /// <returns>A task that completes when the scenario passes.</returns>
    public async Task AcquireWaitsUntilTheHolderReleasesAsync()
    {
        string key = NewKey();
        IAsyncDisposable held = await Lock.AcquireAsync(key, LongWait).ConfigureAwait(false);
        Task<IAsyncDisposable> waiting = Lock.AcquireAsync(key, LongWait).AsTask();
        await Task.Delay(ShortWait).ConfigureAwait(false);
        Check(!waiting.IsCompleted, "AcquireAsync completed while the key was held.");

        await held.DisposeAsync().ConfigureAwait(false);
        IAsyncDisposable next = await waiting.WaitAsync(LongWait).ConfigureAwait(false);

        await next.DisposeAsync().ConfigureAwait(false);
    }

    /// <summary>Acquire throws <see cref="OperationCanceledException"/> when cancelled while waiting.</summary>
    /// <returns>A task that completes when the scenario passes.</returns>
    public async Task AcquireThrowsOperationCanceledExceptionWhenCancelledWhileWaitingAsync()
    {
        string key = NewKey();
        IAsyncDisposable held = await Lock.AcquireAsync(key, LongWait).ConfigureAwait(false);
        await using (held.ConfigureAwait(false))
        {
            using CancellationTokenSource cancellation = new(ShortWait);

            await ExpectAsync<OperationCanceledException>(
                async () => await Lock.AcquireAsync(key, null, cancellation.Token).ConfigureAwait(false),
                "AcquireAsync cancelled while waiting").ConfigureAwait(false);
        }
    }

    /// <summary>Disposing a handle twice does not release the next holder.</summary>
    /// <returns>A task that completes when the scenario passes.</returns>
    public async Task DisposingAHandleTwiceDoesNotReleaseTheNextHolderAsync()
    {
        string key = NewKey();
        IAsyncDisposable first = await Lock.AcquireAsync(key, LongWait).ConfigureAwait(false);
        await first.DisposeAsync().ConfigureAwait(false);
        IAsyncDisposable second = await Lock.AcquireAsync(key, LongWait).ConfigureAwait(false);
        await using (second.ConfigureAwait(false))
        {
            await first.DisposeAsync().ConfigureAwait(false);
            Maybe<IAsyncDisposable> third = await Lock.TryAcquireAsync(key).ConfigureAwait(false);

            Check(!third.HasValue, "A second dispose of an old handle released the next holder.");
        }
    }

    /// <summary>Parallel acquirers hold the key one at a time.</summary>
    /// <returns>A task that completes when the scenario passes.</returns>
    public async Task ParallelAcquirersHoldTheKeyOneAtATimeAsync()
    {
        string key = NewKey();
        int inside = 0;
        int overlaps = 0;

        await Task.WhenAll(Enumerable.Range(0, ParallelAcquirers).Select(async _ =>
        {
            IAsyncDisposable handle = await Lock.AcquireAsync(key, LongWait).ConfigureAwait(false);
            await using (handle.ConfigureAwait(false))
            {
                if (Interlocked.Increment(ref inside) > 1)
                    Interlocked.Increment(ref overlaps);

                await Task.Delay(10).ConfigureAwait(false);
                Interlocked.Decrement(ref inside);
            }
        })).ConfigureAwait(false);

        Check(overlaps == 0, $"Parallel acquirers held the key at the same time {overlaps} times.");
    }

    /// <summary>Empty or null keys are rejected with <see cref="ArgumentException"/>.</summary>
    /// <returns>A task that completes when the scenario passes.</returns>
    public async Task EmptyOrNullKeysAreRejectedAsync()
    {
        await ExpectAsync<ArgumentException>(async () => await Lock.AcquireAsync("", LongWait).ConfigureAwait(false),
            "AcquireAsync with an empty key").ConfigureAwait(false);
        await ExpectAsync<ArgumentException>(async () => await Lock.AcquireAsync(null!, LongWait).ConfigureAwait(false),
            "AcquireAsync with a null key").ConfigureAwait(false);
        await ExpectAsync<ArgumentException>(async () => await Lock.TryAcquireAsync("").ConfigureAwait(false),
            "TryAcquireAsync with an empty key").ConfigureAwait(false);
    }

    private async Task ExpectTimeoutWhileHeldAsync(TimeSpan timeout)
    {
        string key = NewKey();
        IAsyncDisposable held = await Lock.AcquireAsync(key, LongWait).ConfigureAwait(false);
        await using (held.ConfigureAwait(false))
        {
            await ExpectAsync<TimeoutException>(async () => await Lock.AcquireAsync(key, timeout).ConfigureAwait(false),
                $"AcquireAsync with a {timeout} timeout on a held key").ConfigureAwait(false);
        }
    }

    private static string NewKey() => "contract-" + Guid.NewGuid().ToString("N");

    private static void Check(bool condition, string message)
    {
        if (!condition)
            throw new ResourceLockContractException(message);
    }

    private static async Task ExpectAsync<TException>(Func<Task> action, string what)
        where TException : Exception
    {
        try
        {
            await action().ConfigureAwait(false);
        }
        catch (TException)
        {
            return;
        }
        catch (Exception exception)
        {
            throw new ResourceLockContractException($"{what} threw {exception.GetType().Name} instead of {typeof(TException).Name}.", exception);
        }

        throw new ResourceLockContractException($"{what} did not throw {typeof(TException).Name}.");
    }
}
