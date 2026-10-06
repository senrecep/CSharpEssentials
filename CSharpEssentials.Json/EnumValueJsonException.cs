using System.Text.Json;
using CSharpEssentials.Enums;

namespace CSharpEssentials.Json;

/// <summary>
/// Thrown by the enum converters of <see cref="EnumConverterFactory"/> when a JSON value is not an accepted enum value.
/// </summary>
/// <param name="error">The rejected value.</param>
public sealed class EnumValueJsonException(EnumValueError error) : JsonException(error?.Message)
{
    private EnumValueError Rejected { get; } = error ?? throw new ArgumentNullException(nameof(error));

    /// <summary>
    /// The rejected value. <see cref="EnumValueError.Path"/> is the JSON path (<see cref="JsonException.Path"/>) once the serializer
    /// has added it.
    /// </summary>
    public EnumValueError Error => Rejected.Path is null && Path is not null ? Rejected with { Path = Path } : Rejected;
}
