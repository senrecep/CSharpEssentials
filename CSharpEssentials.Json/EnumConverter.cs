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
        ReadText(reader.GetString()!);

    public override void WriteAsPropertyName(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options) =>
        writer.WritePropertyName(info.Format(value, writeAs));

    private TEnum ReadScalar(ref Utf8JsonReader reader, bool splitLegacyFlags)
    {
        if (reader.TokenType == JsonTokenType.Number)
            return ReadNumber(ref reader);
        if (reader.TokenType != JsonTokenType.String)
            throw new EnumValueJsonException(info.CreateError(null, mode));

        string text = reader.GetString()!;
        return splitLegacyFlags ? ReadText(text) : ReadToken(text);
    }

    private TEnum ReadText(string text) =>
        info.IsFlags && text.Contains(',') ? ReadLegacyFlags(text) : ReadToken(text);

    private TEnum ReadToken(string text) =>
        info.TryParse(text, mode, conventions, out TEnum value, out EnumValueError? error)
            ? value
            : throw new EnumValueJsonException(error);

    // 4.x wrote flags as "read, write"; every part is one accepted token.
    private TEnum ReadLegacyFlags(string text)
    {
        ulong raw = 0;
        foreach (string part in text.Split(','))
        {
            if (!info.TryParse(part.Trim(), mode, conventions, out TEnum flag, out _))
                throw new EnumValueJsonException(info.CreateError(text, mode));
            raw |= info.ToRawValue(flag);
        }

        return info.FromRawValue(raw);
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
