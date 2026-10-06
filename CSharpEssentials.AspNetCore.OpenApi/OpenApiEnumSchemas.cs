using System.Text.Json.Nodes;
using CSharpEssentials.Enums;
using Microsoft.OpenApi;

namespace CSharpEssentials.AspNetCore.OpenApi;

/// <summary>
/// Writes the enum component content and the usage shapes (nullable, array, default) in the Microsoft.OpenApi 2.x model.
/// Nullable branches on the document's OpenAPI version: <c>allOf</c> + <c>nullable: true</c> for 3.0,
/// <c>oneOf [$ref, {type: null}]</c> for 3.1.
/// </summary>
internal static class OpenApiEnumSchemas
{
    internal const string VarNamesExtension = "x-enum-varnames";
    internal const string DescriptionsExtension = "x-enum-descriptions";
    internal const string NumericValuesExtension = "x-enum-numeric-values";

    /// <summary>Makes <paramref name="schema"/> the component of the enum described by <paramref name="content"/>.</summary>
    public static void ApplyContent(OpenApiSchema schema, EnumSchemaContent content)
    {
        schema.Type = content.IsNumber ? JsonSchemaType.Integer : JsonSchemaType.String;
        schema.Format = content.IsNumber ? content.IntegerFormat : null;
        schema.Default = null;
        schema.OneOf = null;
        schema.AnyOf = null;
        schema.AllOf = null;
        IEnumerable<JsonNode> values = content.IsNumber
            ? content.NumericTexts.Select(Number)
            : content.WireNames.Select(static name => (JsonNode)JsonValue.Create(name));
        schema.Enum = content.HasEnumValues ? [.. values] : null;

        schema.Extensions ??= new Dictionary<string, IOpenApiExtension>(StringComparer.Ordinal);
        schema.Extensions[VarNamesExtension] = new JsonNodeExtension(Strings(content.VarNames));
        schema.Extensions[DescriptionsExtension] = new JsonNodeExtension(Strings(content.Descriptions));
        if (content.HasNumericValuesExtension)
            schema.Extensions[NumericValuesExtension] = new JsonNodeExtension(new JsonArray([.. content.NumericTexts.Select(Number)]));
        else
            schema.Extensions.Remove(NumericValuesExtension);

        schema.Description = content.AppendTable(schema.Description);
    }

    /// <summary>
    /// The schema of a usage of the enum whose component is <paramref name="reference"/>: nullable by the document version,
    /// flags (string format) and collections as arrays of the component, and <c>default</c> and the description of the usage on
    /// an <c>allOf</c> wrapper, never on the shared component.
    /// </summary>
    public static IOpenApiSchema CreateUsage(
        EnumUsage usage,
        EnumWireFormat format,
        IOpenApiSchema reference,
        IReadOnlyList<string>? defaults,
        string? description,
        OpenApiSpecVersion version)
    {
        IOpenApiSchema element = usage.IsFlagsArray(format)
            ? new OpenApiSchema { Type = JsonSchemaType.Array, Items = reference, UniqueItems = true }
            : reference;

        IOpenApiSchema result;
        if (usage.IsCollection)
        {
            IOpenApiSchema items = usage.ItemsNullable ? MakeNullable(element, version) : element;
            result = new OpenApiSchema { Type = JsonSchemaType.Array, Items = items };
        }
        else
        {
            result = usage.IsNullable ? MakeNullable(element, version) : element;
        }

        if (defaults is not null)
        {
            OpenApiSchema wrapper = Wrap(result);
            wrapper.Default = CreateDefault(usage, format, defaults);
            result = wrapper;
        }

        if (!string.IsNullOrWhiteSpace(description))
        {
            OpenApiSchema wrapper = Wrap(result);
            wrapper.Description = description;
            result = wrapper;
        }

        return result;
    }

    private static JsonNode CreateDefault(EnumUsage usage, EnumWireFormat format, IReadOnlyList<string> defaults)
    {
        if (usage.IsFlagsArray(format))
            return Strings(defaults);
        return format == EnumWireFormat.Number ? Number(defaults[0]) : JsonValue.Create(defaults[0]);
    }

    private static OpenApiSchema MakeNullable(IOpenApiSchema schema, OpenApiSpecVersion version)
    {
        if (schema is OpenApiSchema inline)
        {
            // An array of the usage (flags, collection): type [array, null] in 3.1, nullable: true in 3.0.
            inline.Type |= JsonSchemaType.Null;
            return inline;
        }

        return version >= OpenApiSpecVersion.OpenApi3_1
            ? new OpenApiSchema { OneOf = [schema, new OpenApiSchema { Type = JsonSchemaType.Null }] }
            : new OpenApiSchema { AllOf = [schema], Type = JsonSchemaType.Null };
    }

    // Keywords of the usage go on an allOf wrapper: a $ref cannot carry siblings in OpenAPI 3.0, and both versions stay alike.
    private static OpenApiSchema Wrap(IOpenApiSchema schema) =>
        schema as OpenApiSchema ?? new OpenApiSchema { AllOf = [schema] };

    private static JsonArray Strings(IEnumerable<string> values) => [.. values.Select(static value => (JsonNode)JsonValue.Create(value))];

    // A ulong value above long.MaxValue goes in as a decimal: the Microsoft.OpenApi 2.x writer has no ulong case and would
    // drop it from enum and write an empty default.
    private static JsonNode Number(string text) =>
        EnumUsage.TryParseNumber(text, out long number)
            ? JsonValue.Create(number)
            : JsonValue.Create(decimal.Parse(text, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture));
}
