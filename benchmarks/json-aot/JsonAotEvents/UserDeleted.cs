using CSharpEssentials.Json;
using JsonAotCore;

namespace JsonAotEvents;

[JsonTypeDiscriminator("user.deleted.v1")]
public sealed record UserDeleted(Guid Id) : IEvent;
