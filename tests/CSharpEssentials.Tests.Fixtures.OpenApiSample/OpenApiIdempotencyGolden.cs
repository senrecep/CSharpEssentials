using System.Text.Json;
using System.Text.Json.Nodes;

namespace CSharpEssentials.Tests.Fixtures.OpenApiSample;

/// <summary>
/// The idempotency part of an OpenAPI document, normalized so Swashbuckle and Microsoft.AspNetCore.OpenApi produce the same
/// text: per operation the header parameters, the headers of the 2xx responses, and the description, headers and content of
/// the other responses. Keys are sorted. Operations without header parameters or such responses are left out.
/// </summary>
public static class OpenApiIdempotencyGolden
{
    private static readonly JsonSerializerOptions _indented = new() { WriteIndented = true };

    private static readonly string[] _methods = ["get", "put", "post", "delete", "patch"];

    /// <summary>The golden file name of the document, in <see cref="OpenApiGolden.Directory"/>.</summary>
    public static string PathOf(string? suffix = null) =>
        OpenApiGolden.PathOf("idempotency-api", suffix);

    /// <summary>The normalized extract of <paramref name="openApiJson"/>, indented, with <c>\n</c> line ends.</summary>
    public static string Extract(string openApiJson)
    {
        JsonNode root = JsonNode.Parse(openApiJson) ?? throw new ArgumentException("Not a JSON document.", nameof(openApiJson));
        var operations = new JsonObject();
        if (root["paths"] is JsonObject paths)
        {
            foreach ((string path, JsonNode? item) in paths.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
            {
                foreach (string method in _methods)
                {
                    if (item?[method] is JsonObject operation && ExtractOperation(operation) is { Count: > 0 } extract)
                        operations[$"{method.ToUpperInvariant()} {path}"] = extract;
                }
            }
        }

        return operations.ToJsonString(_indented).Replace("\r\n", "\n", StringComparison.Ordinal) + "\n";
    }

    private static JsonObject ExtractOperation(JsonObject operation)
    {
        var extract = new JsonObject();
        if (operation["parameters"] is JsonArray parameters)
        {
            var headers = new JsonArray();
            foreach (JsonNode? parameter in parameters.Where(static node => (string?)node?["in"] == "header").OrderBy(static node => (string?)node?["name"], StringComparer.Ordinal))
                headers.Add(OpenApiGolden.Normalize(parameter));
            if (headers.Count > 0)
                extract["parameters"] = headers;
        }

        if (operation["responses"] is JsonObject responses)
        {
            var described = new JsonObject();
            foreach ((string status, JsonNode? response) in responses.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
            {
                if (status.StartsWith('2'))
                {
                    if (response?["headers"] is { } headers)
                        described[status] = new JsonObject { ["headers"] = OpenApiGolden.Normalize(headers) };
                }
                else if (status is "400" or "409" or "422")
                {
                    var problem = new JsonObject { ["description"] = response?["description"]?.DeepClone() };
                    if (response?["headers"] is { } problemHeaders)
                        problem["headers"] = OpenApiGolden.Normalize(problemHeaders);
                    problem["content"] = OpenApiGolden.Normalize(response?["content"]);
                    described[status] = problem;
                }
            }

            if (described.Count > 0)
                extract["responses"] = described;
        }

        return extract;
    }
}
