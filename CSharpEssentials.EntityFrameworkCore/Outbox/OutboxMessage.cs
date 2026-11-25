namespace CSharpEssentials.EntityFrameworkCore.Outbox;

/// <summary>
/// One stored domain event. <see cref="EfCoreDomainEventOutbox"/> adds a row in the <c>SaveChanges</c> that raised the
/// event, and the outbox processor claims, dispatches and completes it. Map it with
/// <see cref="OutboxModelBuilderExtensions.AddOutboxMessages"/>. A row is pending while <see cref="ProcessedAt"/> and
/// <see cref="DeadLetteredAt"/> are both <see langword="null"/>; times are UTC.
/// </summary>
public sealed class OutboxMessage
{
    /// <summary>
    /// The message id and the dedupe key a handler should use: the <c>EventId</c> of an
    /// <c>IIdentifiedDomainEvent</c>, otherwise a new version 7 GUID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>The stable type name of the event, as the <see cref="IOutboxMessageSerializer"/> wrote it.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>The serialized event.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>When the event was raised: the <c>OccurredAt</c> of an <c>IIdentifiedDomainEvent</c>, otherwise when it was stored. Delivery follows this order.</summary>
    public DateTime OccurredAt { get; set; }

    /// <summary>
    /// The earliest time the row may be claimed. A claim moves it to the end of the lease, so a row whose worker
    /// stopped is claimed again once the lease ends; a failure moves it to the time of the next retry.
    /// </summary>
    public DateTime NextAttemptAt { get; set; }

    /// <summary>How many times the row has been claimed for dispatch.</summary>
    public int Attempts { get; set; }

    /// <summary>The token of the claim that holds the row; completion and failure only change the row while it matches.</summary>
    public Guid? ClaimToken { get; set; }

    /// <summary>When the handler completed the message; <see langword="null"/> while it is pending or dead-lettered.</summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>When the message gave up after <c>MaxAttempts</c> attempts; <see langword="null"/> otherwise. Dead-lettered rows are kept for inspection.</summary>
    public DateTime? DeadLetteredAt { get; set; }

    /// <summary>The error of the last failed attempt; <see langword="null"/> when there is none.</summary>
    public string? Error { get; set; }
}
