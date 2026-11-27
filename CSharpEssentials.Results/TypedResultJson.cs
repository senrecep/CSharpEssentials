using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace CSharpEssentials.ResultPattern;

/// <summary>
/// Shared logic of the typed result JSON converters. A result is written as <c>{"case":"&lt;name&gt;","value":&lt;case&gt;}</c>,
/// where the name is <see cref="SuccessCase"/> for the success value and the name of the slot type for an error, and
/// <see langword="default"/> is written as <c>null</c>. Values are (de)serialized as their slot type through
/// <see cref="JsonSerializerOptions.GetTypeInfo(Type)"/>, so a source-generated context works without reflection.
/// </summary>
internal static class TypedResultJson
{
    internal const string SuccessCase = "Success";

    private const string CaseProperty = "case";
    private const string ValueProperty = "value";

    /// <summary>
    /// Returns the case names: <see cref="SuccessCase"/> first, then one per error slot. A slot type whose name another slot
    /// type (or the success case) shares uses its <see cref="TypeName"/> instead, which carries no assembly version.
    /// </summary>
    internal static string[] CaseNames(params Type[] errorTypes)
    {
        string[] names = new string[errorTypes.Length + 1];
        names[0] = SuccessCase;
        for (int i = 0; i < errorTypes.Length; i++)
        {
            Type type = errorTypes[i];
            bool shared = type.Name == SuccessCase
                || Array.Exists(errorTypes, other => other != type && other.Name == type.Name);
            names[i + 1] = shared ? TypeName(type) : type.Name;
        }

        return names;
    }

    /// <summary>
    /// Returns the type's name as C# writes it, namespace and declaring types included and generic arguments expanded the
    /// same way, for example <c>MyApp.Errors.Validation&lt;MyApp.Email&gt;</c>.
    /// </summary>
    internal static string TypeName(Type type)
    {
        if (type.IsArray)
            return $"{TypeName(type.GetElementType()!)}[{new string(',', type.GetArrayRank() - 1)}]";

        string prefix = string.IsNullOrEmpty(type.Namespace) ? string.Empty : type.Namespace + ".";
        if (type.DeclaringType is { } outer)
            prefix = TypeName(outer) + ".";
        int tick = type.Name.IndexOf('`');
        string name = tick < 0 ? type.Name : type.Name.Substring(0, tick);
        return type.IsGenericType
            ? $"{prefix}{name}<{string.Join(", ", type.GetGenericArguments().Select(TypeName))}>"
            : prefix + name;
    }

    internal static void WriteCase<TCase>(Utf8JsonWriter writer, string name, TCase value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString(CaseProperty, name);
        writer.WritePropertyName(ValueProperty);
        JsonSerializer.Serialize(writer, value, TypeInfo<TCase>(options));
        writer.WriteEndObject();
    }

    /// <summary>
    /// Reads the envelope object and returns the index of its case in <paramref name="names"/>, leaving
    /// <paramref name="reader"/> on the closing brace and <paramref name="value"/> on the first token of the value.
    /// The two properties may come in either order; any other property is an error.
    /// </summary>
    internal static int ReadEnvelope(ref Utf8JsonReader reader, string[] names, Type typeToConvert, out Utf8JsonReader value)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException($"Expected an object or null for {typeToConvert}, got {reader.TokenType}.");

        int index = -1;
        bool hasValue = false;
        value = default;
        while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
        {
            if (reader.ValueTextEquals(CaseProperty) && index < 0)
            {
                reader.Read();
                index = reader.TokenType == JsonTokenType.String ? Array.IndexOf(names, reader.GetString()) : -1;
                if (index < 0)
                    throw new JsonException($"Unknown case for {typeToConvert}. Expected one of: {string.Join(", ", names)}.");
            }
            else if (reader.ValueTextEquals(ValueProperty) && !hasValue)
            {
                reader.Read();
                value = reader;
                hasValue = true;
                reader.Skip();
            }
            else
            {
                throw new JsonException($"Unexpected or repeated property '{reader.GetString()}' in {typeToConvert}. Expected \"{CaseProperty}\" and \"{ValueProperty}\".");
            }
        }

        if (index < 0 || !hasValue)
            throw new JsonException($"{typeToConvert} needs both a \"{CaseProperty}\" and a \"{ValueProperty}\" property.");

        return index;
    }

    /// <summary>Deserializes a case value as <typeparamref name="TCase"/>; a JSON <c>null</c> is an error.</summary>
    internal static TCase ReadCase<TCase>(ref Utf8JsonReader reader, JsonSerializerOptions options) =>
        JsonSerializer.Deserialize(ref reader, TypeInfo<TCase>(options))
        ?? throw new JsonException($"The value of a {typeof(TCase)} case cannot be null.");

    private static JsonTypeInfo<TCase> TypeInfo<TCase>(JsonSerializerOptions options) =>
#if NET11_0_OR_GREATER
        options.GetTypeInfo<TCase>();
#else
        (JsonTypeInfo<TCase>)options.GetTypeInfo(typeof(TCase));
#endif
}
