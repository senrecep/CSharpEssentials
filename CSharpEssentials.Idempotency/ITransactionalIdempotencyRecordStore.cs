namespace CSharpEssentials.Idempotency;

/// <summary>
/// An <see cref="IIdempotencyRecordStore"/> that writes in the caller's database transaction, so a record commits and
/// rolls back with the business data written next to it, and a concurrent request for the same key waits until that
/// transaction ends.
/// <para>
/// In <see cref="IdempotencyMode.Observe"/> a reservation that waited would block the business transaction and a
/// failed one could abort it, while Observe promises to change nothing. <see cref="IdempotencyExecutor"/> and the HTTP
/// middleware therefore reserve and release through <see cref="Detached"/> in Observe. A result to store releases the
/// detached reservation and is written through this store, in the caller's transaction, so Observe records only work
/// that committed, as Enforce does: a retry after the switch to <see cref="IdempotencyMode.Enforce"/> never replays a
/// result whose transaction rolled back. Between that write and the commit, a reservation of the same key waits for
/// the caller's transaction, as in Enforce.
/// </para>
/// </summary>
public interface ITransactionalIdempotencyRecordStore : IIdempotencyRecordStore
{
    /// <summary>
    /// The same records, written outside the caller's transaction: every call runs on its own connection and commits
    /// on its own. A record written through it is not atomic with the business data.
    /// </summary>
    IIdempotencyRecordStore Detached { get; }
}
