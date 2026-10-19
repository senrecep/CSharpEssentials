using CSharpEssentials.Idempotency;
using CSharpEssentials.Idempotency.Testing;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;

namespace CSharpEssentials.Tests.Idempotency;

public sealed class InMemoryIdempotencyRecordStoreTests : IdempotencyRecordStoreContractTests
{
    protected override IIdempotencyRecordStore CreateStore(TimeProvider timeProvider) =>
        new InMemoryIdempotencyRecordStore(timeProvider);

    private InMemoryIdempotencyRecordStore InMemory => (InMemoryIdempotencyRecordStore)Store;

    [Fact]
    public async Task Sweep_Should_Remove_Expired_Entries_And_Keep_Retained_Ones()
    {
        await ReserveAsync(Key("k1"));
        string token = await ReserveAsync(Key("k2"));
        await Store.CompleteAsync(Key("k2"), token, "fp", Payload(), Retention);

        await AdvanceAsync(TimeSpan.FromMinutes(2));
        await Store.TryReserveAsync(Key("k3"), "fp", Lease);
        int count = InMemory.Count;
        IdempotencyRecordReservation k1 = await Store.TryReserveAsync(Key("k1"), "other", Lease);
        IdempotencyRecordReservation k2 = await Store.TryReserveAsync(Key("k2"), "fp", Lease);

        count.Should().Be(2, "the expired k1 is swept, the completed k2 and the new k3 stay");
        k1.Status.Should().Be(IdempotencyReservationStatus.Reserved);
        k2.Status.Should().Be(IdempotencyReservationStatus.Completed);
    }

    [Fact]
    public async Task Sweep_Should_Run_At_Most_Once_A_Minute()
    {
        await ReserveAsync(Key("k1"));
        await AdvanceAsync(Lease + TimeSpan.FromSeconds(1));

        await Store.TryReserveAsync(Key("k2"), "fp", Lease);

        InMemory.Count.Should().Be(2, "the first sweep ran with the first reservation, the next one is not due yet");
    }

    [Fact]
    public async Task TryReserve_Should_Honor_Cancellation()
    {
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();

        Func<Task> act = async () => await Store.TryReserveAsync(Key("k1"), "fp", Lease, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public void Constructor_Should_Reject_A_Null_TimeProvider()
    {
        Action act = () => _ = new InMemoryIdempotencyRecordStore(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task Parameterless_Constructor_Should_Use_The_System_Clock()
    {
        InMemoryIdempotencyRecordStore store = new();
        IdempotencyRecordKey key = new("ns", "user-1", "k1");

        IdempotencyRecordReservation first = await store.TryReserveAsync(key, "fp", TimeSpan.FromMinutes(1));
        IdempotencyRecordReservation second = await store.TryReserveAsync(key, "fp", TimeSpan.FromMinutes(1));

        first.Status.Should().Be(IdempotencyReservationStatus.Reserved);
        second.Status.Should().Be(IdempotencyReservationStatus.InFlight);
    }

    [Fact]
    public void Clock_Should_Be_A_FakeTimeProvider_By_Default() =>
        Clock.Should().BeOfType<FakeTimeProvider>();
}
