using CSharpEssentials.Json;
using JsonAotCore;

namespace JsonAot;

[JsonTypeDiscriminator("user.deleted.v1")]
public sealed record UserDeleted(Guid Id) : IEvent;
