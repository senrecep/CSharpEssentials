using System;
using System.Text.Json;
using CSharpEssentials.Errors;
using CSharpEssentials.Results;

namespace CSharpEssentials.Json;

public static class JsonExtensions
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

}
