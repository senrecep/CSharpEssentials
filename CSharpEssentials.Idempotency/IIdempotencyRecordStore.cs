namespace CSharpEssentials.Idempotency;

/// <summary>
/// Stores idempotency reservations and the opaque payload each one completed with. The store does not know what the
/// payload holds (an HTTP response, a handler result); the caller serializes it.
/// <para>Rules every implementation follows:</para>
/// <list type="bullet">
/// <item><description>
/// <see cref="TryReserveAsync"/> is one atomic operation for strict guarantees: two concurrent calls for the same key
/// never both return <see cref="IdempotencyReservationStatus.Reserved"/> (for example Redis <c>SET NX</c> or an insert
/// on a unique index). <see cref="InMemoryIdempotencyRecordStore"/> is atomic within one process;
/// <see cref="DistributedCacheIdempotencyRecordStore"/> is best-effort because <c>IDistributedCache</c> has no
/// set-if-absent.
/// </description></item>
/// <item><description>
/// The fingerprint is compared before the in-flight state: a live entry with another fingerprint returns
/// <see cref="IdempotencyReservationStatus.FingerprintMismatch"/>, whether it is in flight or completed.
/// </description></item>
/// <item><description>
/// Every reservation has a token. <see cref="CompleteAsync"/> and <see cref="ReleaseAsync"/> only change an entry
/// that still carries it, so a request that outlived its lease cannot overwrite or remove the reservation of a later
/// retry (for example <c>UPDATE ... WHERE key = @key AND token = @token</c>, or a compare-and-delete script). This
/// protects the entry only: the side effects of the late request have already happened.
/// </description></item>
/// <item><description>
/// A reservation whose lease expired but that nobody took over can still be completed: the work is done, and storing
/// its outcome is better than letting a retry run it again.
/// </description></item>
/// <item><description>
/// <see cref="ReleaseAsync"/> removes only an in-flight entry with a matching token; it never removes a completed one.
/// </description></item>
/// </list>
/// <para>
/// Stores that need a flat key use <see cref="IdempotencyRecordKey.ToStorageKey"/>. Arguments are checked:
/// a <see langword="null"/> key throws <see cref="ArgumentNullException"/>, an empty fingerprint or token throws
/// <see cref="ArgumentException"/> and a lease or retention that is not positive throws
/// <see cref="ArgumentOutOfRangeException"/>.
/// </para>
/// </summary>
public interface IIdempotencyRecordStore
{
    /// <summary>
    /// Reserves <paramref name="key"/> for <paramref name="lease"/>, or reports why it cannot be reserved. An expired
    /// entry is treated as absent. A live entry with another fingerprint returns
    /// <see cref="IdempotencyReservationStatus.FingerprintMismatch"/>; otherwise an in-flight entry returns
    /// <see cref="IdempotencyReservationStatus.InFlight"/> and a completed one
    /// <see cref="IdempotencyReservationStatus.Completed"/> with its payload.
    /// </summary>
    ValueTask<IdempotencyRecordReservation> TryReserveAsync(
        IdempotencyRecordKey key,
        string fingerprint,
        TimeSpan lease,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores <paramref name="payload"/> for the reservation identified by <paramref name="token"/> and keeps it for
    /// <paramref name="retention"/>. Succeeds when the entry still carries the token and is in flight, even after its
    /// lease expired, and when the entry is gone. Returns <see langword="false"/> without changing anything when another
    /// reservation holds the key or it is already completed.
    /// </summary>
    ValueTask<bool> CompleteAsync(
        IdempotencyRecordKey key,
        string token,
        string fingerprint,
        ReadOnlyMemory<byte> payload,
        TimeSpan retention,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the in-flight reservation identified by <paramref name="token"/> so the key can be reserved again.
    /// Returns <see langword="false"/> when the key is held by another reservation, completed or absent.
    /// </summary>
    ValueTask<bool> ReleaseAsync(IdempotencyRecordKey key, string token, CancellationToken cancellationToken = default);
}
