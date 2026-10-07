using Microsoft.Extensions.Caching.Distributed;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// <see cref="IIdempotencyStore"/> on top of any <see cref="IDistributedCache"/>, so the replay works across instances.
/// <para>
/// <see cref="IDistributedCache"/> has no atomic set-if-absent: two requests with the same key that arrive at the same
/// moment can both be reserved and both run. The <c>409</c> for concurrent requests is therefore best-effort; replay of
/// completed responses is not affected. Use a store built on Redis <c>SET NX</c> or a unique index when concurrent
/// duplicates must never run. For the same reason the token check of <see cref="CompleteAsync"/> and
/// <see cref="ReleaseAsync"/> is a read followed by a write, not an atomic compare.
/// </para>
/// </summary>
public sealed class DistributedCacheIdempotencyStore(IDistributedCache cache) : IIdempotencyStore
{
    /// <summary>Prefix of the cache keys written by this store.</summary>
    public const string KeyPrefix = "idempotency:";

    public async ValueTask<IdempotencyReservation> TryReserveAsync(
        string key,
        string fingerprint,
        TimeSpan inFlightTimeout,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentException.ThrowIfNullOrEmpty(fingerprint);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(inFlightTimeout, TimeSpan.Zero);

        byte[]? data = await cache.GetAsync(KeyPrefix + key, cancellationToken);
        if (data is not null && IdempotencyEntrySerializer.TryRead(data, out _, out string storedFingerprint, out IdempotentResponse? response))
        {
            if (!string.Equals(storedFingerprint, fingerprint, StringComparison.Ordinal))
                return IdempotencyReservation.FingerprintMismatch;
            return response is null ? IdempotencyReservation.InFlight : IdempotencyReservation.Completed(response);
        }

        string token = Guid.NewGuid().ToString("N");
        await cache.SetAsync(
            KeyPrefix + key,
            IdempotencyEntrySerializer.Write(token, fingerprint, null),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = inFlightTimeout },
            cancellationToken);
        return IdempotencyReservation.Reserved(token);
    }

    public async ValueTask<bool> CompleteAsync(
        string key,
        string token,
        string fingerprint,
        IdempotentResponse response,
        TimeSpan retention,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentException.ThrowIfNullOrEmpty(token);
        ArgumentException.ThrowIfNullOrEmpty(fingerprint);
        ArgumentNullException.ThrowIfNull(response);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(retention, TimeSpan.Zero);

        // An expired or unreadable entry may be overwritten: nobody else holds the key.
        byte[]? data = await cache.GetAsync(KeyPrefix + key, cancellationToken);
        if (data is not null
            && IdempotencyEntrySerializer.TryRead(data, out string storedToken, out _, out IdempotentResponse? stored)
            && (stored is not null || !string.Equals(storedToken, token, StringComparison.Ordinal)))
            return false;

        await cache.SetAsync(
            KeyPrefix + key,
            IdempotencyEntrySerializer.Write(token, fingerprint, response),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = retention },
            cancellationToken);
        return true;
    }

    public async ValueTask<bool> ReleaseAsync(string key, string token, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentException.ThrowIfNullOrEmpty(token);

        byte[]? data = await cache.GetAsync(KeyPrefix + key, cancellationToken);
        if (data is null
            || !IdempotencyEntrySerializer.TryRead(data, out string storedToken, out _, out IdempotentResponse? stored)
            || stored is not null
            || !string.Equals(storedToken, token, StringComparison.Ordinal))
            return false;

        await cache.RemoveAsync(KeyPrefix + key, cancellationToken);
        return true;
    }
}
