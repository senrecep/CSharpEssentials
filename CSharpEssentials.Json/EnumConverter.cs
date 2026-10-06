using System.Buffers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpEssentials.Enums;

namespace CSharpEssentials.Json;

/// <summary>
/// Reads and writes one enum type with <see cref="EnumInfo{TEnum}"/>. Created by <see cref="EnumConverterFactory"/>.
/// </summary>
internal sealed class EnumConverter<TEnum>(EnumInfo<TEnum> info, EnumConventions conventions, EnumReadMode mode, EnumWireFormat writeAs)
    : JsonConverter<TEnum> where TEnum : struct, Enum
{
    // Longest numeric token EnumNumberParser accepts: "-9223372036854775808" and "18446744073709551615".
    private const int MaxNumberLength = 20;
    private const int StackBufferLength = 256;
    private const int PreviewLength = 64;

    private readonly int _maxTokenLength = MaxSpelling(info);

    // 4.x flags text ("read, write") holds every flag once, with a separator and a space between them.
    private readonly int _maxLegacyFlagsLength = info.IsFlags ? (MaxSpelling(info) + 2) * Math.Max(info.TypedMembers.Count, 1) : 0;

    // Unknown text maps to the fallback member here, so a long value is not rejected up front.
    private readonly bool _acceptsAnyText =
        mode == EnumReadMode.Data && conventions.UnknownValue == UnknownEnumValueHandling.UseFallback && info.Fallback is not null;

    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray && info.IsFlags)
            return ReadFlagsArray(ref reader);
        return ReadScalar(ref reader, splitLegacyFlags: true);
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        if (writeAs == EnumWireFormat.Number)
        {
            WriteNumber(writer, value);
            return;
        }

        if (!info.IsFlags)
        {
            writer.WriteStringValue(info.Format(value, EnumWireFormat.String));
            return;
        }

        // Validate and decompose before anything is written, so an undefined value never leaves a partial array behind.
        List<string> names = [];
        info.FormatFlags(value, names);
        writer.WriteStartArray();
        foreach (string name in names)
            writer.WriteStringValue(name);
        writer.WriteEndArray();
    }

    public override TEnum ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        ReadString(ref reader, splitLegacyFlags: true);

    public override void WriteAsPropertyName(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options) =>
        writer.WritePropertyName(info.Format(value, writeAs));

    private TEnum ReadScalar(ref Utf8JsonReader reader, bool splitLegacyFlags)
    {
        if (reader.TokenType == JsonTokenType.Number)
            return ReadNumber(ref reader);
        if (reader.TokenType != JsonTokenType.String)
            throw new EnumValueJsonException(info.CreateError(null, mode));

        return ReadString(ref reader, splitLegacyFlags);
    }

    // The value is unescaped into a bounded buffer and looked up as a span; text longer than every accepted spelling is
    // rejected before it is unescaped, so a large string never becomes a string.
    private TEnum ReadString(ref Utf8JsonReader reader, bool splitLegacyFlags)
    {
        int limit = splitLegacyFlags && info.IsFlags ? _maxLegacyFlagsLength : _maxTokenLength;
        int byteLength = reader.HasValueSequence ? (int)Math.Min(reader.ValueSequence.Length, int.MaxValue) : reader.ValueSpan.Length;

        // One char is at most 3 UTF-8 bytes, or 6 bytes when it is escaped (\uXXXX).
        if (byteLength > limit * (reader.ValueIsEscaped ? 6 : 3))
        {
            if (_acceptsAnyText)
                return ReadText(reader.GetString().AsSpan());
            throw new EnumValueJsonException(info.CreateError(Preview(ref reader, byteLength), mode));
        }

        char[]? rented = byteLength > StackBufferLength ? ArrayPool<char>.Shared.Rent(byteLength) : null;
        Span<char> buffer = rented is null ? stackalloc char[StackBufferLength] : rented;
        try
        {
            ReadOnlySpan<char> text = buffer[..reader.CopyString(buffer)];
            if (text.Length > limit && !_acceptsAnyText)
                throw new EnumValueJsonException(info.CreateError(text.ToString(), mode));
            return splitLegacyFlags ? ReadText(text) : ReadToken(text);
        }
        finally
        {
            if (rented is not null)
                ArrayPool<char>.Shared.Return(rented);
        }
    }

    private TEnum ReadText(ReadOnlySpan<char> text) =>
        info.IsFlags && text.IndexOf(',') >= 0 ? ReadLegacyFlags(text) : ReadToken(text);

    private TEnum ReadToken(ReadOnlySpan<char> text) =>
        info.TryParse(text, mode, conventions, out TEnum value, out EnumValueError? error)
            ? value
            : throw new EnumValueJsonException(error);

    // 4.x wrote flags as "read, write"; every part is one accepted token.
    private TEnum ReadLegacyFlags(ReadOnlySpan<char> text)
    {
        ulong raw = 0;
        int start = 0;
        while (start <= text.Length)
        {
            int comma = text[start..].IndexOf(',');
            int end = comma < 0 ? text.Length : start + comma;
            if (!info.TryParse(text[start..end].Trim(), mode, conventions, out TEnum flag, out _))
                throw new EnumValueJsonException(info.CreateError(text.ToString(), mode));
            raw |= info.ToRawValue(flag);
            start = end + 1;
        }

        return info.FromRawValue(raw);
    }

    // The first bytes of a rejected value, for the error message; escapes are shown as written.
    private static string Preview(ref Utf8JsonReader reader, int byteLength)
    {
        int count = Math.Min(byteLength, PreviewLength);
        Span<byte> prefix = stackalloc byte[PreviewLength];
        if (reader.HasValueSequence)
            reader.ValueSequence.Slice(0, count).CopyTo(prefix);
        else
            reader.ValueSpan[..count].CopyTo(prefix);
        string text = Encoding.UTF8.GetString(prefix[..count]);
        return count < byteLength ? text + "…" : text;
    }

    private static int MaxSpelling(EnumInfo<TEnum> info)
    {
        int max = MaxNumberLength;
        foreach (EnumMemberInfo<TEnum> member in info.TypedMembers)
        {
            max = Math.Max(max, Math.Max(member.WireName.Length, member.MemberName.Length));
            foreach (string alias in member.Aliases)
                max = Math.Max(max, alias.Length);
        }

        return max;
    }

    private TEnum ReadNumber(ref Utf8JsonReader reader)
    {
        EnumValueError? error;
        if (reader.TryGetInt64(out long signed))
        {
            if (info.TryParseNumber(signed, mode, conventions, out TEnum value, out error))
                return value;
        }
        else if (reader.TryGetUInt64(out ulong unsigned))
        {
            if (info.TryParseNumber(unsigned, mode, conventions, out TEnum value, out error))
                return value;
        }
        else
        {
            byte[] utf8 = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan.ToArray();
            error = info.CreateError(Encoding.UTF8.GetString(utf8), mode);
        }

        throw new EnumValueJsonException(error);
    }

    private TEnum ReadFlagsArray(ref Utf8JsonReader reader)
    {
        ulong raw = 0;
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            raw |= info.ToRawValue(ReadScalar(ref reader, splitLegacyFlags: false));
        }

        return info.FromRawValue(raw);
    }

    // Format throws EnumValueException for an undefined value before anything is written; the text is an invariant integer.
    private void WriteNumber(Utf8JsonWriter writer, TEnum value) =>
        writer.WriteRawValue(info.Format(value, EnumWireFormat.Number), skipInputValidation: true);
}
