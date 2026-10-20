namespace CSharpEssentials.Idempotency;

/// <summary>What <see cref="IIdempotencyExecutor"/> does with the result of an operation that ran.</summary>
public enum IdempotencyResultHandling
{
    /// <summary>The result is stored and replayed for the retention of the operation.</summary>
    Store = 0,

    /// <summary>
    /// The result is a failure that is stored and replayed for the shorter failure retention; after it expires a retry
    /// runs the operation again.
    /// </summary>
    StoreAsFailure = 1,

    /// <summary>
    /// Nothing is stored and the key is released, so a retry with it runs the operation again. Use it for an outcome
    /// whose side effects were undone, for example a transaction that rolled back.
    /// </summary>
    Release = 2,
}
