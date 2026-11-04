using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSharpEssentials.Any;

/// <summary>
/// Reads and writes the <c>Value</c> property of <see cref="Any{T0, T1}"/> through <see cref="Any{T0, T1, T2, T3, T4, T5, T6, T7}"/>.
/// Reading keeps the caller's <see cref="JsonSerializerOptions"/> with the parsed value, so the union deserializes the value
/// for its branch type with those options. It is public so that System.Text.Json source generation can create it.
/// </summary>
public sealed class AnyValueJsonConverter : JsonConverter<object>
{
    /// <inheritdoc />
    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new AnyJsonValue(JsonElement.ParseValue(ref reader), options);

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value, options);
}
