namespace CSharpEssentials.EntityFrameworkCore.Outbox;

/// <summary>One claim of the outbox processor.</summary>
/// <param name="ClaimToken">Written to <see cref="OutboxMessage.ClaimToken"/> of every claimed row.</param>
/// <param name="BatchSize">The maximum number of rows to claim.</param>
/// <param name="Now">The current UTC time: rows whose <see cref="OutboxMessage.NextAttemptAt"/> is at or before it are due.</param>
/// <param name="LeaseUntil">The end of the lease, written to <see cref="OutboxMessage.NextAttemptAt"/> of every claimed row.</param>
public sealed record OutboxClaimRequest(Guid ClaimToken, int BatchSize, DateTime Now, DateTime LeaseUntil);
