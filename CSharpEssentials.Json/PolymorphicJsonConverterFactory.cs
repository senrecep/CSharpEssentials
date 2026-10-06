using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSharpEssentials.Json;

internal static class PolymorphicJson
{
    internal const string Message =
        "Polymorphic JSON discovers derived types by scanning the loaded assemblies and serializes them by reflection. Use [JsonPolymorphic] with source generation for trimmed or native AOT applications.";
}

[RequiresUnreferencedCode(PolymorphicJson.Message)]
[RequiresDynamicCode(PolymorphicJson.Message)]
public sealed class PolymorphicJsonConverterFactory : JsonConverterFactory
{
    /// <summary>
    /// Abstract classes and interfaces, except collections and dictionaries (<see cref="System.Collections.IEnumerable"/>),
    /// which keep the built-in array/object serialization.
    /// </summary>
    public override bool CanConvert(Type typeToConvert) =>
        (typeToConvert.IsAbstract || typeToConvert.IsInterface) &&
        !typeof(System.Collections.IEnumerable).IsAssignableFrom(typeToConvert);

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type converterType = typeof(PolymorphicJsonConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter?)Activator.CreateInstance(converterType);
    }
}

[RequiresUnreferencedCode(PolymorphicJson.Message)]
[RequiresDynamicCode(PolymorphicJson.Message)]
public sealed class PolymorphicJsonConverter<T> : JsonConverter<T>
{
    private const string TypePropertyName = "$type";
    private static readonly Lazy<Dictionary<string, Type>> TypeCache = new(() =>
    {
        Type baseType = typeof(T);
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(GetLoadableTypes)
            .Where(t => baseType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .ToDictionary(t => t.FullName ?? t.Name);
    });

    // A partially loadable assembly (e.g. a proxy assembly still emitting types) must not break discovery
    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.OfType<Type>();
        }
    }

    private static readonly ConditionalWeakTable<JsonSerializerOptions, JsonSerializerOptions> InnerOptionsCache = [];

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
