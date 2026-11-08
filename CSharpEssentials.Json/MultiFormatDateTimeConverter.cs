using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSharpEssentials.Json;

#pragma warning disable IDE0370
/// <summary>
/// A multi-format date time converter factory.
/// </summary>
public sealed class MultiFormatDateTimeConverterFactory : JsonConverterFactory
{
    private static readonly Type _dateTimeType = typeof(DateTime);
    private static readonly Type _nullableDateTimeType = typeof(DateTime?);
    private readonly string[] _formats;


    public MultiFormatDateTimeConverterFactory(params string[] formats) =>
        _formats = [.. DefaultFormats, .. formats];


    public override bool CanConvert(Type typeToConvert) => typeToConvert == _dateTimeType || typeToConvert == _nullableDateTimeType;

    /// <summary>
    /// Creates a converter for the specified type.
    /// </summary>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return typeToConvert == _nullableDateTimeType
            ? new MultiFormatDateTimeConverter<DateTime?>(true, _formats)
            : new MultiFormatDateTimeConverter<DateTime>(false, _formats);
    }

    internal static readonly string[] DefaultFormats = [
        // ISO 8601 and Web Formats
        "yyyy-MM-ddTHH:mm:ss.ffffffzzz",    // 2024-03-14T15:30:45.123456+03:00
        "yyyy-MM-ddTHH:mm:ss.FFFFFFFK",     // 2024-03-14T15:30:45.1234567Z, the round-trip form System.Text.Json writes
        "yyyy-MM-ddTHH:mm:ss.fffZ",         // 2024-03-14T15:30:45.123Z
        "yyyy-MM-ddTHH:mm:ssZ",             // 2024-03-14T15:30:45Z
        "yyyy-MM-ddTHH:mm:ss",              // 2024-03-14T15:30:45
        "yyyy-MM-dd HH:mm:ss",              // 2024-03-14 15:30:45
        
        // Basic Date Formats
        "yyyy-MM-dd",                       // 2024-03-14
        "dd.MM.yyyy",                       // 14.03.2024
        "dd/MM/yyyy",                       // 14/03/2024
        "MM/dd/yyyy",                       // 03/14/2024 (US format)
        "yyyy/MM/dd",                       // 2024/03/14
        "dd-MM-yyyy",                       // 14-03-2024
        
        // Human Readable Formats
        "d MMMM yyyy",                      // 14 March 2024
        "MMMM d, yyyy",                     // March 14, 2024
        "dd MMM yyyy",                      // 14 Mar 2024
        "MMM d, yyyy",                      // Mar 14, 2024
        
        // Compact Formats
        "yyyyMMdd",                         // 20240314
        "yyyy-MM",                          // 2024-03
        
        // Detailed ISO Formats
        "yyyy-MM-ddTHH:mm:ss.ffffff",       // 2024-03-14T15:30:45.123456
        "yyyy-MM-ddTHH:mm:ss.fff",          // 2024-03-14T15:30:45.123
        
        // RFC and Email Formats
        "ddd, dd MMM yyyy HH:mm:ss 'GMT'",  // Thu, 14 Mar 2024 15:30:45 GMT
        "ddd, dd-MMM-yy HH:mm:ss 'GMT'",    // Thu, 14-Mar-24 15:30:45 GMT
        "yyyy-MM-dd HH:mm:ss zzz",          // 2024-03-14 15:30:45 +03:00
        "dd MMM yyyy HH:mm:ss",             // 14 Mar 2024 15:30:45
        
        // Compact Timestamp Formats
        "yyyyMMddHHmmss",                   // 20240314153045
        "yyyy-MM-dd_HH-mm-ss",              // 2024-03-14_15-30-45
        
        // International and Special Formats
        "yyyy年MM月dd日",                    // 2024年03月14日 (Japanese)
        "dd-MMM-yyyy",                      // 14-Mar-2024
        "yyyy.MM.dd HH:mm",                 // 2024.03.14 15:30
        "dd.MM.yyyy HH:mm",                 // 14.03.2024 15:30
        "yyyy-MM-dd HH.mm",                 // 2024-03-14 15.30
        "dd/MM/yyyy HH:mm:ss",              // 14/03/2024 15:30:45
        "MM/dd/yyyy HH:mm:ss",              // 03/14/2024 15:30:45 (US format with time)
        
        // Timezone-aware Formats
        "yyyy-MM-ddTHH:mm:sszzz",           // 2024-03-14T15:30:45+03:00
        
        // Unix and SQL Formats
        "yyyy-MM-dd HH:mm:ss.fff"           // 2024-03-14 15:30:45.123 (SQL)
    ];
}

