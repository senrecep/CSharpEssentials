using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpEssentials.Enums;

namespace CSharpEssentials.Json;

/// <summary>
/// Converts enums with a <see cref="StringEnumAttribute"/> to strings.
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
        _namingPolicy = namingPolicy ?? JsonNamingPolicy.SnakeCaseLower;
        _allowIntegerValues = allowIntegerValues;
        _canConvert = canConvert ?? (type => type.IsEnum && type.GetCustomAttribute<StringEnumAttribute>() != null);
    }
    public override bool CanConvert(Type typeToConvert) => _canConvert(typeToConvert);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return new JsonStringEnumConverter(_namingPolicy, _allowIntegerValues)
            .CreateConverter(typeToConvert, options);
    }
}
