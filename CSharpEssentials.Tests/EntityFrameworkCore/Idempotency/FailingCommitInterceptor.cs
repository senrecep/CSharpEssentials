using System.Data.Common;

using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CSharpEssentials.Tests.EntityFrameworkCore.Idempotency;

/// <summary>
/// Fails every commit of the context it is added to, as a database that rejects the commit would: nothing is committed
/// and the transaction rolls back when it is disposed.
/// </summary>
internal sealed class FailingCommitInterceptor : DbTransactionInterceptor
{
    public override InterceptionResult TransactionCommitting(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result) =>
        throw new InvalidOperationException("The commit failed.");

    public override ValueTask<InterceptionResult> TransactionCommittingAsync(
        DbTransaction transaction, TransactionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("The commit failed.");
}
