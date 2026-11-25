using System.Runtime.CompilerServices;
using CSharpEssentials.Transactions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CSharpEssentials.EntityFrameworkCore.Transactions;

/// <summary>
/// <see cref="ITransactionRunner"/> backed by a <typeparamref name="TDbContext"/>.
/// <para>
/// Outermost call: runs inside <c>Database.CreateExecutionStrategy()</c> and opens a transaction with
/// <c>BeginTransactionAsync</c>; disposing it without a commit rolls it back. A retry re-runs the whole unit of
/// work on a cleared change tracker, so with a retrying strategy the context must have no pending changes on entry.
/// </para>
/// <para>
/// Nested call (<c>Database.CurrentTransaction</c> is set on the same context instance): joins that transaction
/// without a new execution strategy, begin or commit. When the provider supports savepoints, the nested work
/// runs behind a savepoint and is rolled back to it when it fails, so the outer call still decides the commit.
/// A different context or connection is not joined and is not atomic with this one.
/// </para>
/// <para>
/// <see cref="OnCommittedAsync"/> queues callbacks on the context for the outermost unit. They run in order after
/// the commit, once the execution strategy has returned, so a retried attempt never runs them. A rollback or a
/// retry drops them, and a nested unit that rolls back to its savepoint drops the ones it queued. When callbacks
/// throw, the others still run and an <see cref="AggregateException"/> is thrown after the commit. They get
/// <see cref="CancellationToken.None"/>, not the caller's token: the data is durable, so a request that is cancelled
/// right after the commit must not drop the side effects.
/// </para>
/// </summary>
public sealed class EfCoreTransactionRunner<TDbContext>(TDbContext dbContext) : ITransactionRunner, ITransactionCallbacks
    where TDbContext : DbContext
{
    public async ValueTask<T> ExecuteAsync<T>(
        Func<CancellationToken, ValueTask<T>> work,
        Func<T, bool> shouldCommit,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(work);
        ArgumentNullException.ThrowIfNull(shouldCommit);

        IDbContextTransaction? current = dbContext.Database.CurrentTransaction;
        if (current is not null)
            return await ExecuteJoinedAsync(current, AfterCommitCallbacks.Find(dbContext), work, shouldCommit, cancellationToken).ConfigureAwait(false);

        IExecutionStrategy strategy = dbContext.Database.CreateExecutionStrategy();
        if (strategy.RetriesOnFailure && dbContext.ChangeTracker.HasChanges())
            throw new InvalidOperationException(
                $"{typeof(TDbContext).Name} has pending changes before the transaction started. A retry clears the change tracker and would drop them; make those changes inside the unit of work.");

        bool firstAttempt = true;
        bool committed = false;
        List<Func<CancellationToken, ValueTask>> callbacks = [];
        T outcome;
        try
        {
            outcome = await strategy.ExecuteAsync(
                async token =>
                {
                    if (!firstAttempt)
                        dbContext.ChangeTracker.Clear();
                    firstAttempt = false;
                    committed = false;
                    callbacks = AfterCommitCallbacks.Begin(dbContext);

                    IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync(token).ConfigureAwait(false);
                    await using ConfiguredAsyncDisposable transactionScope = transaction.ConfigureAwait(false);
                    T result = await work(token).ConfigureAwait(false);
                    if (shouldCommit(result))
                    {
                        await transaction.CommitAsync(token).ConfigureAwait(false);
                        committed = true;
                    }
                    return result;
                },
                cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            AfterCommitCallbacks.End(dbContext);
        }

        if (committed && callbacks.Count > 0)
            await AfterCommitCallbacks.RunAsync(callbacks, CancellationToken.None).ConfigureAwait(false);
        return outcome;
    }

    /// <summary>
    /// Queues <paramref name="callback"/> to run after the outermost commit of the unit running on this context,
    /// or runs it now when no transaction is open. Throws <see cref="InvalidOperationException"/> when a
    /// transaction this runner did not start, or an ambient <c>TransactionScope</c>, is open.
    /// </summary>
    public ValueTask OnCommittedAsync(Func<CancellationToken, ValueTask> callback, CancellationToken cancellationToken = default) =>
        AfterCommitCallbacks.RegisterAsync(dbContext, callback, cancellationToken);

    private static async ValueTask<T> ExecuteJoinedAsync<T>(
        IDbContextTransaction current,
        List<Func<CancellationToken, ValueTask>>? callbacks,
        Func<CancellationToken, ValueTask<T>> work,
        Func<T, bool> shouldCommit,
        CancellationToken cancellationToken)
    {
        if (!current.SupportsSavepoints)
            return await work(cancellationToken).ConfigureAwait(false);

        int queued = callbacks?.Count ?? 0;
        string savepoint = "cse_" + Guid.NewGuid().ToString("N");
        await current.CreateSavepointAsync(savepoint, cancellationToken).ConfigureAwait(false);
        T result;
        try
        {
            result = await work(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            AfterCommitCallbacks.Truncate(callbacks, queued);
            await TryRollbackToSavepointAsync(current, savepoint).ConfigureAwait(false);
            throw;
        }

        if (shouldCommit(result))
        {
            await current.ReleaseSavepointAsync(savepoint, CancellationToken.None).ConfigureAwait(false);
        }
        else
        {
            AfterCommitCallbacks.Truncate(callbacks, queued);
            await current.RollbackToSavepointAsync(savepoint, CancellationToken.None).ConfigureAwait(false);
        }
        return result;
    }

    private static async Task TryRollbackToSavepointAsync(IDbContextTransaction transaction, string savepoint)
    {
        try
        {
            await transaction.RollbackToSavepointAsync(savepoint, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception)
        {
            // The work's own exception is the one to surface (and to classify for retries); a failed
            // rollback here means the connection is gone and the outer transaction rolls back anyway.
        }
    }
}
