using CSharpEssentials.Locking;
using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;

using FluentAssertions;

using Mediator;

namespace CSharpEssentials.Tests.Mediator;

public class LockBehaviorTests
{
    private readonly FakeResourceLock _lock = new();

    [Fact]
    public async Task Handle_Should_Hold_Lock_While_Handler_Runs()
    {
        LockBehavior<LockedCommand, Result<int>> behavior = new(_lock);
        bool heldDuringHandler = false;

        Result<int> result = await behavior.Handle(new LockedCommand("order:7"), (_, _) =>
        {
            heldDuringHandler = _lock.Held;
            return new ValueTask<Result<int>>(42);
        }, CancellationToken.None);

        result.Value.Should().Be(42);
        heldDuringHandler.Should().BeTrue();
        _lock.Held.Should().BeFalse();
        _lock.Keys.Should().Equal("order:7");
        _lock.Timeouts.Should().Equal((TimeSpan?)null);
    }

    [Fact]
    public async Task Handle_Should_Pass_Request_Timeout_And_Token()
    {
        LockBehavior<TimedLockedCommand, Result<int>> behavior = new(_lock);
        using CancellationTokenSource cts = new();

        await behavior.Handle(new TimedLockedCommand("k"), (_, _) => new ValueTask<Result<int>>(1), cts.Token);

        _lock.Timeouts.Should().Equal(TimeSpan.FromSeconds(3));
        _lock.Tokens.Should().Equal(cts.Token);
    }

    [Fact]
    public async Task Handle_Should_Release_Lock_When_Handler_Throws()
    {
        LockBehavior<LockedCommand, Result<int>> behavior = new(_lock);

        Func<Task> act = async () => await behavior.Handle(new LockedCommand("k"), (_, _) => throw new InvalidOperationException("boom"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _lock.Held.Should().BeFalse();
        _lock.Releases.Should().Be(1);
    }

    [Fact]
    public async Task Handle_Should_Not_Run_Handler_When_Lock_Times_Out()
    {
        _lock.FailWith = new TimeoutException("busy");
        LockBehavior<LockedCommand, Result<int>> behavior = new(_lock);
        bool handlerRan = false;

        Func<Task> act = async () => await behavior.Handle(new LockedCommand("k"), (_, _) =>
        {
            handlerRan = true;
            return new ValueTask<Result<int>>(1);
        }, CancellationToken.None);

        await act.Should().ThrowAsync<TimeoutException>();
        handlerRan.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Serialize_Requests_With_InProcess_Lock()
    {
        LockBehavior<LockedCommand, Result<int>> behavior = new(new InProcessResourceLock());
        int inside = 0;
        int overlaps = 0;

        await Task.WhenAll(Enumerable.Range(0, 20).Select(_ => behavior.Handle(new LockedCommand("same"), async (_, token) =>
        {
            if (Interlocked.Increment(ref inside) > 1)
                Interlocked.Increment(ref overlaps);
            await Task.Delay(1, token);
            Interlocked.Decrement(ref inside);
            return 1;
        }, CancellationToken.None).AsTask()));

        overlaps.Should().Be(0);
    }
}

internal sealed record LockedCommand(string Key) : ICommand<Result<int>>, ILockedRequest
{
    public string LockKey => Key;
}

internal sealed record TimedLockedCommand(string Key) : ICommand<Result<int>>, ILockedRequest
{
    public string LockKey => Key;

    public TimeSpan? LockTimeout => TimeSpan.FromSeconds(3);
}
