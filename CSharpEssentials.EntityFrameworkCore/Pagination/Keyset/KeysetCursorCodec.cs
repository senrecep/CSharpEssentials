using System.Buffers;
using System.Text.Json;

using CSharpEssentials.Errors;

namespace CSharpEssentials.EntityFrameworkCore.Pagination.Keyset;

/// <summary>
/// Encodes keyset cursors as base64url (no padding) of the JSON payload
/// <c>{"v":1,"d":"a"|"b","k":"&lt;key fingerprint&gt;","p":[values]}</c>, then applies the <see cref="ICursorProtector"/>.
/// </summary>
internal static class KeysetCursorCodec
{
    public const int Version = 1;

    private const string VersionProperty = "v";
    private const string DirectionProperty = "d";
    private const string KeysProperty = "k";
    private const string ValuesProperty = "p";
    private const string After = "a";
    private const string Before = "b";

    public static string Encode(
        KeysetCursorDirection direction,
        string fingerprint,
        IReadOnlyList<Type> types,
        IReadOnlyList<object> values,
        ICursorProtector protector)
    {
        ArrayBufferWriter<byte> buffer = new(128);
        using (Utf8JsonWriter writer = new(buffer))
        {
            writer.WriteStartObject();
            writer.WriteNumber(VersionProperty, Version);
            writer.WriteString(DirectionProperty, direction == KeysetCursorDirection.After ? After : Before);
            writer.WriteString(KeysProperty, fingerprint);
            writer.WriteStartArray(ValuesProperty);
            for (int i = 0; i < types.Count; i++)
                KeysetValueSerializer.Write(writer, types[i], values[i]);
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        return protector.Protect(Base64Url.Encode(buffer.WrittenSpan));
    }

    public static bool TryDecode(
        string cursor,
        KeysetCursorDirection expectedDirection,
        string fingerprint,
        IReadOnlyList<Type> types,
        ICursorProtector protector,
        out object[] values,
        out Error error)
    {
        values = [];
        string parameter = expectedDirection == KeysetCursorDirection.After ? "after" : "before";

        if (!protector.TryUnprotect(cursor, out string encoded) || !Base64Url.TryDecode(encoded, out byte[] bytes))
        {
            error = KeysetCursorErrors.Invalid(parameter);
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(bytes);
            return TryRead(document.RootElement, expectedDirection, fingerprint, types, parameter, out values, out error);
        }
        // GetString throws InvalidOperationException for invalid UTF-8 or a lone escaped surrogate inside a string value.
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            error = KeysetCursorErrors.Invalid(parameter);
            return false;
        }
    }

    private static bool TryRead(
        JsonElement root,
        KeysetCursorDirection expectedDirection,
        string fingerprint,
        IReadOnlyList<Type> types,
        string parameter,
        out object[] values,
        out Error error)
    {
        values = [];
        error = KeysetCursorErrors.Invalid(parameter);

        if (root.ValueKind != JsonValueKind.Object
            || !root.TryGetProperty(VersionProperty, out JsonElement version)
            || version.ValueKind != JsonValueKind.Number)
            return false;

        if (!version.TryGetInt32(out int versionNumber) || versionNumber != Version)
        {
            error = KeysetCursorErrors.UnsupportedVersion(parameter);
            return false;
        }

        if (!root.TryGetProperty(DirectionProperty, out JsonElement direction)
            || direction.ValueKind != JsonValueKind.String
            || !root.TryGetProperty(KeysProperty, out JsonElement keys)
            || keys.ValueKind != JsonValueKind.String
            || !root.TryGetProperty(ValuesProperty, out JsonElement payload)
            || payload.ValueKind != JsonValueKind.Array)
            return false;

        string expected = expectedDirection == KeysetCursorDirection.After ? After : Before;
        if (!direction.ValueEquals(expected))
        {
            error = KeysetCursorErrors.DirectionMismatch(parameter);
            return false;
        }

        if (!keys.ValueEquals(fingerprint) || payload.GetArrayLength() != types.Count)
        {
            error = KeysetCursorErrors.KeyMismatch(parameter);
            return false;
        }

        object[] read = new object[types.Count];
        int index = 0;
        foreach (JsonElement element in payload.EnumerateArray())
        {
            if (!KeysetValueSerializer.TryRead(element, types[index], out object? value) || value is null)
                return false;
            read[index++] = value;
        }

        values = read;
        error = default;
        return true;
    }
}