/// <summary> 
/// A multi-format date time converter.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class MultiFormatDateTimeConverter<T> : JsonConverter<T>
{
    private const string _defaultFormat = "yyyy-MM-ddTHH:mm:ss.ffffffzzz";
    private const long _minUnixSeconds = -62135596800;
    private const long _maxUnixSeconds = 253402300799;
    private readonly string[] _formats;
    private readonly string _supportedFormats;
    private readonly bool _isNullable;
    private readonly bool _readsIso;

    public MultiFormatDateTimeConverter(bool isNullable, string[] formats)
    {
        _isNullable = isNullable;
        _formats = formats;
        _readsIso = formats.AsSpan().StartsWith(MultiFormatDateTimeConverterFactory.DefaultFormats);
        _supportedFormats = string.Join(", ", _formats);
    }

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            if (_isNullable)
                return default!;
            throw new JsonException("Null value is not allowed for non-nullable DateTime.");
        }

        if (_readsIso && TryReadIso(ref reader, out DateTime iso))
            return (T)(object)iso;

        string? dateString = reader.GetString();
        if (string.IsNullOrEmpty(dateString))
        {
            if (_isNullable)
                return default!;
            throw new JsonException("Empty or null string cannot be converted to DateTime.");
        }

        foreach (string format in _formats)
        {
            if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime dateTime))
                return (T)(object)dateTime;
        }

        // Try Unix timestamp as fallback (pure numeric string)
        if (long.TryParse(dateString, NumberStyles.Integer, CultureInfo.InvariantCulture, out long unixSeconds) &&
            unixSeconds >= _minUnixSeconds && unixSeconds <= _maxUnixSeconds)
        {
            DateTime unixDate = DateTimeOffset.FromUnixTimeSeconds(unixSeconds).UtcDateTime;
            return (T)(object)unixDate;
        }

        throw new JsonException($"Unable to convert \"{dateString}\" to DateTime. Supported formats: {_supportedFormats}.");
    }

    /// <summary>
    /// Reads the ISO 8601 shapes of the leading default formats without allocating. Only text in exactly those shapes is taken,
    /// so the result is the one the format list gives; anything else, or text the reader rejects, goes through the list.
    /// </summary>
    private static bool TryReadIso(ref Utf8JsonReader reader, out DateTime value)
    {
        value = default;
        if (reader.TokenType != JsonTokenType.String || reader.HasValueSequence || reader.ValueIsEscaped)
            return false;

        ReadOnlySpan<byte> text = reader.ValueSpan;
        if (!IsIsoShape(text, out bool hasZone))
            return false;

        if (hasZone)
        {
            if (!reader.TryGetDateTimeOffset(out DateTimeOffset withOffset))
                return false;
            value = withOffset.UtcDateTime;
            return true;
        }

        if (!reader.TryGetDateTime(out DateTime unspecified))
            return false;
        value = DateTime.SpecifyKind(unspecified, DateTimeKind.Utc);
        return true;
    }

    private static bool IsIsoShape(ReadOnlySpan<byte> s, out bool hasZone)
    {
        hasZone = false;
        if (s.Length < 10 || s[4] != (byte)'-' || s[7] != (byte)'-')
            return false;
        if (s.Length == 10)
            return true;
        if (s.Length < 19 || s[10] != (byte)'T' || s[13] != (byte)':' || s[16] != (byte)':' || !IsDigit(s[18]))
            return false;

        int index = 19;
        if (index < s.Length && s[index] == (byte)'.')
        {
            int start = ++index;
            while (index < s.Length && IsDigit(s[index]))
                index++;
            if (index - start is < 1 or > 7)
                return false;
        }

        ReadOnlySpan<byte> zone = s[index..];
        hasZone = zone.Length > 0;
        return zone.Length switch
        {
            0 => true,
            1 => zone[0] == (byte)'Z',
            6 => IsSign(zone[0]) && zone[3] == (byte)':',
            _ => false,
        };
    }

    private static bool IsDigit(byte value) => value is >= (byte)'0' and <= (byte)'9';

    private static bool IsSign(byte value) => value is (byte)'+' or (byte)'-';

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var dateTime = value as DateTime?;
        if (dateTime.HasValue)
        {
            DateTime toWrite = dateTime.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc)
                : dateTime.Value;
            writer.WriteStringValue(toWrite.ToString(_defaultFormat, CultureInfo.InvariantCulture));
        }
        else if (_isNullable)
        {
            writer.WriteNullValue();
        }
        else
        {
            throw new JsonException("Non-nullable DateTime cannot be null.");
        }
    }
}
