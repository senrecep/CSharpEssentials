using System.Text.Json;

namespace CSharpEssentials.Json;

public static class Extensions
{
    /// <summary>
    /// Tries to get a property from a JSON element.
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <param name="propNames"></param>
    /// <returns></returns>
    public static Result<JsonElement> TryGetProperty(this JsonElement jsonElement, params string[] propNames)
    {
        if (propNames == null || propNames.Length == 0)
            return Error.Validation("NoPropertyNames", "At least one property name must be provided.");

        foreach (string name in propNames)
        {
            if (jsonElement.TryGetProperty(name, out JsonElement prop))
                return prop;
        }

        return Error.NotFound("PropertyNotFound", $"None of the specified property names were found. Checked properties: {string.Join(", ", propNames)}");
    }

    /// <summary>
    /// Tries to get a nested property from a JSON document.
    /// </summary>
    /// <param name="document"></param>
    /// <param name="propNames"></param>
    /// <returns></returns>
    public static Result<JsonElement?> TryGetNestedProperty(this JsonDocument document, params string[] propNames)
    {
        if (document == null || document.RootElement.ValueKind == JsonValueKind.Null)
            return Error.Validation("DocumentIsNull", "The document is null or has no root element.");

        if (propNames == null || propNames.Length == 0)
            return Error.Validation("NoPropertyNames", "At least one property name must be provided.");

        JsonElement value = document.RootElement;

        foreach (string propName in propNames)
        {
            if (value.TryGetProperty(propName, out JsonElement nestedProperty))
                value = nestedProperty;
            else
                return Error.NotFound("PropertyNotFound", $"The specified property name '{propName}' was not found. Checked properties: {string.Join(", ", propNames)}");
        }

        return value;
    }

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
    /// Converts a JSON string to a JSON document.
    /// </summary>
    private static readonly Func<string, JsonDocument?>[] _deserializers = [
        json => JsonSerializer.Deserialize<JsonDocument>(json, EnhancedJsonSerializerOptions.DefaultOptions),
        json => JsonDocument.Parse(json),
        json => json.ConvertToJsonDocument()];

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
