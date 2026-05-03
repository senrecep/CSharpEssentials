using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSharpEssentials.Json;

public sealed class PolymorphicJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsAbstract || typeToConvert.IsInterface;

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type converterType = typeof(PolymorphicJsonConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter?)Activator.CreateInstance(converterType);
    }
}


public sealed class PolymorphicJsonConverter<T> : JsonConverter<T>
{
    private const string TypePropertyName = "$type";
    private static readonly Lazy<Dictionary<string, Type>> TypeCache = new(() =>
    {
        Type baseType = typeof(T);
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => baseType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .ToDictionary(t => t.FullName ?? t.Name);
    });

    private static readonly ConditionalWeakTable<JsonSerializerOptions, JsonSerializerOptions> InnerOptionsCache = new();

    private static JsonSerializerOptions GetInnerOptions(JsonSerializerOptions options)
    {
        return InnerOptionsCache.GetValue(options, static opts =>
        {
            var inner = new JsonSerializerOptions(opts);
            for (int i = inner.Converters.Count - 1; i >= 0; i--)
            {
                if (inner.Converters[i] is PolymorphicJsonConverterFactory)
                    inner.Converters.RemoveAt(i);
            }
            return inner;
        });
    }

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        JsonElement root = doc.RootElement;

        if (!root.TryGetProperty(TypePropertyName, out JsonElement typeProperty))
        {
            throw new JsonException($"Missing required property '{TypePropertyName}' for polymorphic deserialization.");
        }

        string? typeName = typeProperty.GetString();
        if (typeName == null || !TypeCache.Value.TryGetValue(typeName, out Type? derivedType))
        {
            throw new JsonException($"Unknown type discriminator '{typeName}'.");
        }

        return (T?)JsonSerializer.Deserialize(root.GetRawText(), derivedType, GetInnerOptions(options));
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        Type valueType = value.GetType();
        writer.WriteStartObject();
        writer.WriteString(TypePropertyName, valueType.FullName);

        foreach (JsonProperty property in JsonSerializer.SerializeToElement(value, valueType, GetInnerOptions(options)).EnumerateObject())
        {
            property.WriteTo(writer);
        }

        writer.WriteEndObject();
    }
}
