using CSharpEssentials.Entity.Interfaces;

namespace CSharpEssentials.EntityFrameworkCore.Outbox;

/// <summary>
/// Turns domain events into outbox rows and back. <see cref="JsonOutboxMessageSerializer"/> is the default; register
/// your own to use another format or type naming.
/// </summary>
public interface IOutboxMessageSerializer
{
    /// <summary>Serializes <paramref name="domainEvent"/>.</summary>
    /// <exception cref="InvalidOperationException">The event type is not known to the serializer.</exception>
    OutboxPayload Serialize(IDomainEvent domainEvent);

    /// <summary>Reads back an event that <see cref="Serialize"/> wrote.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="type"/> is not known to the serializer.</exception>
    IDomainEvent Deserialize(string type, string content);
}
