using CSharpEssentials.Json;
using JsonAotCore;

namespace JsonAot;

[JsonTypeDiscriminator("text")]
public sealed record TextValue(string Text) : IValue;
