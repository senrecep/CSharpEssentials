using System.Text.Json;

namespace CSharpEssentials.Any;

internal sealed class AnyJsonValue(JsonElement element, JsonSerializerOptions options)
{
    public JsonElement Element { get; } = element;
    public JsonSerializerOptions Options { get; } = options;
}
