using System.Buffers;
using System.Collections.Concurrent;
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
    /// async streams (types implementing <see cref="IAsyncEnumerable{T}"/>), <see cref="JsonNode"/> and types marked with
    /// <see cref="JsonPolymorphicAttribute"/> or <see cref="JsonDerivedTypeAttribute"/>, which keep the built-in serialization.
    /// </summary>
    public override bool CanConvert(Type typeToConvert) =>
        (typeToConvert.IsAbstract || typeToConvert.IsInterface) &&
        !typeof(System.Collections.IEnumerable).IsAssignableFrom(typeToConvert) &&
        !typeof(JsonNode).IsAssignableFrom(typeToConvert) &&
        !typeToConvert.GetInterfaces().Append(typeToConvert).Any(IsAsyncEnumerable) &&
        !typeToConvert.IsDefined(typeof(JsonPolymorphicAttribute), false) &&
        !typeToConvert.IsDefined(typeof(JsonDerivedTypeAttribute), false);

    private static bool IsAsyncEnumerable(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>);

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
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (PolymorphicTypeScan.IsFrameworkAssembly(assembly))
                continue;

            foreach (Type type in PolymorphicTypeScan.GetLoadableTypes(assembly))
            {
                if (baseType.IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract && !type.ContainsGenericParameters)
                    types.TryAdd(type.FullName ?? type.Name, type);
            }
        }
        return types;
    });

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (!TryReadTypeName(reader, out string? typeName))
        {
            throw new JsonException($"Missing required property '{TypePropertyName}' for polymorphic deserialization.");
        }

        if (typeName == null || !TypeCache.Value.TryGetValue(typeName, out Type? derivedType))
        {
            throw new JsonException($"Unknown type discriminator '{typeName}'.");
        }

        PolymorphicJson.EnterNested(options);
        try
        {
            return (T?)JsonSerializer.Deserialize(ref reader, derivedType, options);
        }
        finally
        {
            PolymorphicJson.ExitNested();
        }
    }

    private static bool TryReadTypeName(Utf8JsonReader probe, out string? typeName)
    {
        typeName = null;
        bool found = false;
        if (probe.TokenType != JsonTokenType.StartObject)
            return false;

        int depth = probe.CurrentDepth;
        while (probe.Read() && probe.CurrentDepth > depth)
        {
            if (probe.TokenType != JsonTokenType.PropertyName || probe.CurrentDepth != depth + 1 || !probe.ValueTextEquals(TypePropertyName))
                continue;

            probe.Read();
            typeName = probe.GetString();
            found = true;
        }

        return found;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        Type valueType = value.GetType();
        if (!writer.Options.Indented && !options.WriteIndented && ReferenceEquals(writer.Options.Encoder, options.Encoder))
        {
            byte[] body;
            PolymorphicJson.EnterNested(options);
            try
            {
                body = JsonSerializer.SerializeToUtf8Bytes(value, valueType, options);
            }
            finally
            {
                PolymorphicJson.ExitNested();
            }

            if (body.Length >= 2 && body[0] == (byte)'{')
            {
                WriteSpliced(writer, GetPrefix(valueType, options), body);
                return;
            }
        }

        WriteViaElement(writer, value, valueType, options);
    }

    private readonly ConcurrentDictionary<Type, byte[]> _prefixes = new();

    private byte[] GetPrefix(Type valueType, JsonSerializerOptions options) =>
        _prefixes.GetOrAdd(valueType, static (type, opts) =>
        {
            ArrayBufferWriter<byte> buffer = new();
            using (Utf8JsonWriter prefixWriter = new(buffer, new JsonWriterOptions { Encoder = opts.Encoder, SkipValidation = true }))
            {
                prefixWriter.WriteStartObject();
                prefixWriter.WriteString(TypePropertyName, type.FullName);
            }
            return buffer.WrittenSpan.ToArray();
        }, options);

    private static void WriteSpliced(Utf8JsonWriter writer, byte[] prefix, byte[] body)
    {
        bool empty = body.Length == 2;
        int length = prefix.Length + (empty ? 1 : body.Length);
        byte[] rented = ArrayPool<byte>.Shared.Rent(length);
        try
        {
            prefix.CopyTo(rented, 0);
            if (empty)
            {
                rented[prefix.Length] = (byte)'}';
            }
            else
            {
                rented[prefix.Length] = (byte)',';
                body.AsSpan(1).CopyTo(rented.AsSpan(prefix.Length + 1));
            }
            writer.WriteRawValue(rented.AsSpan(0, length), skipInputValidation: true);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }

    private static void WriteViaElement(Utf8JsonWriter writer, T value, Type valueType, JsonSerializerOptions options)
    {
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
