using System.Text.Json;
using System.Text.Json.Nodes;

namespace CSharpEssentials.Tests.Fixtures.OpenApiSample;

/// <summary>
/// The enum part of an OpenAPI document, normalized so Swashbuckle (Microsoft.OpenApi 1.x) and Microsoft.AspNetCore.OpenApi
/// (Microsoft.OpenApi 2.x) produce the same text: the enum components, the enum properties of object components, and per
/// operation the enum parameters, bodies, responses and markers. Keys are sorted; serializer defaults (<c>style: form</c>,
/// <c>explode: true</c> of a query parameter) and framework-only keys are dropped.
/// </summary>
public static class OpenApiGolden
{
    private const string ComponentPrefix = "#/components/schemas/";

    private static readonly JsonSerializerOptions _indented = new() { WriteIndented = true };

    private static readonly string[] _methods = ["get", "put", "post", "delete", "patch"];

    /// <summary>The directory of the golden files, <c>tests/golden/openapi</c> of the repository.</summary>
    public static string Directory
    {
        get
        {
            DirectoryInfo? directory = new(AppContext.BaseDirectory);
            while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "CSharpEssentials.slnx")))
                directory = directory.Parent;
            if (directory is null)
                throw new InvalidOperationException("CSharpEssentials.slnx was not found above " + AppContext.BaseDirectory);
            return Path.Combine(directory.FullName, "tests", "golden", "openapi");
        }
    }

    /// <summary>The golden file of a document: <c>{document}.json</c> (OpenAPI 3.0, both packages) or <c>{document}.{suffix}.json</c>.</summary>
    public static string PathOf(string document, string? suffix = null) =>
        Path.Combine(Directory, suffix is null ? $"{document}.json" : $"{document}.{suffix}.json");

    /// <summary>The normalized enum extract of <paramref name="openApiJson"/>, indented, with <c>\n</c> line ends.</summary>
    public static string Extract(string openApiJson)
    {
        JsonNode root = JsonNode.Parse(openApiJson) ?? throw new ArgumentException("Not a JSON document.", nameof(openApiJson));
        JsonObject schemas = root["components"]?["schemas"] as JsonObject ?? [];
        HashSet<string> enumIds = [.. schemas.Where(static pair => pair.Value is JsonObject schema && schema.ContainsKey("x-enum-varnames")).Select(static pair => pair.Key)];

        var components = new JsonObject();
        foreach ((string id, JsonNode? schema) in schemas.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
        {
            if (enumIds.Contains(id))
            {
                components[id] = Normalize(schema);
            }
            else if (schema?["properties"] is JsonObject properties)
            {
                var enumProperties = new JsonObject();
                foreach ((string name, JsonNode? property) in properties.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
                {
                    if (UsesEnum(property, enumIds))
                        enumProperties[name] = Normalize(property);
                }

                if (enumProperties.Count > 0)
                    components[id] = new JsonObject { ["properties"] = enumProperties };
            }
        }

        var operations = new JsonObject();
        if (root["paths"] is JsonObject paths)
        {
            foreach ((string path, JsonNode? item) in paths.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
            {
                foreach (string method in _methods)
                {
                    if (item?[method] is JsonObject operation && ExtractOperation(operation, enumIds) is { Count: > 0 } extract)
                        operations[$"{method.ToUpperInvariant()} {path}"] = extract;
                }
            }
        }

        var result = new JsonObject { ["components"] = components, ["operations"] = operations };
        return result.ToJsonString(_indented).Replace("\r\n", "\n", StringComparison.Ordinal) + "\n";
    }

    private static JsonObject ExtractOperation(JsonObject operation, HashSet<string> enumIds)
    {
        var extract = new JsonObject();
        if (operation["parameters"] is JsonArray parameters)
        {
            var enumParameters = new JsonObject();
            foreach (JsonNode? parameter in parameters.OrderBy(static node => (string?)node?["name"], StringComparer.Ordinal))
            {
                if (parameter is null || !UsesEnum(parameter["schema"], enumIds))
                    continue;
                var copy = new JsonObject { ["in"] = parameter["in"]?.DeepClone(), ["schema"] = Normalize(parameter["schema"]) };
                if (parameter["style"] is { } style && !((string?)parameter["in"] == "query" && (string?)style == "form"))
                    copy["style"] = style.DeepClone();
                if (parameter["explode"] is { } explode && !((string?)parameter["in"] == "query" && (bool)explode))
                    copy["explode"] = explode.DeepClone();
                enumParameters[(string)parameter["name"]!] = copy;
            }

            if (enumParameters.Count > 0)
                extract["parameters"] = enumParameters;
        }

        if (JsonSchemaOf(operation["requestBody"]) is { } body && UsesEnum(body, enumIds))
            extract["requestBody"] = Normalize(body);

        if (operation["responses"] is JsonObject responses)
        {
            var enumResponses = new JsonObject();
            foreach ((string status, JsonNode? response) in responses.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
            {
                if (JsonSchemaOf(response) is { } schema && UsesEnum(schema, enumIds))
                    enumResponses[status] = Normalize(schema);
            }

            if (enumResponses.Count > 0)
                extract["responses"] = enumResponses;
        }

        foreach (string marker in (string[])["x-enum-wire-format", "x-enum-wire-format-header"])
        {
            if (operation[marker] is { } value)
                extract[marker] = value.DeepClone();
        }

        if ((string?)operation["description"] is { Length: > 0 } description && description.Contains("enum", StringComparison.OrdinalIgnoreCase))
            extract["description"] = description;
        return extract;
    }

    private static JsonNode? JsonSchemaOf(JsonNode? bodyOrResponse) => bodyOrResponse?["content"]?["application/json"]?["schema"];

    private static bool UsesEnum(JsonNode? schema, HashSet<string> enumIds) => schema switch
    {
        JsonObject obj => obj.Any(pair => pair.Key == "$ref"
            ? enumIds.Contains(((string?)pair.Value ?? string.Empty).Replace(ComponentPrefix, string.Empty, StringComparison.Ordinal))
            : pair.Key is "allOf" or "oneOf" or "anyOf" or "items" && UsesEnum(pair.Value, enumIds)),
        JsonArray array => array.Any(item => UsesEnum(item, enumIds)),
        _ => false,
    };

    internal static JsonNode? Normalize(JsonNode? node) => node switch
    {
        JsonObject obj => Sorted(obj),
        JsonArray array => new JsonArray([.. array.Select(Normalize)]),
        null => null,
        _ => NormalizeValue(node),
    };

    private static JsonObject Sorted(JsonObject obj)
    {
        var sorted = new JsonObject();
        foreach ((string key, JsonNode? value) in obj.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
            sorted[key] = Normalize(value);
        return sorted;
    }

    // 1.x and 2.x write the same numbers with different CLR types (OpenApiLong, long, decimal): compare them as text.
    private static JsonNode NormalizeValue(JsonNode value) =>
        value.GetValueKind() == JsonValueKind.Number ? JsonValue.Create(decimal.Parse(value.ToJsonString(), System.Globalization.CultureInfo.InvariantCulture)) : value.DeepClone();
}
