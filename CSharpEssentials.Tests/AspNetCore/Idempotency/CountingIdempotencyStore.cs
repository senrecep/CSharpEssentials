using CSharpEssentials.Idempotency;

namespace CSharpEssentials.Tests.AspNetCore.Idempotency;

/// <summary>An in-memory store that counts its reservations, to show which store an endpoint uses.</summary>
internal sealed class CountingIdempotencyStore(TimeProvider timeProvider) : IIdempotencyRecordStore
{
    private readonly InMemoryIdempotencyRecordStore _inner = new(timeProvider);
    private int _reservations;

    public int Reservations => Volatile.Read(ref _reservations);

    public ValueTask<IdempotencyRecordReservation> TryReserveAsync(IdempotencyRecordKey key, string fingerprint, TimeSpan lease, CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _reservations);
        return _inner.TryReserveAsync(key, fingerprint, lease, cancellationToken);
    }

    public ValueTask<bool> CompleteAsync(IdempotencyRecordKey key, string token, string fingerprint, ReadOnlyMemory<byte> payload, TimeSpan retention, CancellationToken cancellationToken = default) =>
        _inner.CompleteAsync(key, token, fingerprint, payload, retention, cancellationToken);

    public ValueTask<bool> ReleaseAsync(IdempotencyRecordKey key, string token, CancellationToken cancellationToken = default) =>
        _inner.ReleaseAsync(key, token, cancellationToken);
}
