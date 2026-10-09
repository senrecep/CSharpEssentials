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
/// </summary>
public sealed class EfCoreTransactionRunner<TDbContext>(TDbContext dbContext) : ITransactionRunner
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
            return await ExecuteJoinedAsync(current, work, shouldCommit, cancellationToken).ConfigureAwait(false);

        IExecutionStrategy strategy = dbContext.Database.CreateExecutionStrategy();
        if (strategy.RetriesOnFailure && dbContext.ChangeTracker.HasChanges())
            throw new InvalidOperationException(
                $"{typeof(TDbContext).Name} has pending changes before the transaction started. A retry clears the change tracker and would drop them; make those changes inside the unit of work.");

        bool firstAttempt = true;
        return await strategy.ExecuteAsync(
            async token =>
            {
                if (!firstAttempt)
                    dbContext.ChangeTracker.Clear();
                firstAttempt = false;

                IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync(token).ConfigureAwait(false);
                await using ConfiguredAsyncDisposable transactionScope = transaction.ConfigureAwait(false);
                T result = await work(token).ConfigureAwait(false);
                if (shouldCommit(result))
                    await transaction.CommitAsync(token).ConfigureAwait(false);
                return result;
            },
            cancellationToken).ConfigureAwait(false);
    }

    private static async ValueTask<T> ExecuteJoinedAsync<T>(
        IDbContextTransaction current,
        Func<CancellationToken, ValueTask<T>> work,
        Func<T, bool> shouldCommit,
        CancellationToken cancellationToken)
    {
        if (!current.SupportsSavepoints)
            return await work(cancellationToken).ConfigureAwait(false);

        string savepoint = "cse_" + Guid.NewGuid().ToString("N");
        await current.CreateSavepointAsync(savepoint, cancellationToken).ConfigureAwait(false);
        T result;
        try
        {
            result = await work(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await TryRollbackToSavepointAsync(current, savepoint).ConfigureAwait(false);
            throw;
        }

        if (shouldCommit(result))
            await current.ReleaseSavepointAsync(savepoint, CancellationToken.None).ConfigureAwait(false);
        else
            await current.RollbackToSavepointAsync(savepoint, CancellationToken.None).ConfigureAwait(false);
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
