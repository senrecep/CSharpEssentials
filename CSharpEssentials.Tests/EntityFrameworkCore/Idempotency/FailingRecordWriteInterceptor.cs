using System.Data.Common;

using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CSharpEssentials.Tests.EntityFrameworkCore.Idempotency;

/// <summary>
/// Turns every write of the context it is added to that touches the idempotency records into a statement that fails on
/// the server, as a lock timeout or any other database error would: on PostgreSQL the failure aborts the transaction the
/// write runs in.
/// </summary>
internal sealed class FailingRecordWriteInterceptor : DbCommandInterceptor
{
    public override InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
    {
        Fail(command);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Fail(command);
        return ValueTask.FromResult(result);
    }

    private static void Fail(DbCommand command)
    {
        if (command.CommandText.Contains("IdempotencyRecords", StringComparison.Ordinal))
            command.CommandText = "SELECT 1/0";
    }
}
