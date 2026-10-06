using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpEssentials.Enums;

namespace CSharpEssentials.Json;

/// <summary>
/// Converts the enums selected by <see cref="EnumConventions.CanHandle"/> with their wire names (design section 7).
/// Flags are written as arrays; dictionary keys, nullable enums and enum collections are handled through the serializer.
/// </summary>
/// <remarks>
/// Add it with <see cref="JsonSerializerOptionsExtensions.AddEnumConventions"/>, which also removes competing enum converters.
/// Enums with generated metadata (<see cref="StringEnumAttribute"/>) get their converter without reflection; an enum without
/// generated metadata that <see cref="EnumConventions.CanHandle"/> accepts takes the reflection path
/// (<see cref="EnumMetadata.GetOrCreateWithReflection"/>).
/// </remarks>
public class EnumConverterFactory : JsonConverterFactory
{
    /// <summary>Creates the factory.</summary>
    /// <param name="conventions">The conventions.</param>
    /// <param name="mode">Where read values come from: <see cref="EnumReadMode.Data"/> (tolerant) or <see cref="EnumReadMode.Input"/> (requests).</param>
    /// <param name="writeAs">The output format; <see langword="null"/> uses <see cref="EnumConventions.WriteAs"/>.</param>
    public EnumConverterFactory(EnumConventions conventions, EnumReadMode mode = EnumReadMode.Data, EnumWireFormat? writeAs = null)
    {
        Conventions = conventions ?? throw new ArgumentNullException(nameof(conventions));
        Mode = mode;
        WriteAs = writeAs ?? conventions.WriteAs;
    }

    /// <summary>The conventions.</summary>
    public EnumConventions Conventions { get; }

    /// <summary>Where read values come from.</summary>
    public EnumReadMode Mode { get; }

    /// <summary>The output format.</summary>
    public EnumWireFormat WriteAs { get; }

    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert is not null && typeToConvert.IsEnum && Conventions.CanHandle(typeToConvert);

    /// <inheritdoc />
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        _ = typeToConvert ?? throw new ArgumentNullException(nameof(typeToConvert));

        IEnumInfo info = EnumMetadata.TryGet(typeToConvert, out IEnumInfo? registered)
            ? registered
            : EnumMetadata.GetOrCreateWithReflection(typeToConvert);
        return info.Accept(new ConverterVisitor(Conventions, Mode, WriteAs));
    }

    private sealed class ConverterVisitor(EnumConventions conventions, EnumReadMode mode, EnumWireFormat writeAs) : IEnumInfoVisitor<JsonConverter>
    {
        public JsonConverter Visit<TEnum>(EnumInfo<TEnum> info) where TEnum : struct, Enum =>
            new EnumConverter<TEnum>(info, conventions, mode, writeAs);
    }
}
