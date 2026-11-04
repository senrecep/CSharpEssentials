using System.Buffers;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpEssentials.Enums;

namespace CSharpEssentials.Json;

/// <summary>
/// Reads and writes one enum type with <see cref="EnumInfo{TEnum}"/>. Created by <see cref="EnumConverterFactory"/>.
/// </summary>
/// <remarks>
/// An undefined value, including a <see cref="FlagsAttribute"/> value with bits no member covers, is written as its underlying
/// number (as a property name: the number text) in every <see cref="EnumWireFormat"/>. Reads do not mirror this: an undefined
/// number is rejected in <see cref="EnumReadMode.Input"/> mode and handled by <see cref="EnumConventions.UnknownValue"/> in
/// <see cref="EnumReadMode.Data"/> mode.
/// </remarks>
internal sealed class EnumConverter<TEnum>(EnumInfo<TEnum> info, EnumConventions conventions, EnumReadMode mode, EnumWireFormat writeAs)
    : JsonConverter<TEnum> where TEnum : struct, Enum
{
    // Longest numeric token EnumNumberParser accepts: "-9223372036854775808" and "18446744073709551615".
    private const int MaxNumberLength = 20;
    private const int StackBufferLength = 256;
    private const int PreviewLength = 64;

    // Mirrors EnumTypeTraits<TEnum>.IsSigned and FormatRaw, which are internal to CSharpEssentials.Enums.
    private static readonly bool IsSigned =
        Type.GetTypeCode(typeof(TEnum)) is TypeCode.SByte or TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64;

    private readonly int _maxTokenLength = MaxSpelling(info);

    private readonly Dictionary<ulong, JsonEncodedText> _encodedNames = EncodeNames(info);

    private readonly ulong[] _rawValues = [.. info.TypedMembers.Select(static m => m.RawValue)];

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
        ulong raw = info.ToRawValue(value);
        if (!info.IsFlags)
        {
            if (writeAs == EnumWireFormat.String && UsesStandardEncoder(writer) && _encodedNames.TryGetValue(raw, out JsonEncodedText name))
                writer.WriteStringValue(name);
            else if (!info.TryGetMember(value, out EnumMemberInfo<TEnum>? member))
                WriteNumber(writer, raw);
            else if (writeAs == EnumWireFormat.Number)
                writer.WriteRawValue(member.NumericText, skipInputValidation: true);
            else
                writer.WriteStringValue(member.WireName);
            return;
        }

        if (!info.IsDefined(value) || writeAs == EnumWireFormat.Number)
        {
            WriteNumber(writer, raw);
            return;
        }

        if (UsesStandardEncoder(writer) && WriteFlagNames(raw, writer: null))
        {
            writer.WriteStartArray();
            WriteFlagNames(raw, writer);
            writer.WriteEndArray();
            return;
        }

        List<string> names = [];
        info.FormatFlags(value, names);
        writer.WriteStartArray();
        foreach (string name in names)
            writer.WriteStringValue(name);
        writer.WriteEndArray();
    }

    public override TEnum ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        ReadString(ref reader, splitLegacyFlags: true);

    public override void WriteAsPropertyName(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        if (info.IsFlags)
            writer.WritePropertyName(info.IsDefined(value) ? info.Format(value, writeAs) : UndefinedNumber(value));
        else if (!info.TryGetMember(value, out EnumMemberInfo<TEnum>? member))
            writer.WritePropertyName(UndefinedNumber(value));
        else
            writer.WritePropertyName(writeAs == EnumWireFormat.Number ? member.NumericText : member.WireName);
    }

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
                return IsPlainUnknownText(ref reader, splitLegacyFlags) ? info.Fallback!.Value : ReadText(reader.GetString().AsSpan());
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

    // An over-long value is the fallback member unless it looks like a number (rejected as malformed), starts or ends with
    // whitespace (rejected) or is split into legacy flags. Only plain ASCII edges are decided from the bytes; anything else
    // is read as a string.
    private bool IsPlainUnknownText(ref Utf8JsonReader reader, bool splitLegacyFlags)
    {
        if (reader.ValueIsEscaped || splitLegacyFlags && info.IsFlags)
            return false;

        byte first;
        byte last;
        if (reader.HasValueSequence)
        {
            ReadOnlySequence<byte> sequence = reader.ValueSequence;
            Span<byte> edge = stackalloc byte[1];
            sequence.Slice(0, 1).CopyTo(edge);
            first = edge[0];
            sequence.Slice(sequence.Length - 1, 1).CopyTo(edge);
            last = edge[0];
        }
        else
        {
            first = reader.ValueSpan[0];
            last = reader.ValueSpan[^1];
        }

        return IsPlainEdge(first) && IsPlainEdge(last) && !(first is >= (byte)'0' and <= (byte)'9' or (byte)'-' or (byte)'+' or (byte)'.');
    }

    private static bool IsPlainEdge(byte value) => value is > 0x20 and < 0x7F;

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
            // "-0" is not canonical; the string form rejects it as well.
            if (signed == 0 && FirstByte(ref reader) == (byte)'-')
                error = info.CreateError("-0", mode);
            else if (info.TryParseNumber(signed, mode, conventions, out TEnum value, out error))
                return value;
        }
        else if (reader.TryGetUInt64(out ulong unsigned))
        {
            if (info.TryParseNumber(unsigned, mode, conventions, out TEnum value, out error))
                return value;
        }
        else
        {
            int byteLength = reader.HasValueSequence ? (int)Math.Min(reader.ValueSequence.Length, int.MaxValue) : reader.ValueSpan.Length;
            error = info.CreateError(Preview(ref reader, byteLength), mode);
        }

        throw new EnumValueJsonException(error);
    }

    private static byte FirstByte(ref Utf8JsonReader reader)
    {
        if (!reader.HasValueSequence)
            return reader.ValueSpan[0];
        Span<byte> first = stackalloc byte[1];
        reader.ValueSequence.Slice(0, 1).CopyTo(first);
        return first[0];
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

    private static void WriteNumber(Utf8JsonWriter writer, ulong raw)
    {
        if (IsSigned)
            writer.WriteNumberValue(unchecked((long)raw));
        else
            writer.WriteNumberValue(raw);
    }

    private static bool UsesStandardEncoder(Utf8JsonWriter writer) =>
        writer.Options.Encoder is null ||
        ReferenceEquals(writer.Options.Encoder, JavaScriptEncoder.Default) ||
        ReferenceEquals(writer.Options.Encoder, JavaScriptEncoder.UnsafeRelaxedJsonEscaping);

    /// <summary>
    /// The wire name of the member <see cref="EnumInfo{TEnum}.TryGetMember"/> returns for each value, for names the default
    /// encoder leaves unchanged; those are written the same by the relaxed encoder as well.
    /// </summary>
    private static Dictionary<ulong, JsonEncodedText> EncodeNames(EnumInfo<TEnum> info)
    {
        Dictionary<ulong, JsonEncodedText> names = [];
        foreach (EnumMemberInfo<TEnum> member in info.TypedMembers)
        {
            var encoded = JsonEncodedText.Encode(member.WireName);
            if (info.TryGetMember(member.Value, out EnumMemberInfo<TEnum>? first) && ReferenceEquals(first, member) && encoded.Value == member.WireName)
                names[member.RawValue] = encoded;
        }

        return names;
    }

    /// <summary>
    /// Mirrors <see cref="EnumInfo{TEnum}.FormatFlags"/>: single flags in declaration order, then the composites that cover
    /// the bits left. Returns <see langword="false"/> when a bit stays uncovered or a name is not pre-encoded; with no
    /// <paramref name="writer"/> it only checks.
    /// </summary>
    private bool WriteFlagNames(ulong raw, Utf8JsonWriter? writer)
    {
        ulong remaining = raw;
        foreach (ulong flag in _rawValues)
        {
            if (flag != 0 && (flag & (flag - 1)) == 0 && (remaining & flag) != 0 && !TakeFlags(flag, ref remaining, writer))
                return false;
        }

        foreach (ulong flags in _rawValues)
        {
            if (remaining == 0)
                break;
            if (flags != 0 && (raw & flags) == flags && (remaining & flags) != 0 && !TakeFlags(flags, ref remaining, writer))
                return false;
        }

        return remaining == 0;
    }

    private bool TakeFlags(ulong flags, ref ulong remaining, Utf8JsonWriter? writer)
    {
        if (!_encodedNames.TryGetValue(flags, out JsonEncodedText name))
            return false;
        writer?.WriteStringValue(name);
        remaining &= ~flags;
        return true;
    }

    private string UndefinedNumber(TEnum value)
    {
        ulong raw = info.ToRawValue(value);
        return IsSigned
            ? unchecked((long)raw).ToString(CultureInfo.InvariantCulture)
            : raw.ToString(CultureInfo.InvariantCulture);
    }
}
