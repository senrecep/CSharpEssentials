using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace CSharpEssentials.Json;

/// <summary>
/// Writes <typeparamref name="T"/> with System.Text.Json's polymorphism and reads both the current discriminators and the full CLR
/// names of the derived types.
/// </summary>
internal sealed class ClrTypeNameConverter<T> : JsonConverter<T>, IClrTypeNameConverter
{
    private readonly ConditionalWeakTable<JsonSerializerOptions, JsonTypeInfo<T>> _typeInfos = [];

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonTypeInfo<T> typeInfo = GetTypeInfo(options);
        JsonPolymorphismOptions polymorphism = typeInfo.PolymorphismOptions!;
        string propertyName = polymorphism.TypeDiscriminatorPropertyName;
        if (reader.TokenType == JsonTokenType.StartObject &&
            ReadDiscriminator(reader, propertyName) is { } value &&
            FindByClrName(polymorphism, value) is { } derived)
        {
            JsonObject node = JsonNode.Parse(ref reader)!.AsObject();
            node[propertyName] = derived.TypeDiscriminator is int number ? JsonValue.Create(number) : JsonValue.Create((string)derived.TypeDiscriminator!);
            return node.Deserialize(typeInfo);
        }

        return JsonSerializer.Deserialize(ref reader, typeInfo);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value, GetTypeInfo(options));

    private JsonTypeInfo<T> GetTypeInfo(JsonSerializerOptions options) => _typeInfos.GetValue(options, static source =>
    {
        JsonSerializerOptions shadow = new(source);
        for (int i = shadow.Converters.Count - 1; i >= 0; i--)
        {
            if (shadow.Converters[i] is ClrTypeNameConverterFactory)
                shadow.Converters.RemoveAt(i);
        }

#if NET11_0_OR_GREATER
        return shadow.GetTypeInfo<T>();
#else
        return (JsonTypeInfo<T>)shadow.GetTypeInfo(typeof(T));
#endif
    });

    private static string? ReadDiscriminator(Utf8JsonReader probe, string propertyName)
    {
        int depth = probe.CurrentDepth;
        while (probe.Read() && probe.CurrentDepth > depth)
        {
            if (probe.TokenType != JsonTokenType.PropertyName || probe.CurrentDepth != depth + 1 || !probe.ValueTextEquals(propertyName))
                continue;

            probe.Read();
            return probe.TokenType == JsonTokenType.String ? probe.GetString() : null;
        }

        return null;
    }

    private static JsonDerivedType? FindByClrName(JsonPolymorphismOptions polymorphism, string value)
    {
        JsonDerivedType? match = null;
        foreach (JsonDerivedType derived in polymorphism.DerivedTypes)
        {
            if (derived.TypeDiscriminator is string current && string.Equals(current, value, StringComparison.Ordinal))
                return null;

            if (match is null && string.Equals(derived.DerivedType.FullName, value, StringComparison.Ordinal))
                match = derived;
        }

        return match;
    }
}
