using Microsoft.Extensions.Caching.Distributed;

namespace CSharpEssentials.Idempotency;

/// <summary>
/// <see cref="IIdempotencyRecordStore"/> on top of any <see cref="IDistributedCache"/>. Meant for a single instance or
/// development: it is <b>best-effort, not atomic</b>.
/// <para>
/// <see cref="IDistributedCache"/> has no set-if-absent, so two requests with the same key that arrive at the same moment
/// can both be reserved and both run, and the token checks of <see cref="CompleteAsync"/> and <see cref="ReleaseAsync"/>
/// are a read followed by a write rather than an atomic compare. Replay of completed payloads is not affected. Use a store
/// built on Redis <c>SET NX</c> or a unique database index when concurrent duplicates must never run.
/// </para>
/// <para>
/// Entries are stored under <see cref="IdempotencyRecordKey.ToStorageKey"/> with <see cref="KeyPrefix"/> in a versioned
/// binary format. An entry that cannot be read (another format version, truncated or corrupt) is treated as absent.
/// </para>
/// </summary>
public sealed class DistributedCacheIdempotencyRecordStore : IIdempotencyRecordStore
{
    private readonly IDistributedCache _cache;

    /// <summary>Uses <see cref="IdempotencyRecordKey.DefaultPrefix"/>.</summary>
    public DistributedCacheIdempotencyRecordStore(IDistributedCache cache)
        : this(cache, IdempotencyRecordKey.DefaultPrefix)
    {
    }

    /// <param name="cache">The cache that holds the entries.</param>
    /// <param name="keyPrefix">Prefix of the cache keys, passed to <see cref="IdempotencyRecordKey.ToStorageKey"/>.</param>
    public DistributedCacheIdempotencyRecordStore(IDistributedCache cache, string keyPrefix)
    {
        Guard.NotNull(cache);
        Guard.NotNull(keyPrefix);
        _cache = cache;
        KeyPrefix = keyPrefix;
    }

    /// <summary>Prefix of the cache keys written by this store.</summary>
    public string KeyPrefix { get; }

    public async ValueTask<IdempotencyRecordReservation> TryReserveAsync(
        IdempotencyRecordKey key,
        string fingerprint,
        TimeSpan lease,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNull(key);
        Guard.NotNullOrEmpty(fingerprint);
        Guard.Positive(lease);

        string cacheKey = key.ToStorageKey(KeyPrefix);
        byte[]? data = await _cache.GetAsync(cacheKey, cancellationToken).ConfigureAwait(false);
        if (data is not null && RecordEntrySerializer.TryRead(data, out _, out string storedFingerprint, out byte[]? payload))
        {
            if (!string.Equals(storedFingerprint, fingerprint, StringComparison.Ordinal))
                return IdempotencyRecordReservation.FingerprintMismatch;
            return payload is null ? IdempotencyRecordReservation.InFlight : IdempotencyRecordReservation.Completed(payload);
        }

        string token = Guid.NewGuid().ToString("N");
        await _cache.SetAsync(
            cacheKey,
            RecordEntrySerializer.Write(token, fingerprint, default, completed: false),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = lease },
            cancellationToken).ConfigureAwait(false);
        return IdempotencyRecordReservation.Reserved(token);
    }

    public async ValueTask<bool> CompleteAsync(
        IdempotencyRecordKey key,
        string token,
        string fingerprint,
        ReadOnlyMemory<byte> payload,
        TimeSpan retention,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNull(key);
        Guard.NotNullOrEmpty(token);
        Guard.NotNullOrEmpty(fingerprint);
        Guard.Positive(retention);

        string cacheKey = key.ToStorageKey(KeyPrefix);
        byte[]? data = await _cache.GetAsync(cacheKey, cancellationToken).ConfigureAwait(false);
        if (data is not null
            && RecordEntrySerializer.TryRead(data, out string storedToken, out _, out byte[]? stored)
            && (stored is not null || !string.Equals(storedToken, token, StringComparison.Ordinal)))
            return false;

        await _cache.SetAsync(
            cacheKey,
            RecordEntrySerializer.Write(token, fingerprint, payload.Span, completed: true),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = retention },
            cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async ValueTask<bool> ReleaseAsync(IdempotencyRecordKey key, string token, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(key);
        Guard.NotNullOrEmpty(token);

        string cacheKey = key.ToStorageKey(KeyPrefix);
        byte[]? data = await _cache.GetAsync(cacheKey, cancellationToken).ConfigureAwait(false);
        if (data is null
            || !RecordEntrySerializer.TryRead(data, out string storedToken, out _, out byte[]? stored)
            || stored is not null
            || !string.Equals(storedToken, token, StringComparison.Ordinal))
            return false;

        await _cache.RemoveAsync(cacheKey, cancellationToken).ConfigureAwait(false);
        return true;
    }
}
