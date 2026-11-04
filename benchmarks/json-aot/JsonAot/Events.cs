using CSharpEssentials.Json;
using JsonAotCore;

namespace JsonAot;

[JsonTypeDiscriminator("user.created.v1")]
public sealed record UserCreated(Guid Id, string Name) : IEvent;

[JsonTypeDiscriminator("user.deleted.v1")]
public sealed record UserDeleted(Guid Id) : IEvent;

[JsonTypeDiscriminator("text")]
public sealed record TextValue(string Text) : IValue;

[JsonTypeDiscriminator("number")]
public sealed record NumberValue(decimal Number) : IValue;

public sealed class Data
{
    [JsonPolymorphicProperty("values")]
    public List<IValue> Items { get; set; } = [];
}
