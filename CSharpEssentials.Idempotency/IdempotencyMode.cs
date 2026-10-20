namespace CSharpEssentials.Idempotency;

/// <summary>How an idempotent operation uses its record. Rolling out a new idempotent operation in <see cref="Observe"/> first shows how often its keys collide before anything is blocked.</summary>
public enum IdempotencyMode
{
    /// <summary>The operation runs without touching the store.</summary>
    Off = 0,

    /// <summary>
    /// The key is reserved and the outcome stored as in <see cref="Enforce"/>, but nothing is blocked: a replay, an
    /// in-flight key or a reused key is logged as the decision <see cref="Enforce"/> would make, and the operation runs
    /// anyway. A record written in this mode is used once the mode becomes <see cref="Enforce"/>; an
    /// <see cref="ITransactionalIdempotencyRecordStore"/> writes it in the caller's transaction, so only committed work
    /// is replayed. It measures duplicate runs; it does not prevent them.
    /// </summary>
    Observe = 1,

    /// <summary>A stored outcome is replayed, and an in-flight or reused key stops the operation from running.</summary>
    Enforce = 2,
}
