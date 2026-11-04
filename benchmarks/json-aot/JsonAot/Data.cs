using CSharpEssentials.Json;
using JsonAotCore;

namespace JsonAot;

public sealed class Data
{
    [JsonPolymorphicProperty("values")]
    public List<IValue> Items { get; set; } = [];
}
