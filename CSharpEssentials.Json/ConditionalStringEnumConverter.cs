using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpEssentials.Enums;

namespace CSharpEssentials.Json;

/// <summary>
/// Converts enums with a <see cref="StringEnumAttribute"/> to strings.
/// Names follow <see cref="StringEnumNaming"/> (snake_case by default, <c>[JsonStringEnumMemberName]</c> wins).
/// </summary>
public class ConditionalStringEnumConverter : JsonConverterFactory
{
    private readonly JsonNamingPolicy? _namingPolicy;
    private readonly bool _allowIntegerValues;
    private readonly Predicate<Type> _canConvert;

    public ConditionalStringEnumConverter(
        JsonNamingPolicy? namingPolicy = null,
        bool allowIntegerValues = true,
        Predicate<Type>? canConvert = null)
    {
        _namingPolicy = namingPolicy ?? StringEnumNaming.DefaultPolicy;
        _allowIntegerValues = allowIntegerValues;
        _canConvert = canConvert ?? StringEnumNaming.IsStringEnum;
    }

    /// <summary>
    /// Whether a number that is not a defined member (or, for <see cref="FlagsAttribute"/> enums, a combination of
    /// defined flags) is accepted when reading. Defaults to <see langword="true"/> (the <see cref="JsonStringEnumConverter"/>
    /// behavior); set it to <see langword="false"/> to reject such values with a <see cref="JsonException"/>.
    /// Writing is unaffected.
    /// </summary>
    public bool AllowUndefinedValues { get; set; } = true;

    public override bool CanConvert(Type typeToConvert) => _canConvert(typeToConvert);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        JsonConverter converter = new JsonStringEnumConverter(_namingPolicy, _allowIntegerValues)
            .CreateConverter(typeToConvert, options);
        if (AllowUndefinedValues)
            return converter;
        return (JsonConverter)Activator.CreateInstance(typeof(DefinedEnumConverter<>).MakeGenericType(typeToConvert), converter)!;
    }

    private sealed class DefinedEnumConverter<TEnum>(JsonConverter<TEnum> inner) : JsonConverter<TEnum>
        where TEnum : struct, Enum
    {
        public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            EnsureDefined(inner.Read(ref reader, typeToConvert, options));

        public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options) =>
            inner.Write(writer, value, options);

        public override TEnum ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            EnsureDefined(inner.ReadAsPropertyName(ref reader, typeToConvert, options));

        public override void WriteAsPropertyName(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options) =>
            inner.WriteAsPropertyName(writer, value, options);

        private static TEnum EnsureDefined(TEnum value) =>
            StringEnumNaming.IsDefined(value)
                ? value
                : throw new JsonException($"The value '{value:D}' is not defined in enum '{typeof(TEnum).Name}'.");
    }
}
