using CSharpEssentials.Entity.Interfaces;

namespace CSharpEssentials.EntityFrameworkCore.Outbox;

/// <summary>
/// Delivers one outbox message, for example by publishing it to a broker or calling in-process handlers. The outbox
/// processor calls it at least once per message: after a crash or a lost completion the same message is delivered
/// again, so use <see cref="OutboxMessage.Id"/> as the dedupe key downstream. Throwing marks the attempt as failed.
/// </summary>
public interface IOutboxMessageHandler
{
    /// <summary>Delivers <paramref name="domainEvent"/>, read from <paramref name="message"/>.</summary>
    /// <param name="message">The claimed row; <see cref="OutboxMessage.Attempts"/> counts this attempt.</param>
    /// <param name="domainEvent">The deserialized event.</param>
    /// <param name="cancellationToken">Cancels the delivery.</param>
    Task HandleAsync(OutboxMessage message, IDomainEvent domainEvent, CancellationToken cancellationToken);
}
