namespace CSharpEssentials.Mediator;

/// <summary>Where <see cref="LockBehavior{TRequest, TResponse}"/> runs relative to the transaction behavior.</summary>
public enum LockPlacement
{
    /// <summary>
    /// Lock → transaction → handler. The lock is held until the transaction has committed or rolled back, so the next
    /// waiter sees the committed data. Use it with process or session locks, such as <see cref="Locking.InProcessResourceLock"/>.
    /// </summary>
    OutsideTransaction = 0,

    /// <summary>
    /// Transaction → lock → handler. Required for locks that belong to the transaction, such as PostgreSQL
    /// <c>pg_advisory_xact_lock</c>. With a process or session lock this releases the lock before the commit, so the
    /// next waiter can read data from before it.
    /// </summary>
    InsideTransaction = 1,
}
