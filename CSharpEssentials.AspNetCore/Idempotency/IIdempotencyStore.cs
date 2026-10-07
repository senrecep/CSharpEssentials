namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Stores <c>Idempotency-Key</c> reservations and the responses they produced.
/// <para>
/// <see cref="TryReserveAsync"/> must be atomic for strict guarantees: two concurrent calls with the same key may not
/// both return <see cref="IdempotencyReservationStatus.Reserved"/>. <see cref="InMemoryIdempotencyStore"/> is atomic
/// within one process; <see cref="DistributedCacheIdempotencyStore"/> is best-effort because <c>IDistributedCache</c>
/// has no set-if-absent. Use a store backed by Redis <c>SET NX</c> or a unique database index for several instances.
/// </para>
/// <para>
/// Every reservation has a token. <see cref="CompleteAsync"/> and <see cref="ReleaseAsync"/> only change an entry that
/// still carries it, so a request that outlived its <c>inFlightTimeout</c> cannot overwrite or remove the reservation of
/// a later retry (for example <c>UPDATE ... WHERE key = @key AND token = @token</c>, or a compare-and-delete script).
/// This protects the entry only: the side effects of the late request have already happened.
/// </para>
/// </summary>
public interface IIdempotencyStore
{
    /// <summary>
    /// Reserves <paramref name="key"/> for <paramref name="inFlightTimeout"/>, or reports why it cannot be reserved.
    /// A key whose stored fingerprint differs from <paramref name="fingerprint"/> returns
    /// <see cref="IdempotencyReservationStatus.FingerprintMismatch"/>, whether it is in flight or completed.
    /// An expired entry is treated as absent.
    /// </summary>
    ValueTask<IdempotencyReservation> TryReserveAsync(
        string key,
        string fingerprint,
        TimeSpan inFlightTimeout,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores the response of the reservation identified by <paramref name="token"/> and keeps it for
    /// <paramref name="retention"/>. Returns <see langword="false"/> without changing anything when the key is held by
    /// another reservation or already completed.
    /// </summary>
    ValueTask<bool> CompleteAsync(
        string key,
        string token,
        string fingerprint,
        IdempotentResponse response,
        TimeSpan retention,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the in-flight reservation identified by <paramref name="token"/> so the client can retry with the same
    /// key. Returns <see langword="false"/> when the key is held by another reservation, completed or absent.
    /// </summary>
    ValueTask<bool> ReleaseAsync(string key, string token, CancellationToken cancellationToken = default);
}
