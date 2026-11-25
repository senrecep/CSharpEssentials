namespace CSharpEssentials.Transactions;

/// <summary>
/// Registers work that must run only after the outermost transaction of the current unit of work commits,
/// such as publishing a message, waking an outbox relay or evicting a cache entry.
/// Kept apart from <see cref="ITransactionRunner"/> so existing runner implementations stay valid.
/// </summary>
public interface ITransactionCallbacks
{
    /// <summary>
    /// Inside a unit of work run by the runner, queues <paramref name="callback"/> to run after the outermost
    /// commit and returns at once. A rollback, a retry of the unit or a rolled-back nested unit drops the
    /// callbacks it registered. With no transaction open, runs <paramref name="callback"/> now.
    /// A queued callback receives <see cref="CancellationToken.None"/>, because the data is already committed and a
    /// cancelled request must not drop the side effect; a callback that runs now receives <paramref name="cancellationToken"/>.
    /// Implementations throw <see cref="InvalidOperationException"/> when a transaction they do not manage is open,
    /// an ambient <c>TransactionScope</c> included, because they cannot tell when it commits.
    /// </summary>
    ValueTask OnCommittedAsync(Func<CancellationToken, ValueTask> callback, CancellationToken cancellationToken = default);
}
