using CSharpEssentials.Idempotency;
using StackExchange.Redis;

namespace Examples.Idempotency.Redis;

/// <summary>
/// <see cref="IIdempotencyRecordStore"/> on Redis. Every operation is one Lua script on one key, so it is atomic and
/// works on Redis Cluster. An entry is a hash: <c>t</c> (token), <c>f</c> (fingerprint) and, once completed,
/// <c>p</c> (payload). Leases and retention are Redis TTLs (<c>PEXPIRE</c>), so an expired entry is simply absent.
/// </summary>
public sealed class RedisIdempotencyRecordStore : IIdempotencyRecordStore
{
    private const string ReserveScript = """
        local entry = redis.call('HMGET', KEYS[1], 't', 'f', 'p')
        if not entry[1] then
          redis.call('HSET', KEYS[1], 't', ARGV[1], 'f', ARGV[2])
          redis.call('PEXPIRE', KEYS[1], ARGV[3])
          return { 0 }
        end
        if entry[2] ~= ARGV[2] then
          return { 3 }
        end
        if not entry[3] then
          return { 1 }
        end
        return { 2, entry[3] }
        """;

    private const string CompleteScript = """
        local entry = redis.call('HMGET', KEYS[1], 't', 'p')
        if entry[1] and (entry[2] or entry[1] ~= ARGV[1]) then
          return 0
        end
        redis.call('HSET', KEYS[1], 't', ARGV[1], 'f', ARGV[2], 'p', ARGV[3])
        redis.call('PEXPIRE', KEYS[1], ARGV[4])
        return 1
        """;

    private const string ReleaseScript = """
        local entry = redis.call('HMGET', KEYS[1], 't', 'p')
        if entry[1] == ARGV[1] and not entry[2] then
          redis.call('DEL', KEYS[1])
          return 1
        end
        return 0
        """;

    private readonly IConnectionMultiplexer _connection;

    /// <summary>Uses <see cref="IdempotencyRecordKey.DefaultPrefix"/>.</summary>
    public RedisIdempotencyRecordStore(IConnectionMultiplexer connection)
        : this(connection, IdempotencyRecordKey.DefaultPrefix)
    {
    }

    /// <param name="connection">The application's Redis connection.</param>
    /// <param name="keyPrefix">Prefix of the Redis keys, passed to <see cref="IdempotencyRecordKey.ToStorageKey"/>.</param>
    public RedisIdempotencyRecordStore(IConnectionMultiplexer connection, string keyPrefix)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(keyPrefix);
        _connection = connection;
        KeyPrefix = keyPrefix;
    }

    /// <summary>Prefix of the Redis keys written by this store.</summary>
    public string KeyPrefix { get; }

    /// <inheritdoc />
    public async ValueTask<IdempotencyRecordReservation> TryReserveAsync(
        IdempotencyRecordKey key,
        string fingerprint,
        TimeSpan lease,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentException.ThrowIfNullOrEmpty(fingerprint);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(lease, TimeSpan.Zero);
        cancellationToken.ThrowIfCancellationRequested();

        string token = Guid.NewGuid().ToString("N");
        RedisResult result = await _connection.GetDatabase().ScriptEvaluateAsync(
            ReserveScript,
            [key.ToStorageKey(KeyPrefix)],
            [token, fingerprint, Milliseconds(lease)]).ConfigureAwait(false);

        return (int)result[0] switch
        {
            0 => IdempotencyRecordReservation.Reserved(token),
            1 => IdempotencyRecordReservation.InFlight,
            2 => IdempotencyRecordReservation.Completed((byte[]?)result[1] ?? []),
            _ => IdempotencyRecordReservation.FingerprintMismatch,
        };
    }

    /// <inheritdoc />
    public async ValueTask<bool> CompleteAsync(
        IdempotencyRecordKey key,
        string token,
        string fingerprint,
        ReadOnlyMemory<byte> payload,
        TimeSpan retention,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentException.ThrowIfNullOrEmpty(token);
        ArgumentException.ThrowIfNullOrEmpty(fingerprint);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(retention, TimeSpan.Zero);
        cancellationToken.ThrowIfCancellationRequested();

        RedisResult result = await _connection.GetDatabase().ScriptEvaluateAsync(
            CompleteScript,
            [key.ToStorageKey(KeyPrefix)],
            [token, fingerprint, payload.ToArray(), Milliseconds(retention)]).ConfigureAwait(false);
        return (int)result == 1;
    }

    /// <inheritdoc />
    public async ValueTask<bool> ReleaseAsync(IdempotencyRecordKey key, string token, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentException.ThrowIfNullOrEmpty(token);
        cancellationToken.ThrowIfCancellationRequested();

        RedisResult result = await _connection.GetDatabase().ScriptEvaluateAsync(
            ReleaseScript,
            [key.ToStorageKey(KeyPrefix)],
            [token]).ConfigureAwait(false);
        return (int)result == 1;
    }

    /// <summary>Whole milliseconds for <c>PEXPIRE</c>, rounded up so a positive span never becomes 0.</summary>
    private static long Milliseconds(TimeSpan duration) => Math.Max(1L, (long)Math.Ceiling(duration.TotalMilliseconds));
}
