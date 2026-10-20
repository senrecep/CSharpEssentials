namespace CSharpEssentials.Mediator;

/// <summary>Where <see cref="IdempotencyBehavior{TRequest, TResponse}"/> runs relative to the transaction behavior.</summary>
public enum IdempotencyPlacement
{
    /// <summary>
    /// Idempotency → lock → transaction → handler. The record is written after the transaction has committed or rolled
    /// back, in a store of its own. A failed <see cref="ITransactionalRequest"/> releases its key, because the
    /// transaction rolled back. If the process stops between the commit and the write of the record, the key stays in
    /// flight until its lease expires and a retry after that runs the handler again.
    /// </summary>
    OutsideTransaction = 0,

    /// <summary>
    /// Transaction → idempotency → lock → handler. Use it with a record store that enlists in the same transaction, so the
    /// record commits or rolls back with the handler's data. With a store that does not enlist, a commit that fails after
    /// the record was written leaves a stored response for work that never committed, and a retry replays it.
    /// </summary>
    InsideTransaction = 1,
}
