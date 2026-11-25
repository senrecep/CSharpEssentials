using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using CSharpEssentials.Enums;

namespace CSharpEssentials.Json;

/// <summary>
/// Enum convention registration for <see cref="JsonSerializerOptions"/>.
/// </summary>
public static class JsonSerializerOptionsExtensions
{
    private static readonly ConditionalWeakTable<JsonSerializerOptions, object> RequiredParameterOptions = [];
    private static readonly object Marker = new();

    /// <summary>
    /// Adds an <see cref="EnumConverterFactory"/> at position 0 and removes earlier <see cref="EnumConverterFactory"/>
    /// instances (<c>ConditionalStringEnumConverter</c> included). Other converters such as <see cref="JsonStringEnumConverter"/>
    /// stay: the factory is first, so it wins for the enums the conventions handle, and enums it does not handle keep the
    /// converter the host configured.
    /// </summary>
    /// <param name="options">The options to change; they must not be in use yet.</param>
    /// <param name="conventions">The conventions.</param>
    /// <param name="mode">Where read values come from: <see cref="EnumReadMode.Data"/> (tolerant) or <see cref="EnumReadMode.Input"/> (requests).</param>
    /// <param name="writeAs">The output format; <see langword="null"/> uses <see cref="EnumConventions.WriteAs"/>.</param>
    /// <returns><paramref name="options"/>.</returns>
    public static JsonSerializerOptions AddEnumConventions(
        this JsonSerializerOptions options,
        EnumConventions conventions,
        EnumReadMode mode = EnumReadMode.Data,
        EnumWireFormat? writeAs = null)
    {
        _ = options ?? throw new ArgumentNullException(nameof(options));
        _ = conventions ?? throw new ArgumentNullException(nameof(conventions));
        return Replace(options, new EnumConverterFactory(conventions, mode, writeAs));
    }

    /// <summary>
    /// Like <see cref="AddEnumConventions"/>, and also converts enums without generated metadata that
    /// <see cref="EnumConventions.CanHandle"/> accepts, with metadata read by reflection
    /// (<see cref="EnumConverterFactory.CreateWithReflectionFallback"/>).
    /// </summary>
    /// <param name="options">The options to change; they must not be in use yet.</param>
    /// <param name="conventions">The conventions.</param>
    /// <param name="mode">Where read values come from: <see cref="EnumReadMode.Data"/> (tolerant) or <see cref="EnumReadMode.Input"/> (requests).</param>
    /// <param name="writeAs">The output format; <see langword="null"/> uses <see cref="EnumConventions.WriteAs"/>.</param>
    /// <returns><paramref name="options"/>.</returns>
    [RequiresUnreferencedCode(EnumConverterFactory.ReflectionMessage)]
    [RequiresDynamicCode(EnumConverterFactory.ReflectionMessage)]
    public static JsonSerializerOptions AddEnumConventionsWithReflection(
        this JsonSerializerOptions options,
        EnumConventions conventions,
        EnumReadMode mode = EnumReadMode.Data,
        EnumWireFormat? writeAs = null)
    {
        _ = options ?? throw new ArgumentNullException(nameof(options));
        _ = conventions ?? throw new ArgumentNullException(nameof(conventions));
        return Replace(options, EnumConverterFactory.CreateWithReflectionFallback(conventions, mode, writeAs));
    }

    private static JsonSerializerOptions Replace(JsonSerializerOptions options, EnumConverterFactory factory)
    {
        for (int i = options.Converters.Count - 1; i >= 0; i--)
        {
            if (IsEnumConverter(options.Converters[i]))
                options.Converters.RemoveAt(i);
        }

        options.Converters.Insert(0, factory);
        return options;
    }

    private static bool IsEnumConverter(JsonConverter converter) => converter is EnumConverterFactory;

    /// <summary>
    /// Adds <see cref="JsonTypeInfoModifiers.RequireValueTypeConstructorParameters"/> to <see cref="JsonSerializerOptions.TypeInfoResolver"/>,
    /// which must be set: a missing JSON property for a non-nullable value-type constructor parameter without a default then
    /// throws <see cref="JsonException"/> instead of binding as <see langword="default"/>.
    /// </summary>
    /// <remarks>
    /// Call it before <see cref="JsonPolymorphismExtensions.AddPolymorphism"/> or
    /// <see cref="JsonPolymorphismExtensions.AddPolymorphismWithReflection"/>, so their wrapper stays outermost.
    /// <see cref="EnhancedJsonSerializerOptions.ApplyTo"/> carries the requirement to the target, also when the target keeps its own resolver.
    /// </remarks>
    /// <param name="options">The options to change; they must not be in use yet.</param>
    /// <returns><paramref name="options"/>.</returns>
    /// <exception cref="InvalidOperationException"><see cref="JsonSerializerOptions.TypeInfoResolver"/> is not set.</exception>
    public static JsonSerializerOptions RequireValueTypeConstructorParameters(this JsonSerializerOptions options)
    {
        _ = options ?? throw new ArgumentNullException(nameof(options));
        IJsonTypeInfoResolver resolver = options.TypeInfoResolver is { } current
            ? JsonTypeInfoResolver.Combine(current)
            : throw new InvalidOperationException(
                "Set TypeInfoResolver (for example to a JsonSerializerContext or a DefaultJsonTypeInfoResolver) before RequireValueTypeConstructorParameters.");
        options.TypeInfoResolver = resolver.WithAddedModifier(JsonTypeInfoModifiers.RequireValueTypeConstructorParameters);
        RequiredParameterOptions.AddOrUpdate(options, Marker);
        return options;
    }

    internal static bool RequiresValueTypeConstructorParameters(JsonSerializerOptions options) =>
        RequiredParameterOptions.TryGetValue(options, out _);

    internal static void MarkValueTypeConstructorParametersRequired(JsonSerializerOptions options) =>
        RequiredParameterOptions.AddOrUpdate(options, Marker);
}
