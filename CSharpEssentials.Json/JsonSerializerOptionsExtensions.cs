using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpEssentials.Enums;

namespace CSharpEssentials.Json;

/// <summary>
/// Enum convention registration for <see cref="JsonSerializerOptions"/>.
/// </summary>
public static class JsonSerializerOptionsExtensions
{
    /// <summary>
    /// Adds an <see cref="EnumConverterFactory"/> at position 0 and removes every other enum converter
    /// (<see cref="JsonStringEnumConverter"/>, <see cref="JsonStringEnumConverter{TEnum}"/> and earlier
    /// <see cref="EnumConverterFactory"/> instances, <c>ConditionalStringEnumConverter</c> included), so a host default cannot win by order.
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

        for (int i = options.Converters.Count - 1; i >= 0; i--)
        {
            if (IsEnumConverter(options.Converters[i]))
                options.Converters.RemoveAt(i);
        }

        options.Converters.Insert(0, new EnumConverterFactory(conventions, mode, writeAs));
        return options;
    }

    private static bool IsEnumConverter(JsonConverter converter) =>
        converter is EnumConverterFactory or JsonStringEnumConverter ||
        converter.GetType() is { IsGenericType: true } type && type.GetGenericTypeDefinition() == typeof(JsonStringEnumConverter<>);
}
