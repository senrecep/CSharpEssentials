using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;
using CSharpEssentials.Enums;

namespace CSharpEssentials.Json;

/// <summary>
/// Converts enums with a <see cref="StringEnumAttribute"/> to strings.
/// </summary>
/// <param name="namingPolicy"></param>
/// <param name="allowIntegerValues"></param>
/// <param name="canConvert"></param>
public class ConditionalStringEnumConverter(
    JsonNamingPolicy? namingPolicy = null,
    bool allowIntegerValues = true,
    Predicate<Type>? canConvert = null) : JsonConverterFactory
{
    private readonly JsonNamingPolicy? _namingPolicy = namingPolicy ?? JsonNamingPolicy.SnakeCaseLower;
    private readonly bool _allowIntegerValues = allowIntegerValues;
    private readonly Predicate<Type> _canConvert = canConvert ?? (type => type.IsEnum && type.GetCustomAttribute<StringEnumAttribute>() != null);
    public override bool CanConvert(Type typeToConvert) => _canConvert(typeToConvert);

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return new JsonStringEnumConverter(_namingPolicy, _allowIntegerValues)
            .CreateConverter(typeToConvert, options);
    }
}