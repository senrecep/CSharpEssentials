using System.Text.Json.Nodes;

namespace CSharpEssentials.Tests.Fixtures.OpenApiSample;

/// <summary>Reads the parts of an OpenAPI 3.0 document that a <see cref="CrossLayerRow"/> states, normalized like <see cref="OpenApiGolden"/>.</summary>
public static class CrossLayerOpenApi
{
    /// <summary>The schema of the <c>value</c> parameter of <c>GET /golden/{shape}/query</c>.</summary>
    public static string Parameter(string openApiJson, CrossLayerRow row)
    {
        ArgumentNullException.ThrowIfNull(row);

        JsonNode root = Parse(openApiJson);
        JsonArray parameters = root["paths"]?[$"/golden/{row.Shape}/query"]?["get"]?["parameters"] as JsonArray
            ?? throw new InvalidOperationException($"The document has no parameters for GET /golden/{row.Shape}/query.");
        JsonNode parameter = parameters.Single(static node => (string?)node?["name"] == "value")!;
        return OpenApiGolden.Normalize(parameter["schema"])!.ToJsonString();
    }

    /// <summary>The <c>type</c> and <c>enum</c> of the component of <see cref="CrossLayerRow.EnumType"/>.</summary>
    public static string Component(string openApiJson, CrossLayerRow row)
    {
        ArgumentNullException.ThrowIfNull(row);

        JsonNode component = Parse(openApiJson)["components"]?["schemas"]?[row.EnumType.Name]
            ?? throw new InvalidOperationException($"The document has no component {row.EnumType.Name}.");
        var extract = new JsonObject();
        foreach (string key in (string[])["enum", "type"])
        {
            if (component[key] is { } value)
                extract[key] = value.DeepClone();
        }

        return OpenApiGolden.Normalize(extract)!.ToJsonString();
    }

    private static JsonNode Parse(string openApiJson) =>
        JsonNode.Parse(openApiJson) ?? throw new ArgumentException("Not a JSON document.", nameof(openApiJson));
}
