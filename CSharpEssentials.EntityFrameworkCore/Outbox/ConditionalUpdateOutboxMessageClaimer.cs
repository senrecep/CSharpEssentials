using Microsoft.EntityFrameworkCore;

namespace CSharpEssentials.EntityFrameworkCore.Outbox;

/// <summary>
/// The provider-neutral <see cref="IOutboxMessageClaimer"/>: it reads the ids of the oldest due rows, then claims them
/// with one conditional <c>UPDATE</c> (<c>ExecuteUpdate</c>) that repeats the due condition, so a row another
/// processor claimed in between is skipped, and reads back the rows that carry its token. Concurrent processors
/// never claim the same row, but they may compete for the same candidates and claim fewer rows than the batch size;
/// use <see cref="SqlOutboxMessageClaimer"/> with a provider dialect for many processors on a busy table.
/// </summary>
public sealed class ConditionalUpdateOutboxMessageClaimer : IOutboxMessageClaimer
{
    /// <summary>The shared instance.</summary>
    public static ConditionalUpdateOutboxMessageClaimer Instance { get; } = new();

    /// <inheritdoc />
    public async Task<IReadOnlyList<OutboxMessage>> ClaimAsync(DbContext context, OutboxClaimRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(request);

        DbSet<OutboxMessage> messages = context.Set<OutboxMessage>();
        DateTime now = request.Now;
        Guid[] candidates = await messages
            .Where(m => m.ProcessedAt == null && m.DeadLetteredAt == null && m.NextAttemptAt <= now)
            .OrderBy(m => m.OccurredAt)
            .ThenBy(m => m.Id)
            .Select(m => m.Id)
            .Take(request.BatchSize)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        if (candidates.Length == 0)
            return [];

        Guid token = request.ClaimToken;
        DateTime leaseUntil = request.LeaseUntil;
        int claimed = await messages
            .Where(m => candidates.Contains(m.Id) && m.ProcessedAt == null && m.DeadLetteredAt == null && m.NextAttemptAt <= now)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(m => m.ClaimToken, token)
                    .SetProperty(m => m.NextAttemptAt, leaseUntil)
                    .SetProperty(m => m.Attempts, m => m.Attempts + 1),
                cancellationToken)
            .ConfigureAwait(false);
        if (claimed == 0)
            return [];

        return await messages
            .AsNoTracking()
            .Where(m => m.ClaimToken == token)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
