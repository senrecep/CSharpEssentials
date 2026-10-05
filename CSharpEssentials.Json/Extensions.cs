using System.Text.Json;

namespace CSharpEssentials.Json;

public static class Extensions
{
    /// <summary>
    /// Tries to get a nested property from a JSON element.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static JsonDocument ConvertToJsonDocument<T>(this T data, JsonSerializerOptions? options = null) =>
        JsonSerializer.SerializeToDocument(data, options ?? EnhancedJsonSerializerOptions.DefaultOptions);

    /// <summary>
    /// Converts an object to a JSON string.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static string ConvertToJson<T>(this T data, JsonSerializerOptions? options = null) =>
        JsonSerializer.Serialize(data, options ?? EnhancedJsonSerializerOptions.DefaultOptions);
    /// <summary>
    /// Converts a JSON string to an object.
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    /// <param name="json"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static TClass? ConvertFromJson<TClass>(this string json, JsonSerializerOptions? options = null) =>
        JsonSerializer.Deserialize<TClass>(json, options ?? EnhancedJsonSerializerOptions.DefaultOptions);

    /// <summary>
    /// Converts a JSON string to an object.
    /// </summary>
    /// <param name="json"></param>
    /// <param name="returnType"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static object? ConvertFromJson(this string json, Type returnType, JsonSerializerOptions? options = null) =>
        JsonSerializer.Deserialize(json, returnType, options ?? EnhancedJsonSerializerOptions.DefaultOptions);

    /// <summary>
    /// Converts a JSON element to plain CLR values: objects become <see cref="Dictionary{TKey, TValue}"/>,
    /// arrays become <see cref="List{T}"/>, numbers become int, long, decimal or double (first that fits).
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    public static object? ToClrObject(this JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Object => ToClrDictionary(element),
        JsonValueKind.Array => ToClrList(element),
        JsonValueKind.String => element.GetString(),
        JsonValueKind.Number => ToClrNumber(element),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Null or JsonValueKind.Undefined => null,
        _ => throw new ArgumentOutOfRangeException(nameof(element), element.ValueKind, "Unsupported JSON value kind.")
    };

    private static Dictionary<string, object?> ToClrDictionary(JsonElement element)
    {
        var result = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (JsonProperty property in element.EnumerateObject())
            result[property.Name] = property.Value.ToClrObject();
        return result;
    }

    private static List<object?> ToClrList(JsonElement element)
    {
        var result = new List<object?>(element.GetArrayLength());
        foreach (JsonElement item in element.EnumerateArray())
            result.Add(item.ToClrObject());
        return result;
    }

    private static object ToClrNumber(JsonElement element)
    {
        if (element.TryGetInt32(out int intValue))
            return intValue;
        if (element.TryGetInt64(out long longValue))
            return longValue;
        if (element.TryGetDecimal(out decimal decimalValue))
            return decimalValue;
        return element.GetDouble();
    }

    /// <summary>
    /// Converts a JSON string to a JSON document.
    /// </summary>
    private static readonly Func<string, JsonDocument?>[] _deserializers = [
        json => JsonSerializer.Deserialize<JsonDocument>(json, EnhancedJsonSerializerOptions.DefaultOptions),
        json => JsonDocument.Parse(json),
        json => json.ConvertToJsonDocument(EnhancedJsonSerializerOptions.DefaultOptions)];

    /// <summary>
    /// Converts a JSON string to a JSON document.
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static JsonDocument? ConvertToJsonDocument(this string json)
    {
        foreach (Func<string, JsonDocument?> deserialize in _deserializers)
        {
            try
            {
                JsonDocument? document = deserialize(json);
                if (document != null)
                    return document;
            }
            catch (JsonException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        return null;
    }
}
