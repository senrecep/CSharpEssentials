using CSharpEssentials.Idempotency;

namespace CSharpEssentials.Tests.AspNetCore.Idempotency;

/// <summary>Fails every call, to show that a request never touched the store.</summary>
internal sealed class UnusedIdempotencyStore : IIdempotencyRecordStore
{
    public ValueTask<IdempotencyRecordReservation> TryReserveAsync(IdempotencyRecordKey key, string fingerprint, TimeSpan lease, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("The store must not be used.");

    public ValueTask<bool> CompleteAsync(IdempotencyRecordKey key, string token, string fingerprint, ReadOnlyMemory<byte> payload, TimeSpan retention, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("The store must not be used.");

    public ValueTask<bool> ReleaseAsync(IdempotencyRecordKey key, string token, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("The store must not be used.");
}
