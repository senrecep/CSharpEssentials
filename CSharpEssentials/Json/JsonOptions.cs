using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSharpEssentials.Json;

public static class EnhancedJsonSerializerOptions
{
    /// <summary>
    /// The default JSON serializer options.
    /// </summary>
    public static readonly JsonSerializerOptions DefaultOptions = new(JsonSerializerDefaults.Web)
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        WriteIndented = false,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new ConditionalStringEnumConverter(),
            new MultiFormatDateTimeConverterFactory(),
        }
    };

    /// <summary>
    /// The default JSON serializer options without converters.
    /// </summary>
    public static readonly JsonSerializerOptions DefaultOptionsWithoutConverters = new(JsonSerializerDefaults.Web)
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        WriteIndented = false,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// The default JSON serializer options with a date time converter.
    /// </summary>
    public static readonly JsonSerializerOptions DefaultOptionsWithDateTimeConverter = new(JsonSerializerDefaults.Web)
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        WriteIndented = false,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new MultiFormatDateTimeConverterFactory() }
    };

    /// <summary>
    /// Creates JSON serializer options with the specified converters.
    /// </summary>
    /// <param name="converters"></param>
    /// <returns></returns>
    public static JsonSerializerOptions CreateOptionsWithConverters(params JsonConverter[] converters)
    {
        JsonSerializerOptions options = DefaultOptionsWithoutConverters.Create(options =>
        {
            foreach (JsonConverter converter in converters)
                options.Converters.Add(converter);
        });
        return options;
    }

    /// <summary>
    /// Creates JSON serializer options with the specified converters.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static JsonSerializerOptions Create(this JsonSerializerOptions options, Action<JsonSerializerOptions> configure)
    {
        var jsonSerializerOptions = new JsonSerializerOptions(options);
        configure(jsonSerializerOptions);
        return jsonSerializerOptions;

    }


    public static JsonSerializerOptions ApplyTo(this JsonSerializerOptions source, JsonSerializerOptions target)
    {
        target.ReferenceHandler = source.ReferenceHandler;
        target.WriteIndented = source.WriteIndented;
        target.PropertyNameCaseInsensitive = source.PropertyNameCaseInsensitive;
        target.PropertyNamingPolicy = source.PropertyNamingPolicy;
        target.Encoder = source.Encoder;
        target.DefaultIgnoreCondition = source.DefaultIgnoreCondition;
        target.DefaultBufferSize = source.DefaultBufferSize;
        target.IgnoreReadOnlyFields = source.IgnoreReadOnlyFields;
        target.IgnoreReadOnlyProperties = source.IgnoreReadOnlyProperties;
        target.ReadCommentHandling = source.ReadCommentHandling;
        target.AllowTrailingCommas = source.AllowTrailingCommas;
        target.WriteIndented = source.WriteIndented;
        target.Encoder = source.Encoder;
        target.NumberHandling = source.NumberHandling;
        target.MaxDepth = source.MaxDepth;


        foreach (JsonConverter converter in source.Converters)
        {
            if (target.Converters.Any(c => c.Type == converter.Type))
                continue;
            target.Converters.Add(converter);
        }

        return target;
    }

    public static JsonSerializerOptions ApplyFrom(this JsonSerializerOptions source, JsonSerializerOptions target) => target.ApplyTo(source);
}
