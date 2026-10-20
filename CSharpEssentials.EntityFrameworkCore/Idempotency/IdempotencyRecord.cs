using CSharpEssentials.Idempotency;

namespace CSharpEssentials.EntityFrameworkCore.Idempotency;

/// <summary>
/// One row of <see cref="EfCoreIdempotencyRecordStore{TContext}"/>: an in-flight reservation or a completed response,
/// keyed by <see cref="Namespace"/>, <see cref="Scope"/> and <see cref="Key"/>. Map it with
/// <see cref="IdempotencyRecordModelBuilderExtensions.AddIdempotencyRecords"/>. The store reads and writes these rows
/// with set-based SQL and never through the change tracker, so they are not meant to be edited by application code.
/// </summary>
public sealed class IdempotencyRecord
{
    /// <summary>The value of <see cref="Scope"/> for a global key, whose <see cref="IdempotencyRecordKey.Scope"/> is <see langword="null"/>.</summary>
    public const string GlobalScope = "global";

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
}
