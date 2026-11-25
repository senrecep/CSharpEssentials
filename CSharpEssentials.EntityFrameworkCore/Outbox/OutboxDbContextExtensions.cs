using Microsoft.EntityFrameworkCore;

namespace CSharpEssentials.EntityFrameworkCore.Outbox;

/// <summary>Maintenance of the <see cref="OutboxMessage"/> table.</summary>
public static class OutboxDbContextExtensions
{
    /// <summary>
    /// Deletes, in one <c>DELETE</c> statement, the processed rows whose <see cref="OutboxMessage.ProcessedAt"/> is before
    /// <paramref name="processedBefore"/>. The processor never deletes rows, so call this from a job to keep the table
    /// small. Dead-lettered and pending rows are kept.
    /// </summary>
    /// <param name="context">A context that maps <see cref="OutboxMessage"/>.</param>
    /// <param name="processedBefore">The UTC cutoff; rows processed at or after it are kept.</param>
    /// <param name="cancellationToken">Cancels the statement.</param>
    /// <returns>The number of deleted rows.</returns>
    public static Task<int> PurgeProcessedOutboxMessagesAsync(
        this DbContext context, DateTime processedBefore, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Set<OutboxMessage>()
            .Where(m => m.ProcessedAt != null && m.ProcessedAt < processedBefore)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
