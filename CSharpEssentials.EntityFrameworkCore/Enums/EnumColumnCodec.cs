using System.Buffers;
using System.Text;
using System.Text.Json;
using CSharpEssentials.Core;
using CSharpEssentials.Enums;

namespace CSharpEssentials.EntityFrameworkCore;

/// <summary>
/// Reads and writes one enum column: canonical writes, tolerant <see cref="EnumReadMode.Data"/> reads, errors that name the column.
/// </summary>
internal sealed class EnumColumnCodec<TEnum> where TEnum : struct, Enum
{
    private readonly EnumConventions _conventions;
    private readonly Dictionary<TEnum, string>? _legacyNames;
    private readonly Dictionary<string, TEnum>? _legacyLookup;

    public EnumColumnCodec(EnumInfo<TEnum> info, EnumConventions conventions, EnumStoredAs? legacyFormat = null, string? column = null)
    {
        Info = info ?? throw new ArgumentNullException(nameof(info));
        _conventions = conventions ?? throw new ArgumentNullException(nameof(conventions));
        if (legacyFormat == EnumStoredAs.Text)
            throw new ArgumentException($"{nameof(EnumStoredAs)}.{nameof(EnumStoredAs.Text)} is a conversion source, not a write format.", nameof(legacyFormat));

        Column = column;
        if (legacyFormat is EnumStoredAs.MemberName or EnumStoredAs.CamelCase or EnumStoredAs.LegacySnakeCase or EnumStoredAs.FlagsText)
        {
            KeyValuePair<TEnum, string>[] names =
                [.. info.TypedMembers.Select(member => KeyValuePair.Create(member.Value, LegacyName(member.MemberName, legacyFormat.Value)))];
            _legacyNames = names.DistinctBy(pair => pair.Key).ToDictionary();
            _legacyLookup = new Dictionary<string, TEnum>(
                names.DistinctBy(pair => pair.Value, StringComparer.OrdinalIgnoreCase).Select(pair => KeyValuePair.Create(pair.Value, pair.Key)),
                StringComparer.OrdinalIgnoreCase);
        }
    }

    public EnumInfo<TEnum> Info { get; }

    public string? Column { get; }

    public string ToText(TEnum value)
    {
        if (_legacyNames is null)
        {
            try
            {
                return Info.Format(value, EnumWireFormat.String);
            }
            catch (EnumValueException ex) when (NeedsColumn(ex))
            {
                throw WithColumn(ex);
            }
        }
        if (!Info.IsFlags)
            return _legacyNames.TryGetValue(value, out string? name) ? name : throw Undefined(value);

        string[] wireNames = ToArray(value);
        if (wireNames.Length == 0)
            return _legacyNames.TryGetValue(value, out string? zero) ? zero : "0";
        return string.Join(", ", wireNames.Select(wire => _legacyNames[Parse(wire)]));
    }

    public TEnum FromText(string text)
    {
        if (_legacyLookup is not null && _legacyLookup.TryGetValue(text, out TEnum legacy))
            return legacy;
        if (Info.IsFlags && text.Contains(','))
            return FromNames(text.Split(',').Select(part => part.Trim()));

        return Info.TryParse(text, EnumReadMode.Data, _conventions, out TEnum value, out EnumValueError? error) ? value : throw ReadError(error);
    }

    public ulong ToRaw(TEnum value) =>
        Info.IsDefined(value) ? Info.ToRawValue(value) : throw Undefined(value);

    public TEnum FromNumber(long number) =>
        Info.TryParseNumber(number, EnumReadMode.Data, _conventions, out TEnum value, out EnumValueError? error) ? value : throw ReadError(error);

    public TEnum FromNumber(ulong number) =>
        Info.TryParseNumber(number, EnumReadMode.Data, _conventions, out TEnum value, out EnumValueError? error) ? value : throw ReadError(error);

    public string[] ToArray(TEnum value)
    {
        try
        {
            List<string> names = [];
            Info.FormatFlags(value, names);
            return [.. names];
        }
        catch (EnumValueException ex) when (NeedsColumn(ex))
        {
            throw WithColumn(ex);
        }
    }

    public TEnum FromArray(string[] names) => FromNames(names);

    public string ToJsonArray(TEnum value)
    {
        ArrayBufferWriter<byte> buffer = new();
        using (Utf8JsonWriter writer = new(buffer))
        {
            writer.WriteStartArray();
            foreach (string name in ToArray(value))
                writer.WriteStringValue(name);
            writer.WriteEndArray();
        }

        return Encoding.UTF8.GetString(buffer.WrittenSpan);
    }

    public TEnum FromJsonArray(string text)
    {
        if (!text.StartsWith('['))
            return FromText(text);

        List<string> names = [];
        Utf8JsonReader reader = new(Encoding.UTF8.GetBytes(text));
        while (reader.Read())
        {
            if (reader.TokenType is JsonTokenType.String or JsonTokenType.Number)
                names.Add(reader.TokenType == JsonTokenType.String ? reader.GetString()! : Encoding.UTF8.GetString(reader.ValueSpan));
        }

        return FromNames(names);
    }

    public InvalidOperationException ReadError(EnumValueError error)
    {
        EnumValueError located = error with { Path = Column };
        return new InvalidOperationException(
            Column is null ? located.Message : $"Cannot read column {Column}: {located.Message}",
            new EnumValueException(located));
    }

    private TEnum FromNames(IEnumerable<string> names)
    {
        ulong raw = 0;
        foreach (string name in names)
            raw |= Info.ToRawValue(FromText(name));
        return Info.TryParseNumber(raw, EnumReadMode.Data, _conventions, out TEnum value, out EnumValueError? error)
            ? value
            : throw ReadError(error);
    }

    private TEnum Parse(string wireName) =>
        Info.TryParse(wireName, EnumReadMode.Data, _conventions, out TEnum value, out EnumValueError? error) ? value : throw ReadError(error);

    private bool NeedsColumn(EnumValueException ex) => Column is not null && ex.Error.Path is null;

    private EnumValueException WithColumn(EnumValueException ex) => new(ex.Error with { Path = Column });

    private EnumValueException Undefined(TEnum value) =>
        new(Info.CreateError(value.ToString(), EnumReadMode.Data, Column));

    internal static string LegacyName(string memberName, EnumStoredAs format)
    {
        if (format == EnumStoredAs.CamelCase)
            return JsonNamingPolicy.CamelCase.ConvertName(memberName);
        return format == EnumStoredAs.LegacySnakeCase ? memberName.ToSnakeCase() : memberName;
    }
}
