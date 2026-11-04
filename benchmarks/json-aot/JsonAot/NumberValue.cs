using CSharpEssentials.Json;
using JsonAotCore;

namespace JsonAot;

[JsonTypeDiscriminator("number")]
public sealed record NumberValue(decimal Number) : IValue;
