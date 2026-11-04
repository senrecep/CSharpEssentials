using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace CSharpEssentials.Json;

/// <summary>
/// Writes <typeparamref name="T"/> with System.Text.Json's polymorphism and reads both the current discriminators and the full CLR
/// names of the derived types, also from a <c>$type</c> property when the base uses another discriminator name.
/// </summary>
internal sealed class ClrTypeNameConverter<T> : JsonConverter<T>, IClrTypeNameConverter
{
    private const string ClrTypeProperty = "$type";

    private readonly ConditionalWeakTable<JsonSerializerOptions, JsonTypeInfo<T>> _typeInfos = [];

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonTypeInfo<T> typeInfo = GetTypeInfo(options);
        if (reader.TokenType == JsonTokenType.StartObject && FindDerivedType(reader, typeInfo.PolymorphismOptions!) is { } match)
        {
            JsonObject node = JsonNode.Parse(ref reader)!.AsObject();
            node.Remove(match.PropertyName);
            return match.Type == typeof(T) ? node.Deserialize(typeInfo) : (T?)node.Deserialize(options.GetTypeInfo(match.Type));
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

    private static (Type Type, string PropertyName)? FindDerivedType(Utf8JsonReader probe, JsonPolymorphismOptions polymorphism)
    {
        string propertyName = polymorphism.TypeDiscriminatorPropertyName;
        bool readClrTypeProperty = !string.Equals(propertyName, ClrTypeProperty, StringComparison.Ordinal);
        Type? current = null;
        Type? legacy = null;
        int seen = 0;
        int depth = probe.CurrentDepth;
        while (probe.Read() && probe.CurrentDepth > depth)
        {
            if (probe.TokenType != JsonTokenType.PropertyName || probe.CurrentDepth != depth + 1)
                continue;

            if (probe.ValueTextEquals(propertyName))
            {
                probe.Read();
                seen++;
                current = FindByValue(polymorphism, ref probe);
            }
            else if (readClrTypeProperty && probe.ValueTextEquals(ClrTypeProperty))
            {
                probe.Read();
                legacy = probe.TokenType == JsonTokenType.String ? FindByClrName(polymorphism, probe.GetString()!) : null;
            }
        }

        return seen switch
        {
            1 when current is not null => (current, propertyName),
            0 when legacy is not null => (legacy, ClrTypeProperty),
            _ => null,
        };
    }

    private static Type? FindByValue(JsonPolymorphismOptions polymorphism, ref Utf8JsonReader probe)
    {
        if (probe.TokenType == JsonTokenType.Number && probe.TryGetInt32(out int number))
        {
            foreach (JsonDerivedType derived in polymorphism.DerivedTypes)
            {
                if (derived.TypeDiscriminator is int value && value == number)
                    return derived.DerivedType;
            }

            return null;
        }

        if (probe.TokenType != JsonTokenType.String)
            return null;

        string text = probe.GetString()!;
        foreach (JsonDerivedType derived in polymorphism.DerivedTypes)
        {
            if (derived.TypeDiscriminator is string value && string.Equals(value, text, StringComparison.Ordinal))
                return derived.DerivedType;
        }

        return FindByClrName(polymorphism, text);
    }

    private static Type? FindByClrName(JsonPolymorphismOptions polymorphism, string value)
    {
        foreach (JsonDerivedType derived in polymorphism.DerivedTypes)
        {
            if (derived.TypeDiscriminator is string current && string.Equals(current, value, StringComparison.Ordinal))
                return null;
        }

        foreach (JsonDerivedType derived in polymorphism.DerivedTypes)
        {
            if (string.Equals(derived.DerivedType.FullName, value, StringComparison.Ordinal))
                return derived.DerivedType;
        }

        return null;
    }
}
