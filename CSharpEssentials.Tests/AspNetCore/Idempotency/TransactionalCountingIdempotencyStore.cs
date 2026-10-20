using CSharpEssentials.Idempotency;

namespace CSharpEssentials.Tests.AspNetCore.Idempotency;

/// <summary>A transactional store whose own records and detached records are counted apart.</summary>
internal sealed class TransactionalCountingIdempotencyStore(TimeProvider timeProvider) : ITransactionalIdempotencyRecordStore
{
    private readonly CountingIdempotencyStore _attached = new(timeProvider);
    private readonly CountingIdempotencyStore _detached = new(timeProvider);

    public int AttachedReservations => _attached.Reservations;

    public int DetachedReservations => _detached.Reservations;

    public IIdempotencyRecordStore Detached => _detached;

    public ValueTask<IdempotencyRecordReservation> TryReserveAsync(IdempotencyRecordKey key, string fingerprint, TimeSpan lease, CancellationToken cancellationToken = default) =>
        _attached.TryReserveAsync(key, fingerprint, lease, cancellationToken);

    public ValueTask<bool> CompleteAsync(IdempotencyRecordKey key, string token, string fingerprint, ReadOnlyMemory<byte> payload, TimeSpan retention, CancellationToken cancellationToken = default) =>
        _attached.CompleteAsync(key, token, fingerprint, payload, retention, cancellationToken);

    public ValueTask<bool> ReleaseAsync(IdempotencyRecordKey key, string token, CancellationToken cancellationToken = default) =>
        _attached.ReleaseAsync(key, token, cancellationToken);
}
