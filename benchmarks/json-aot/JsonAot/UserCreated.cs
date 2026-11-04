using CSharpEssentials.Json;
using JsonAotCore;

namespace JsonAot;

[JsonTypeDiscriminator("user.created.v1")]
public sealed record UserCreated(Guid Id, string Name) : IEvent;
