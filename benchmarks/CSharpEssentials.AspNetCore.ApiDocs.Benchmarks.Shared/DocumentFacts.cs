using System.Globalization;
using System.Text;
using System.Text.Json;

namespace CSharpEssentials.AspNetCore.ApiDocs.Benchmarks;

/// <summary>The parts of one OpenAPI document the parity checks compare, read once with <see cref="JsonDocument"/>.</summary>
internal sealed record DocumentFacts(
    string Hash,
    int Length,
    string Version,
    IReadOnlyList<string> Operations,
    bool PathParametersRequired,
    int ResourceSchemas,
    IReadOnlyDictionary<string, string> EnumComponents,
    IReadOnlyList<string> KindDefaults,
    int EnumVarnameMarkers,
    int IdempotencyKeyMarkers,
    IReadOnlyList<string> IdempotentOperations,
    IReadOnlyList<string> PartlyIdempotentOperations,
    string Enum01Shape,
    string StageShape)
{
    private static readonly string[] _methods = ["get", "put", "post", "delete", "options", "head", "patch", "trace"];

    public static DocumentFacts Read(byte[] body)
    {
        using var document = JsonDocument.Parse(body);
        JsonElement root = document.RootElement;
        string text = Encoding.UTF8.GetString(body);
        JsonElement schemas = root.GetProperty("components").GetProperty("schemas");

        var operations = new List<string>();
        var idempotent = new List<string>();
        var partly = new List<string>();
        bool pathRequired = true;
        foreach (JsonProperty path in root.GetProperty("paths").EnumerateObject())
        {
            foreach (JsonProperty method in path.Value.EnumerateObject().Where(static m => _methods.Contains(m.Name)))
            {
                string key = method.Name.ToUpperInvariant() + " " + path.Name;
                operations.Add(key);
                (bool keyHeader, bool replayed, int problems, bool pathOk) = Idempotency(root, method.Value);
                pathRequired &= pathOk;
                if (keyHeader && replayed && problems == 3)
                    idempotent.Add(key);
                if (keyHeader || replayed || problems > 0)
                    partly.Add(key);
            }
        }

        var enums = new SortedDictionary<string, string>(StringComparer.Ordinal);
        int resources = 0;
        var defaults = new List<string>();
        foreach (JsonProperty schema in schemas.EnumerateObject())
        {
            if (schema.Name.Length == 6 && schema.Name.StartsWith("Enum", StringComparison.Ordinal))
                enums[schema.Name] = EnumSummary(schema.Value);
            if (schema.Name.Length >= 10 && schema.Name.AsSpan(schema.Name.Length - 10).StartsWith("Resource", StringComparison.Ordinal))
            {
                resources++;
                defaults.Add(KindDefault(root, schema.Value));
            }
        }

        operations.Sort(StringComparer.Ordinal);
        idempotent.Sort(StringComparer.Ordinal);
        partly.Sort(StringComparer.Ordinal);
        return new DocumentFacts(
            Parity.Hash(body),
            body.Length,
            root.GetProperty("openapi").GetString() ?? string.Empty,
            operations,
            pathRequired,
            resources,
            enums,
            defaults,
            Count(text, "x-enum-varnames"),
            Count(text, "Idempotency-Key"),
            idempotent,
            partly,
            Enum01(root, schemas),
            Stage(schemas));
    }

    private static (bool KeyHeader, bool Replayed, int Problems, bool PathRequired) Idempotency(JsonElement root, JsonElement operation)
    {
        bool keyHeader = false;
        bool pathRequired = true;
        if (operation.TryGetProperty("parameters", out JsonElement parameters))
        {
            foreach (JsonElement parameter in parameters.EnumerateArray().Select(p => Resolve(root, p)))
            {
                string location = parameter.GetProperty("in").GetString() ?? string.Empty;
                string name = parameter.GetProperty("name").GetString() ?? string.Empty;
                keyHeader |= location == "header" && name == "Idempotency-Key";
                if (location == "path")
                    pathRequired &= parameter.TryGetProperty("required", out JsonElement required) && required.ValueKind == JsonValueKind.True;
            }
        }

        bool replayed = false;
        int problems = 0;
        foreach (JsonProperty response in operation.GetProperty("responses").EnumerateObject())
        {
            JsonElement value = Resolve(root, response.Value);
            if (response.Name.StartsWith('2') && value.TryGetProperty("headers", out JsonElement headers) && headers.TryGetProperty("Idempotency-Replayed", out _))
                replayed = true;
            if (response.Name is "400" or "409" or "422" && value.TryGetProperty("content", out JsonElement content) && content.TryGetProperty("application/problem+json", out _))
                problems++;
        }

        return (keyHeader, replayed, problems, pathRequired);
    }

    private static JsonElement Resolve(JsonElement root, JsonElement element)
    {
        if (!element.TryGetProperty("$ref", out JsonElement reference))
            return element;
        JsonElement current = root;
        foreach (string segment in (reference.GetString() ?? string.Empty).TrimStart('#', '/').Split('/'))
            current = current.GetProperty(segment);
        return current;
    }

    private static string EnumSummary(JsonElement schema) => string.Join('|',
        Member(schema, "type"),
        Member(schema, "enum"),
        Member(schema, "x-enum-varnames"),
        Member(schema, "x-enum-numeric-values"));

    private static string Member(JsonElement schema, string name)
    {
        if (!schema.TryGetProperty(name, out JsonElement value))
            return "-";
        if (value.ValueKind == JsonValueKind.Array)
            return string.Join(',', value.EnumerateArray().Select(Text));
        return Text(value);
    }

    private static string Text(JsonElement value) =>
        value.ValueKind == JsonValueKind.String ? value.GetString() ?? string.Empty : value.GetRawText();

    private static string KindDefault(JsonElement root, JsonElement schema)
    {
        if (!schema.TryGetProperty("properties", out JsonElement properties) || !properties.TryGetProperty("kind", out JsonElement kind))
            return "<no kind>";
        if (kind.TryGetProperty("default", out JsonElement value))
            return value.GetRawText();
        foreach (string group in new[] { "allOf", "oneOf", "anyOf" })
        {
            if (!kind.TryGetProperty(group, out JsonElement members))
                continue;
            foreach (JsonElement member in members.EnumerateArray())
            {
                if (member.TryGetProperty("default", out value))
                    return value.GetRawText();
            }
        }

        return Resolve(root, kind).TryGetProperty("default", out value) ? "component " + value.GetRawText() : "<none>";
    }

    private static string Enum01(JsonElement root, JsonElement schemas)
    {
        string component = schemas.TryGetProperty("Enum01", out JsonElement schema) ? Compact(schema) : "none";
        JsonElement list = root.GetProperty("paths").GetProperty("/resources01").GetProperty("get");
        JsonElement status = list.GetProperty("parameters").EnumerateArray().Select(p => Resolve(root, p)).First(static p => p.GetProperty("name").GetString() == "status");
        return $"component {component}; query status {Compact(status.GetProperty("schema"))}";
    }

    private static string Stage(JsonElement schemas)
    {
        JsonProperty resource = schemas.EnumerateObject().First(static s => s.Name.EndsWith("Resource01", StringComparison.Ordinal));
        return Compact(resource.Value.GetProperty("properties").GetProperty("stage"));
    }

    private static string Compact(JsonElement element)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
            element.WriteTo(writer);
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static int Count(string text, string value)
    {
        int count = 0;
        for (int index = text.IndexOf(value, StringComparison.Ordinal); index >= 0; index = text.IndexOf(value, index + value.Length, StringComparison.Ordinal))
            count++;
        return count;
    }

    public override string ToString() => string.Create(CultureInfo.InvariantCulture,
        $"{Length} bytes, openapi {Version}, {Operations.Count} operations, {EnumComponents.Count} enum components, {IdempotentOperations.Count} idempotent operations");
}
