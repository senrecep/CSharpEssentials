using CSharpEssentials.Idempotency;
using CSharpEssentials.Idempotency.Testing;

namespace Examples.Idempotency.Testing;

public sealed class InMemoryIdempotencyRecordStoreTests : IdempotencyRecordStoreContractTests
{
    protected override IIdempotencyRecordStore CreateStore(TimeProvider timeProvider) =>
        new InMemoryIdempotencyRecordStore(timeProvider);

    [Fact]
    public async Task Completed_Entry_Expires_After_Its_Retention()
    {
        string token = await ReserveAsync(Key("order-1"));
        await Store.CompleteAsync(Key("order-1"), token, "fp", Payload(), Retention);

        await AdvanceAsync(Retention + ExpirySlack);
        IdempotencyRecordReservation again = await Store.TryReserveAsync(Key("order-1"), "fp", Lease);

        Assert.Equal(IdempotencyReservationStatus.Reserved, again.Status);
    }
}
