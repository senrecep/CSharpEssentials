using CSharpEssentials.Core;
using CSharpEssentials.Entity.Interfaces;
using CSharpEssentials.EntityFrameworkCore.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace CSharpEssentials.EntityFrameworkCore.Outbox;

/// <summary>
/// <see cref="IDomainEventOutbox"/> that adds one <see cref="OutboxMessage"/> per event to the saving context, so
/// the rows are written by the same <c>SaveChanges</c>, in the same transaction, as the entities that raised the
/// events. The context must map <see cref="OutboxMessage"/> (<see cref="OutboxModelBuilderExtensions.AddOutboxMessages"/>).
/// Register it with <see cref="OutboxServiceCollectionExtensions.AddEfCoreOutbox"/>.
/// </summary>
/// <param name="serializer">Serializes the events.</param>
/// <param name="timeProvider">Stamps events that carry no time; <see cref="TimeProvider.System"/> when <see langword="null"/>.</param>
public sealed class EfCoreDomainEventOutbox(IOutboxMessageSerializer serializer, TimeProvider? timeProvider = null) : IDomainEventOutbox
{
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">The context does not map <see cref="OutboxMessage"/>, or an event type is unknown to the serializer.</exception>
    public Task StoreAsync(DbContext context, IReadOnlyList<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(domainEvents);
        if (context.Model.FindEntityType(typeof(OutboxMessage)) is null)
            throw new InvalidOperationException(
                $"{context.GetType().Name} does not map {nameof(OutboxMessage)}. Call modelBuilder.{nameof(OutboxModelBuilderExtensions.AddOutboxMessages)}() in OnModelCreating.");

        var messages = new OutboxMessage[domainEvents.Count];
        DateTimeOffset now = _timeProvider.GetUtcNow();
        for (int i = 0; i < messages.Length; i++)
        {
            IDomainEvent domainEvent = domainEvents[i];
            OutboxPayload payload = serializer.Serialize(domainEvent);
            (Guid id, DateTimeOffset occurredAt) = domainEvent is IIdentifiedDomainEvent identified
                ? (identified.EventId, identified.OccurredAt)
                : (Guider.NewGuid(now), now);
            messages[i] = new OutboxMessage
            {
                Id = id,
                Type = payload.Type,
                Content = payload.Content,
                OccurredAt = occurredAt.UtcDateTime,
                NextAttemptAt = now.UtcDateTime,
            };
        }

        context.Set<OutboxMessage>().AddRange(messages);
        return Task.CompletedTask;
    }
}
