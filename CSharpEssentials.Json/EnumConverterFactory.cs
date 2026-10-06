using System.Diagnostics.CodeAnalysis;
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
/// Only enums with generated metadata (<see cref="StringEnumAttribute"/>) are converted, without reflection. A
/// <see cref="StringEnumAttribute"/> enum without generated metadata fails with <see cref="InvalidOperationException"/> instead of
/// silently becoming a number. Reflection metadata for other enums is an explicit opt-in:
/// <see cref="CreateWithReflectionFallback"/> or <see cref="JsonSerializerOptionsExtensions.AddEnumConventionsWithReflection"/>.
/// </remarks>
public class EnumConverterFactory : JsonConverterFactory
{
    /// <summary>Creates the factory.</summary>
    /// <param name="conventions">The conventions.</param>
    /// <param name="mode">Where read values come from: <see cref="EnumReadMode.Data"/> (tolerant) or <see cref="EnumReadMode.Input"/> (requests).</param>
    /// <param name="writeAs">The output format; <see langword="null"/> uses <see cref="EnumConventions.WriteAs"/>.</param>
    public EnumConverterFactory(EnumConventions conventions, EnumReadMode mode = EnumReadMode.Data, EnumWireFormat? writeAs = null)
        : this(conventions, mode, writeAs, reflectionFallback: null)
    {
    }

    private EnumConverterFactory(EnumConventions conventions, EnumReadMode mode, EnumWireFormat? writeAs, Func<Type, IEnumInfo>? reflectionFallback)
    {
        Conventions = conventions ?? throw new ArgumentNullException(nameof(conventions));
        Mode = mode;
        WriteAs = writeAs ?? conventions.WriteAs;
        ReflectionFallback = reflectionFallback;
    }

    /// <summary>
    /// Creates a factory that also converts enums without generated metadata that <see cref="EnumConventions.CanHandle"/> accepts,
    /// with metadata read by reflection (<see cref="EnumMetadata.GetOrCreateWithReflection"/>).
    /// </summary>
    /// <param name="conventions">The conventions.</param>
    /// <param name="mode">Where read values come from: <see cref="EnumReadMode.Data"/> (tolerant) or <see cref="EnumReadMode.Input"/> (requests).</param>
    /// <param name="writeAs">The output format; <see langword="null"/> uses <see cref="EnumConventions.WriteAs"/>.</param>
    [RequiresUnreferencedCode(ReflectionMessage)]
    [RequiresDynamicCode(ReflectionMessage)]
    public static EnumConverterFactory CreateWithReflectionFallback(
        EnumConventions conventions,
        EnumReadMode mode = EnumReadMode.Data,
        EnumWireFormat? writeAs = null) =>
        new(conventions, mode, writeAs, static type => EnumMetadata.GetOrCreateWithReflection(type));

    internal const string ReflectionMessage =
        "Enums without generated metadata are read with reflection. Mark them [StringEnum] and use the factory without reflection.";

    /// <summary>Whether enums without generated metadata are converted with reflection metadata.</summary>
    public bool UsesReflectionFallback => ReflectionFallback is not null;

    private Func<Type, IEnumInfo>? ReflectionFallback { get; }

    /// <summary>The conventions.</summary>
    public EnumConventions Conventions { get; }

    /// <summary>Where read values come from.</summary>
    public EnumReadMode Mode { get; }

    /// <summary>The output format.</summary>
    public EnumWireFormat WriteAs { get; }

    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        if (typeToConvert is null || !typeToConvert.IsEnum)
            return false;
        if (ReflectionFallback is not null || EnumMetadata.TryGet(typeToConvert, out _))
            return Conventions.CanHandle(typeToConvert);

        // A [StringEnum] enum without generated metadata is claimed so CreateConverter fails loudly instead of writing numbers,
        // unless the conventions exclude it.
        return typeToConvert.IsDefined(typeof(StringEnumAttribute), inherit: false) && Conventions.CanHandle(typeToConvert);
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">The enum has no generated metadata and the factory has no reflection fallback.</exception>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        _ = typeToConvert ?? throw new ArgumentNullException(nameof(typeToConvert));

        if (EnumMetadata.TryGet(typeToConvert, out IEnumInfo? info))
            return info.Accept(new ConverterVisitor(Conventions, Mode, WriteAs));
        if (ReflectionFallback is not null)
            return ReflectionFallback(typeToConvert).Accept(new ConverterVisitor(Conventions, Mode, WriteAs));

        throw new InvalidOperationException(
            $"Enum '{typeToConvert.FullName}' is marked [StringEnum] but has no generated metadata. Rebuild its project with the " +
            "CSharpEssentials.Enums 5.0 generator (C# 9 or newer) and make the enum and its containing types public or internal " +
            "(not private, protected, file-local or nested in a generic type).");
    }

    private sealed class ConverterVisitor(EnumConventions conventions, EnumReadMode mode, EnumWireFormat writeAs) : IEnumInfoVisitor<JsonConverter>
    {
        public JsonConverter Visit<TEnum>(EnumInfo<TEnum> info) where TEnum : struct, Enum =>
            new EnumConverter<TEnum>(info, conventions, mode, writeAs);
    }
}
