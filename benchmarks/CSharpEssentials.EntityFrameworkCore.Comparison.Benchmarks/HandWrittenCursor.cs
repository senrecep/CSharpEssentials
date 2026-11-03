using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks;

/// <summary>
/// The hand-written cursor: base64url of <c>{"c":ticks,"i":id}</c>, with no version, direction or key checks. The MR rows
/// decode it too and pass it as their reference object, since MR reads <see cref="PublishedAt"/> and <see cref="Id"/> by name.
/// </summary>
public sealed record HandWrittenCursor([property: JsonPropertyName("c")] long Ticks, [property: JsonPropertyName("i")] int Id)
{
    [JsonIgnore]
    public DateTime PublishedAt => new(Ticks, DateTimeKind.Utc);

    public static string Encode(FeedItem item) =>
        Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(new HandWrittenCursor(item.PublishedAt.Ticks, item.Id)))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

    public static HandWrittenCursor Decode(string cursor)
    {
        string base64 = cursor.Replace('-', '+').Replace('_', '/');
        base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
        return JsonSerializer.Deserialize<HandWrittenCursor>(Convert.FromBase64String(base64))
            ?? throw new FormatException("The cursor holds no value.");
    }
}
