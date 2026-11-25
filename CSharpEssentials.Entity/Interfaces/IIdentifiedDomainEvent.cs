namespace CSharpEssentials.Entity.Interfaces;

/// <summary>
/// A domain event that carries its own identity and creation time. An outbox stores <see cref="EventId"/> as the
/// dedupe key of the message and orders delivery by <see cref="OccurredAt"/>. Derive from <see cref="DomainEvent"/> to
/// have both filled for you.
/// </summary>
public interface IIdentifiedDomainEvent : IDomainEvent
{
    /// <summary>The unique id of this event; stays the same when the event is serialized and read back.</summary>
    Guid EventId { get; }

    /// <summary>When the event was raised, in UTC.</summary>
    DateTimeOffset OccurredAt { get; }
}
