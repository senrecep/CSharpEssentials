using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSharpEssentials.Json.Comparison.Benchmarks;

/// <summary>
/// The documented "DateTime.Parse as a fallback" converter. The culture and styles are added to Read so the result
/// is UTC, like CSE's, and Write uses the round-trip "O" format instead of the sample's "dd/MM/yyyy" (Write is not benchmarked).
/// </summary>
// https://learn.microsoft.com/dotnet/standard/datetime/system-text-json-support#use-datetimeoffsetparse-as-a-fallback
public sealed class FallbackDateTimeConverter : JsonConverter<DateTime>
{
    /// <inheritdoc />
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TryGetDateTime(out DateTime value)
            ? value
            : DateTime.Parse(reader.GetString()!, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString("O", CultureInfo.InvariantCulture));
}
