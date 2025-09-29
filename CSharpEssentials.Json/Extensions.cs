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
