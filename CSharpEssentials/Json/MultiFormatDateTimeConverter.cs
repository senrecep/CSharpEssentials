using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSharpEssentials.Json;

/// <summary>
/// A multi-format date time converter factory.
/// </summary>
/// <param name="formats"></param>
public sealed class MultiFormatDateTimeConverterFactory(params string[] formats) : JsonConverterFactory
{
    private static readonly Type _dateTimeType = typeof(DateTime);
    private static readonly Type _nullableDateTimeType = typeof(DateTime?);
    private static readonly Type _convertType = typeof(MultiFormatDateTimeConverter<>);
    private readonly string[] _formats = [.. _defaultFormats, .. formats];

    public override bool CanConvert(Type typeToConvert) => typeToConvert == _dateTimeType || typeToConvert == _nullableDateTimeType;

    /// <summary>
    /// Creates a converter for the specified type.
    /// </summary>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        bool isNullable = typeToConvert == _nullableDateTimeType;

        return (JsonConverter)Activator.CreateInstance(
            _convertType.MakeGenericType(typeToConvert),
            isNullable,
            _formats
        )!;
    }

    private static readonly string[] _defaultFormats =
    [
        // ISO 8601 and Web Formats
        "yyyy-MM-ddTHH:mm:ss.ffffffzzz",    // 2024-03-14T15:30:45.123456+03:00
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
        "yyyy'W'ww",                        // 2024W11 (Week 11)
        
        // Detailed ISO Formats
        "yyyy-MM-ddTHH:mm:ss.ffffff",       // 2024-03-14T15:30:45.123456
        "yyyy-MM-ddTHH:mm:ss.fff",          // 2024-03-14T15:30:45.123
        
        // RFC and Email Formats
        "ddd, dd MMM yyyy HH:mm:ss 'GMT'",  // Thu, 14 Mar 2024 15:30:45 GMT
        "ddd, dd-MMM-yy HH:mm:ss 'GMT'",    // Thu, 14-Mar-24 15:30:45 GMT
        "yyyy-MM-dd'T'HH:mm:ssXXX",         // 2024-03-14T15:30:45+03:00
        "yyyy-MM-dd HH:mm:ss zzz",          // 2024-03-14 15:30:45 +03:00
        "dd MMM yyyy HH:mm:ss",             // 14 Mar 2024 15:30:45
        "yyyy.MM.dd G 'at' HH:mm:ss z",     // 2024.03.14 AD at 15:30:45 GMT
        
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
        
        // Week-Based Formats
        "yyyy'W'ww",                        // 2024W11
        "yyyy-'W'ww-d",                     // 2024-W11-4 (4th day of week 11)
        
        // Unix and SQL Formats
        "U",                                // 1710424245 (Unix timestamp)
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
    private readonly string[] _formats;
    private readonly string _supportedFormats;
    private readonly bool _isNullable;

    public MultiFormatDateTimeConverter(bool isNullable, string[] formats)
    {
        _isNullable = isNullable;
        _formats = formats;
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

        string? dateString = reader.GetString();
        if (string.IsNullOrEmpty(dateString))
        {
            if (_isNullable)
                return default!;
            throw new JsonException("Empty or null string cannot be converted to DateTime.");
        }

        foreach (string format in _formats)
        {
            if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime dateTime))
                return (T)(object)dateTime;
        }

        throw new JsonException($"Unable to convert \"{dateString}\" to DateTime. Supported formats: {_supportedFormats}.");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var dateTime = value as DateTime?;
        if (dateTime.HasValue)
        {
            writer.WriteStringValue(dateTime.Value.ToString(_defaultFormat, CultureInfo.InvariantCulture));
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
