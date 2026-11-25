using CSharpEssentials.Maybe;
using Xunit;

namespace CSharpEssentials.Locking.Testing;

/// <summary>
/// Behavior every <see cref="IResourceLock"/> must have. Derive from it in an xUnit test project and implement
/// <see cref="CreateLock"/>; every scenario then runs against your lock. Keys are unique per test instance
/// (<see cref="Key"/>), so the scenarios can share one database or cache. The waits are real: a lock that polls should
/// poll more often than <see cref="ShortWait"/>.
/// </summary>
public abstract class ResourceLockContractTests
{
    private const int ParallelAcquirers = 8;

    private readonly string _prefix = "contract-" + Guid.NewGuid().ToString("N") + ":";

    /// <summary>A timeout the scenarios expect to pass while the key is held elsewhere. Default: 300 ms.</summary>
    protected virtual TimeSpan ShortWait => TimeSpan.FromMilliseconds(300);

    /// <summary>The longest the scenarios wait for an acquire that must succeed. Default: 10 seconds.</summary>
    protected virtual TimeSpan LongWait => TimeSpan.FromSeconds(10);

    /// <summary>The lock under test, created on first use.</summary>
    protected IResourceLock Lock => field ??= CreateLock();

    /// <summary>Creates the lock under test.</summary>
    protected abstract IResourceLock CreateLock();

    /// <summary>A key unique to this test instance.</summary>
    protected string Key(string name) => _prefix + name;

    [Fact]
    public async Task TryAcquire_returns_a_handle_for_a_free_key()
    {
        Maybe<IAsyncDisposable> handle = await Lock.TryAcquireAsync(Key("a"));

        Assert.True(handle.HasValue);
        await handle.Value.DisposeAsync();
    }

    [Fact]
    public async Task TryAcquire_returns_none_while_the_key_is_held()
    {
        await using IAsyncDisposable held = await Lock.AcquireAsync(Key("a"), LongWait);

        Maybe<IAsyncDisposable> second = await Lock.TryAcquireAsync(Key("a"));

        Assert.False(second.HasValue);
    }

    [Fact]
    public async Task Disposing_the_handle_releases_the_key()
    {
        IAsyncDisposable held = await Lock.AcquireAsync(Key("a"), LongWait);
        await held.DisposeAsync();

        Maybe<IAsyncDisposable> again = await Lock.TryAcquireAsync(Key("a"));

        Assert.True(again.HasValue);
        await again.Value.DisposeAsync();
    }

    [Fact]
    public async Task Different_keys_do_not_block_each_other()
    {
        await using IAsyncDisposable first = await Lock.AcquireAsync(Key("a"), LongWait);

        Maybe<IAsyncDisposable> other = await Lock.TryAcquireAsync(Key("b"));

        Assert.True(other.HasValue);
        await other.Value.DisposeAsync();
    }

    [Fact]
    public async Task Acquire_throws_TimeoutException_when_the_key_stays_held()
    {
        await using IAsyncDisposable held = await Lock.AcquireAsync(Key("a"), LongWait);

        await Assert.ThrowsAsync<TimeoutException>(async () => await Lock.AcquireAsync(Key("a"), ShortWait));
    }

    [Fact]
    public async Task Acquire_with_zero_timeout_throws_TimeoutException_when_the_key_is_held()
    {
        await using IAsyncDisposable held = await Lock.AcquireAsync(Key("a"), LongWait);

        await Assert.ThrowsAsync<TimeoutException>(async () => await Lock.AcquireAsync(Key("a"), TimeSpan.Zero));
    }

    [Fact]
    public async Task Acquire_waits_until_the_holder_releases()
    {
        IAsyncDisposable held = await Lock.AcquireAsync(Key("a"), LongWait);
        Task<IAsyncDisposable> waiting = Lock.AcquireAsync(Key("a"), LongWait).AsTask();
        await Task.Delay(ShortWait);
        Assert.False(waiting.IsCompleted);

        await held.DisposeAsync();
        IAsyncDisposable next = await waiting.WaitAsync(LongWait);

        await next.DisposeAsync();
    }

    [Fact]
    public async Task Acquire_throws_OperationCanceledException_when_cancelled_while_waiting()
    {
        await using IAsyncDisposable held = await Lock.AcquireAsync(Key("a"), LongWait);
        using CancellationTokenSource cancellation = new(ShortWait);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await Lock.AcquireAsync(Key("a"), null, cancellation.Token));
    }

    [Fact]
    public async Task Disposing_a_handle_twice_does_not_release_the_next_holder()
    {
        IAsyncDisposable first = await Lock.AcquireAsync(Key("a"), LongWait);
        await first.DisposeAsync();
        await using IAsyncDisposable second = await Lock.AcquireAsync(Key("a"), LongWait);

        await first.DisposeAsync();
        Maybe<IAsyncDisposable> third = await Lock.TryAcquireAsync(Key("a"));

        Assert.False(third.HasValue);
    }

    [Fact]
    public async Task Parallel_acquirers_hold_the_key_one_at_a_time()
    {
        int inside = 0;
        int overlaps = 0;

        await Task.WhenAll(Enumerable.Range(0, ParallelAcquirers).Select(async _ =>
        {
            await using IAsyncDisposable handle = await Lock.AcquireAsync(Key("a"), LongWait);
            if (Interlocked.Increment(ref inside) > 1)
                Interlocked.Increment(ref overlaps);

            await Task.Delay(10);
            Interlocked.Decrement(ref inside);
        }));

        Assert.Equal(0, overlaps);
    }

    [Fact]
    public async Task Empty_or_null_keys_are_rejected()
    {
        await Assert.ThrowsAnyAsync<ArgumentException>(async () => await Lock.AcquireAsync("", LongWait));
        await Assert.ThrowsAnyAsync<ArgumentException>(async () => await Lock.AcquireAsync(null!, LongWait));
        await Assert.ThrowsAnyAsync<ArgumentException>(async () => await Lock.TryAcquireAsync(""));
    }
}
