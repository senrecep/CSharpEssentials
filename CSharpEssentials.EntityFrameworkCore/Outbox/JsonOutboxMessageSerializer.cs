using System.Text.Json;
using CSharpEssentials.Entity.Interfaces;

namespace CSharpEssentials.EntityFrameworkCore.Outbox;

/// <summary>
/// The default <see cref="IOutboxMessageSerializer"/>: System.Text.Json over the metadata of an
/// <see cref="OutboxEventTypeRegistry"/>. It serializes with the registered <c>JsonTypeInfo</c>, so it is trim and AOT
/// safe when every type is registered with source-generated metadata.
/// </summary>
/// <param name="registry">The registered event types.</param>
public sealed class JsonOutboxMessageSerializer(OutboxEventTypeRegistry registry) : IOutboxMessageSerializer
{
    /// <inheritdoc />
    public OutboxPayload Serialize(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        (string name, System.Text.Json.Serialization.Metadata.JsonTypeInfo typeInfo) = registry.GetByType(domainEvent.GetType());
        return new OutboxPayload(name, JsonSerializer.Serialize(domainEvent, typeInfo));
    }

    /// <inheritdoc />
    public IDomainEvent Deserialize(string type, string content)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(content);
        return JsonSerializer.Deserialize(content, registry.GetByName(type)) as IDomainEvent
            ?? throw new InvalidOperationException($"The outbox content of type '{type}' is null.");
    }
}
