using CSharpEssentials.Idempotency;

namespace CSharpEssentials.Tests.AspNetCore.Idempotency;

/// <summary>Reserves in memory and fails every <see cref="CompleteAsync"/> and <see cref="ReleaseAsync"/>.</summary>
internal sealed class ThrowingIdempotencyStore(TimeProvider timeProvider) : IIdempotencyRecordStore
{
    private readonly InMemoryIdempotencyRecordStore _inner = new(timeProvider);

    public ValueTask<IdempotencyRecordReservation> TryReserveAsync(IdempotencyRecordKey key, string fingerprint, TimeSpan lease, CancellationToken cancellationToken = default) =>
        _inner.TryReserveAsync(key, fingerprint, lease, cancellationToken);

    public ValueTask<bool> CompleteAsync(IdempotencyRecordKey key, string token, string fingerprint, ReadOnlyMemory<byte> payload, TimeSpan retention, CancellationToken cancellationToken = default) =>
        throw new IOException("store unavailable");

    public ValueTask<bool> ReleaseAsync(IdempotencyRecordKey key, string token, CancellationToken cancellationToken = default) =>
        throw new IOException("store unavailable");
}
