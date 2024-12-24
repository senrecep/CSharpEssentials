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
        "yyyy-MM-ddTHH:mm:ss.ffffffzzz",
        "yyyy-MM-ddTHH:mm:ss.fffZ",
        "yyyy-MM-ddTHH:mm:ssZ",
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-dd HH:mm:ss",
        "yyyy-MM-dd",
        "yyyy/MM/dd",
        "dd-MM-yyyy",
        "dd/MM/yyyy",
        "MM/dd/yyyy",
        "dd.MM.yyyy",
        "MMMM yyyy",
        "MMM yyyy",
        "dd MMM yyyy",
        "d MMM yyyy",
        "MMM d, yyyy",
        "MMMM d, yyyy",
        "HH:mm:ss",
        "HH:mm",
        "yyyyMMdd",
        "yyyy-MM",
        "yyyy'W'ww",

        "yyyy-MM-ddTHH:mm:ss.ffffff",
        "yyyy-MM-ddTHH:mm:ss.fff",
        "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
        "ddd, dd-MMM-yy HH:mm:ss 'GMT'",
        "yyyy-MM-dd'T'HH:mm:ssXXX",
        "yyyy-MM-dd HH:mm:ss zzz",
        "dd MMM yyyy HH:mm:ss",
        "yyyy.MM.dd G 'at' HH:mm:ss z",
        "yyyyMMddHHmmss",
        "yyyy-MM-dd_HH-mm-ss",
        "yyyy年MM月dd日"
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
