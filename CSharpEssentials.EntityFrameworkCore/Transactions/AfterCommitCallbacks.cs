using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore;

namespace CSharpEssentials.EntityFrameworkCore.Transactions;

internal static class AfterCommitCallbacks
{
    private static readonly ConditionalWeakTable<DbContext, List<Func<CancellationToken, ValueTask>>> Pending = new();

    internal static List<Func<CancellationToken, ValueTask>>? Find(DbContext context) =>
        Pending.TryGetValue(context, out List<Func<CancellationToken, ValueTask>>? callbacks) ? callbacks : null;

    internal static List<Func<CancellationToken, ValueTask>> Begin(DbContext context)
    {
        List<Func<CancellationToken, ValueTask>> callbacks = [];
        Pending.AddOrUpdate(context, callbacks);
        return callbacks;
    }

    internal static void End(DbContext context) => Pending.Remove(context);

    internal static void Truncate(List<Func<CancellationToken, ValueTask>>? callbacks, int count)
    {
        if (callbacks is not null && callbacks.Count > count)
            callbacks.RemoveRange(count, callbacks.Count - count);
    }

    internal static bool TryDefer(DbContext context, Func<CancellationToken, ValueTask> callback)
    {
        List<Func<CancellationToken, ValueTask>>? callbacks = Find(context);
        if (callbacks is null)
            return false;
        callbacks.Add(callback);
        return true;
    }

    internal static async ValueTask RegisterAsync(
        DbContext context, Func<CancellationToken, ValueTask> callback, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (TryDefer(context, callback))
            return;
        if (context.Database.CurrentTransaction is not null || System.Transactions.Transaction.Current is not null)
            throw new InvalidOperationException(
                $"{context.GetType().Name} is in a transaction that was not started by EfCoreTransactionRunner (or in an ambient TransactionScope), so the commit cannot be observed. Run the work through the runner.");
        await callback(cancellationToken).ConfigureAwait(false);
    }

    internal static async ValueTask RunAsync(List<Func<CancellationToken, ValueTask>> callbacks, CancellationToken cancellationToken)
    {
        List<Exception>? failures = null;
        foreach (Func<CancellationToken, ValueTask> callback in callbacks)
        {
            try
            {
                await callback(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                (failures ??= []).Add(exception);
            }
        }

        if (failures is not null)
            throw new AggregateException("One or more after-commit callbacks failed. The transaction is committed.", failures);
    }
}
