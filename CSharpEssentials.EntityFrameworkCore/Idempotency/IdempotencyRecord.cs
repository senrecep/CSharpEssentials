using System.Security.Cryptography;
using System.Text;

using CSharpEssentials.Idempotency;

namespace CSharpEssentials.EntityFrameworkCore.Idempotency;

/// <summary>
/// One row of <see cref="EfCoreIdempotencyRecordStore{TContext}"/>: an in-flight reservation or a completed response,
/// keyed by <see cref="KeyHash"/>, the hash of <see cref="Namespace"/>, <see cref="Scope"/> and <see cref="Key"/>. Map it with
/// <see cref="IdempotencyRecordModelBuilderExtensions.AddIdempotencyRecords"/>. The store reads and writes these rows
/// with set-based SQL and never through the change tracker, so they are not meant to be edited by application code.
/// </summary>
public sealed class IdempotencyRecord
{
    /// <summary>The value of <see cref="Scope"/> for a global key, whose <see cref="IdempotencyRecordKey.Scope"/> is <see langword="null"/>.</summary>
    public const string GlobalScope = "global";

    /// <summary>
    /// The input prefix of <see cref="KeyHash"/>. It belongs to this table format: changing it, or the format of
    /// <see cref="IdempotencyRecordKey.ToStorageKey"/>, would orphan every stored record.
    /// </summary>
    internal const string KeyHashPrefix = "idem:efcore:v1:";

    /// <summary>The length of <see cref="KeyHash"/> in bytes.</summary>
    internal const int KeyHashLength = 32;

    /// <summary>
    /// The primary key: SHA-256 of the UTF-8 bytes of <see cref="IdempotencyRecordKey.ToStorageKey"/>, which prefixes
    /// every part with its length, so two different keys never share a hash input. The key is fixed-length binary, so
    /// uniqueness does not depend on the collation of the text columns: trailing spaces (<c>PAD SPACE</c>) and casing
    /// cannot merge two keys.
    /// </summary>
    public byte[] KeyHash { get; set; } = [];

    /// <summary>The caller that owns the key space (<see cref="IdempotencyRecordKey.Namespace"/>).</summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// The owner of the key (<see cref="IdempotencyRecordKey.Scope"/>), or <see cref="GlobalScope"/> for a global key.
    /// No scope built by <see cref="IdempotencyScope"/> equals <see cref="GlobalScope"/>, so the two never collide.
    /// </summary>
    public string Scope { get; set; } = string.Empty;

    /// <summary>The client's idempotency key (<see cref="IdempotencyRecordKey.Key"/>).</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>The token of the reservation that wrote the row; complete and release only change the row while it matches.</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>The fingerprint of the request that reserved or completed the key.</summary>
    public string Fingerprint { get; set; } = string.Empty;

    /// <summary>The stored response; <see langword="null"/> while the key is in flight. It may be empty.</summary>
    public byte[]? Payload { get; set; }

    /// <summary>
    /// When the lease (in flight) or the retention (completed) ends, in UTC. From then on the row counts as absent and
    /// <see cref="EfCoreIdempotencyRecordStore{TContext}.PurgeExpiredAsync"/> deletes it.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>The <see cref="KeyHash"/> of <paramref name="key"/>.</summary>
    /// <param name="key">The record key.</param>
    /// <returns>32 bytes.</returns>
    internal static byte[] HashKey(IdempotencyRecordKey key) =>
        SHA256.HashData(Encoding.UTF8.GetBytes(key.ToStorageKey(KeyHashPrefix)));
}
