using Microsoft.EntityFrameworkCore;

namespace CSharpEssentials.EntityFrameworkCore.Outbox;

/// <summary>
/// Claims a batch of due outbox rows for one processor. A claim must, atomically per row: take only rows that are
/// pending (<see cref="OutboxMessage.ProcessedAt"/> and <see cref="OutboxMessage.DeadLetteredAt"/> are
/// <see langword="null"/>) and due (<see cref="OutboxMessage.NextAttemptAt"/> &lt;= <see cref="OutboxClaimRequest.Now"/>),
/// set <see cref="OutboxMessage.ClaimToken"/> to the request's token, set <see cref="OutboxMessage.NextAttemptAt"/> to
/// <see cref="OutboxClaimRequest.LeaseUntil"/> and add one to <see cref="OutboxMessage.Attempts"/>, and return the
/// claimed rows with those values. <see cref="ConditionalUpdateOutboxMessageClaimer"/> works on any relational provider;
/// <see cref="SqlOutboxMessageClaimer"/> runs provider-specific SQL such as PostgreSQL <c>FOR UPDATE SKIP LOCKED</c>.
/// </summary>
public interface IOutboxMessageClaimer
{
    /// <summary>Claims up to <see cref="OutboxClaimRequest.BatchSize"/> due rows of <paramref name="context"/>.</summary>
    Task<IReadOnlyList<OutboxMessage>> ClaimAsync(DbContext context, OutboxClaimRequest request, CancellationToken cancellationToken);
}
