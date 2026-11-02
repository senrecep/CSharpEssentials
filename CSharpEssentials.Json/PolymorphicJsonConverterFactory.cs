using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace CSharpEssentials.Json;

[RequiresUnreferencedCode(PolymorphicJson.Message)]
[RequiresDynamicCode(PolymorphicJson.Message)]
public sealed class PolymorphicJsonConverterFactory : JsonConverterFactory
{
    /// <summary>
    /// Abstract classes and interfaces, except collections and dictionaries (<see cref="System.Collections.IEnumerable"/>),
    /// async streams (<see cref="IAsyncEnumerable{T}"/>), <see cref="JsonNode"/> and types marked with
    /// <see cref="JsonPolymorphicAttribute"/> or <see cref="JsonDerivedTypeAttribute"/>, which keep the built-in serialization.
    /// </summary>
    public override bool CanConvert(Type typeToConvert) =>
        (typeToConvert.IsAbstract || typeToConvert.IsInterface) &&
        !typeof(System.Collections.IEnumerable).IsAssignableFrom(typeToConvert) &&
        !typeof(JsonNode).IsAssignableFrom(typeToConvert) &&
        !(typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>)) &&
        !typeToConvert.IsDefined(typeof(JsonPolymorphicAttribute), false) &&
        !typeToConvert.IsDefined(typeof(JsonDerivedTypeAttribute), false);

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
        Dictionary<string, Type> types = [];
        foreach (Type type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(GetLoadableTypes))
        {
            if (baseType.IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract && !type.ContainsGenericParameters)
                types.TryAdd(type.FullName ?? type.Name, type);
        }
        return types;
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

        PolymorphicJson.EnterNested(options);
        try
        {
            return (T?)JsonSerializer.Deserialize(root.GetRawText(), derivedType, options);
        }
        finally
        {
            PolymorphicJson.ExitNested();
        }
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        Type valueType = value.GetType();
        JsonElement element;
        PolymorphicJson.EnterNested(options);
        try
        {
            element = JsonSerializer.SerializeToElement(value, valueType, options);
        }
        finally
        {
            PolymorphicJson.ExitNested();
        }

        writer.WriteStartObject();
        writer.WriteString(TypePropertyName, valueType.FullName);

        foreach (JsonProperty property in element.EnumerateObject())
        {
            property.WriteTo(writer);
        }

        writer.WriteEndObject();
    }
}
