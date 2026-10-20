namespace CSharpEssentials.Idempotency;

/// <summary>
/// An <see cref="IIdempotencyRecordStore"/> that writes in the caller's database transaction, so a record commits and
/// rolls back with the business data written next to it, and a concurrent request for the same key waits until that
/// transaction ends.
/// <para>
/// In <see cref="IdempotencyMode.Observe"/> that wait would block the business transaction and a failed write could
/// abort it, while Observe promises to change nothing. Callers in Observe use <see cref="Detached"/> instead:
/// <see cref="IdempotencyExecutor"/> and the HTTP middleware switch to it on their own.
/// </para>
/// </summary>
public interface ITransactionalIdempotencyRecordStore : IIdempotencyRecordStore
{
    /// <summary>
    /// The same records, written outside the caller's transaction: every call runs on its own connection and commits
    /// on its own, and a reservation never waits for an uncommitted one. A record written through it is not atomic
    /// with the business data.
    /// </summary>
    IIdempotencyRecordStore Detached { get; }
}
