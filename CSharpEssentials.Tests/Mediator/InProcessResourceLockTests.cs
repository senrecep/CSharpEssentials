using CSharpEssentials.Locking;
using CSharpEssentials.Maybe;

using FluentAssertions;

namespace CSharpEssentials.Tests.Mediator;

public class InProcessResourceLockTests
{
    private static readonly TimeSpan Short = TimeSpan.FromMilliseconds(50);
    private static readonly TimeSpan Long = TimeSpan.FromSeconds(10);

    private readonly InProcessResourceLock _lock = new();

    [Fact]
    public async Task AcquireAsync_Should_Wait_For_Holder_Of_Same_Key()
    {
        IAsyncDisposable first = await _lock.AcquireAsync("order:1", null);

        ValueTask<IAsyncDisposable> second = _lock.AcquireAsync("order:1", Long);
        await Task.Delay(Short);
        second.IsCompleted.Should().BeFalse();

        await first.DisposeAsync();
        await using IAsyncDisposable acquired = await second;
        acquired.Should().NotBeNull();
    }

    [Fact]
    public async Task AcquireAsync_Should_Not_Block_Different_Keys()
    {
        await using IAsyncDisposable first = await _lock.AcquireAsync("order:1", null);

        ValueTask<IAsyncDisposable> second = _lock.AcquireAsync("order:2", Short);

        second.IsCompleted.Should().BeTrue();
        await (await second).DisposeAsync();
    }

    [Fact]
    public async Task AcquireAsync_Should_Serialize_Concurrent_Work_Per_Key()
    {
        int counter = 0;
        int overlaps = 0;
        int inside = 0;

        await Task.WhenAll(Enumerable.Range(0, 50).Select(async _ =>
        {
            await using IAsyncDisposable handle = await _lock.AcquireAsync("counter", Long);
            if (Interlocked.Increment(ref inside) > 1)
                Interlocked.Increment(ref overlaps);
            int read = counter;
            await Task.Yield();
            counter = read + 1;
            Interlocked.Decrement(ref inside);
        }));

        counter.Should().Be(50);
        overlaps.Should().Be(0);
        _lock.KeyCount.Should().Be(0);
    }

    [Fact]
    public async Task TryAcquireAsync_Should_Return_None_While_Key_Is_Held()
    {
        IAsyncDisposable held = await _lock.AcquireAsync("order:1", null);

        Maybe<IAsyncDisposable> whileHeld = await _lock.TryAcquireAsync("order:1");
        await held.DisposeAsync();
        Maybe<IAsyncDisposable> afterRelease = await _lock.TryAcquireAsync("order:1");

        whileHeld.HasNoValue.Should().BeTrue();
        afterRelease.HasValue.Should().BeTrue();
        await afterRelease.Value.DisposeAsync();
        _lock.KeyCount.Should().Be(0);
    }

    [Fact]
    public async Task AcquireAsync_Should_Throw_TimeoutException_And_Release_Entry()
    {
        IAsyncDisposable held = await _lock.AcquireAsync("order:1", null);

        Func<Task> act = async () => await _lock.AcquireAsync("order:1", Short);

        await act.Should().ThrowAsync<TimeoutException>().WithMessage("*order:1*");
        await held.DisposeAsync();
        _lock.KeyCount.Should().Be(0);
    }

    [Fact]
    public async Task AcquireAsync_Should_Throw_When_Cancelled_And_Release_Entry()
    {
        IAsyncDisposable held = await _lock.AcquireAsync("order:1", null);
        using CancellationTokenSource cts = new(Short);

        Func<Task> act = async () => await _lock.AcquireAsync("order:1", null, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        await held.DisposeAsync();
        _lock.KeyCount.Should().Be(0);
    }

    [Fact]
    public async Task TryAcquireAsync_Should_Throw_When_Already_Cancelled()
    {
        Func<Task> act = async () => await _lock.TryAcquireAsync("order:1", new CancellationToken(canceled: true));

        await act.Should().ThrowAsync<OperationCanceledException>();
        _lock.KeyCount.Should().Be(0);
    }

    [Fact]
    public async Task Dispose_Should_Remove_Key_And_Be_Idempotent()
    {
        IAsyncDisposable first = await _lock.AcquireAsync("order:1", null);
        _lock.KeyCount.Should().Be(1);

        await first.DisposeAsync();
        await first.DisposeAsync();

        _lock.KeyCount.Should().Be(0);
        Maybe<IAsyncDisposable> again = await _lock.TryAcquireAsync("order:1");
        again.HasValue.Should().BeTrue();
        Maybe<IAsyncDisposable> blocked = await _lock.TryAcquireAsync("order:1");
        blocked.HasNoValue.Should().BeTrue();
        await again.Value.DisposeAsync();
    }

    [Fact]
    public async Task AcquireAsync_Should_Validate_Arguments()
    {
        Func<Task> nullKey = async () => await _lock.AcquireAsync(null!, null);
        Func<Task> emptyKey = async () => await _lock.AcquireAsync("", null);
        Func<Task> negativeTimeout = async () => await _lock.AcquireAsync("order:1", TimeSpan.FromSeconds(-2));
        Func<Task> hugeTimeout = async () => await _lock.AcquireAsync("order:1", TimeSpan.MaxValue);

        await nullKey.Should().ThrowAsync<ArgumentNullException>();
        await emptyKey.Should().ThrowAsync<ArgumentException>();
        await negativeTimeout.Should().ThrowAsync<ArgumentOutOfRangeException>();
        await hugeTimeout.Should().ThrowAsync<ArgumentOutOfRangeException>();
        _lock.KeyCount.Should().Be(0);
    }

    [Fact]
    public async Task AcquireAsync_Should_Accept_Zero_Timeout_When_Free()
    {
        await using IAsyncDisposable handle = await _lock.AcquireAsync("order:1", TimeSpan.Zero);

        handle.Should().NotBeNull();
    }
}
