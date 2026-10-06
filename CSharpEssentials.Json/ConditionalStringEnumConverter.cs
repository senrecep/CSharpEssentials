using System.Text.Json;
using CSharpEssentials.Enums;

namespace CSharpEssentials.Json;

/// <summary>
/// The 4.x enum converter, kept as an <see cref="EnumConverterFactory"/> that maps its arguments to <see cref="EnumConventions"/>
/// and reads in <see cref="EnumReadMode.Input"/> mode (so <c>allowIntegerValues</c> keeps its meaning).
/// </summary>
[Obsolete("Use JsonSerializerOptions.AddEnumConventions(EnumConventions) instead. Wire names are set at build time.")]
public class ConditionalStringEnumConverter : EnumConverterFactory
{
    /// <summary>Creates the converter.</summary>
    /// <param name="namingPolicy"><see langword="null"/> or <see cref="JsonNamingPolicy.SnakeCaseLower"/>; wire names come from enum metadata.</param>
    /// <param name="allowIntegerValues">Maps to <see cref="EnumConventions.AcceptNumbers"/>.</param>
    /// <param name="canConvert">Maps to <see cref="EnumConventions.CanHandle"/>; <see langword="null"/> keeps the default.</param>
    /// <exception cref="NotSupportedException"><paramref name="namingPolicy"/> is another policy.</exception>
    public ConditionalStringEnumConverter(
        JsonNamingPolicy? namingPolicy = null,
        bool allowIntegerValues = true,
        Predicate<Type>? canConvert = null)
        : base(CreateConventions(namingPolicy, allowIntegerValues, canConvert), EnumReadMode.Input)
    {
    }

    private static EnumConventions CreateConventions(JsonNamingPolicy? namingPolicy, bool allowIntegerValues, Predicate<Type>? canConvert)
    {
        if (namingPolicy is not null && !ReferenceEquals(namingPolicy, JsonNamingPolicy.SnakeCaseLower))
        {
            throw new NotSupportedException(
                "Runtime enum naming policies are not supported. Set the naming at build time with the CSharpEssentialsEnumNaming " +
                "MSBuild property, [StringEnum(Naming = ...)] or [JsonStringEnumMemberName] (enum conventions design, section 4.1).");
        }

        EnumConventions conventions = EnumConventions.Default with { AcceptNumbers = allowIntegerValues };
        return canConvert is null ? conventions : conventions with { CanHandle = type => canConvert(type) };
    }
}
