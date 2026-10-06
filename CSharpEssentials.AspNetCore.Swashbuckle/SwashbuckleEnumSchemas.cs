using System.Globalization;
using CSharpEssentials.Enums;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace CSharpEssentials.AspNetCore.Swagger.Filters;

/// <summary>
/// Writes the enum component content and the usage shapes (nullable, array, default) in the Microsoft.OpenApi 1.x model.
/// Swashbuckle writes OpenAPI 3.0, so nullable is <c>allOf</c> + <c>nullable: true</c>.
/// </summary>
internal static class SwashbuckleEnumSchemas
{
    internal const string VarNamesExtension = "x-enum-varnames";
    internal const string DescriptionsExtension = "x-enum-descriptions";
    internal const string NumericValuesExtension = "x-enum-numeric-values";

    /// <summary>Makes <paramref name="schema"/> the component of the enum described by <paramref name="content"/>.</summary>
    public static void ApplyContent(OpenApiSchema schema, EnumSchemaContent content)
    {
        schema.Type = content.IsNumber ? "integer" : "string";
        schema.Format = content.IsNumber ? content.IntegerFormat : null;
        schema.Nullable = false;
        schema.Default = null;
        IEnumerable<IOpenApiAny> values = content.IsNumber
            ? content.NumericTexts.Select(Number)
            : content.WireNames.Select(static name => (IOpenApiAny)new OpenApiString(name));
        schema.Enum = content.HasEnumValues ? [.. values] : [];
        schema.Extensions[VarNamesExtension] = Strings(content.VarNames);
        schema.Extensions[DescriptionsExtension] = Strings(content.Descriptions);
        if (content.HasNumericValuesExtension)
        {
            var numbers = new OpenApiArray();
            numbers.AddRange(content.NumericTexts.Select(Number));
            schema.Extensions[NumericValuesExtension] = numbers;
        }
        else
        {
            schema.Extensions.Remove(NumericValuesExtension);
        }

        schema.Description = content.AppendTable(schema.Description);
    }

    /// <summary>
    /// The schema of a usage of the enum whose component is <paramref name="reference"/>: nullable as <c>allOf</c> +
    /// <c>nullable: true</c>, flags (string format) and collections as arrays of the component, and <c>default</c> and the
    /// description of the usage on an <c>allOf</c> wrapper, never on the shared component.
    /// </summary>
    public static OpenApiSchema CreateUsage(
        EnumUsage usage,
        EnumWireFormat format,
        OpenApiSchema reference,
        IReadOnlyList<string>? defaults,
        string? description)
    {
        OpenApiSchema element = usage.IsFlagsArray(format)
            ? new OpenApiSchema { Type = "array", Items = reference, UniqueItems = true }
            : reference;

        OpenApiSchema result;
        if (usage.IsCollection)
        {
            OpenApiSchema items = usage.ItemsNullable ? MakeNullable(element) : element;
            result = new OpenApiSchema { Type = "array", Items = items };
        }
        else
        {
            result = usage.IsNullable ? MakeNullable(element) : element;
        }

        if (defaults is not null)
        {
            result = Wrap(result);
            result.Default = CreateDefault(usage, format, defaults);
        }

        if (!string.IsNullOrWhiteSpace(description))
        {
            result = Wrap(result);
            result.Description = description;
        }

        return result;
    }

    private static IOpenApiAny CreateDefault(EnumUsage usage, EnumWireFormat format, IReadOnlyList<string> defaults)
    {
        if (usage.IsFlagsArray(format))
            return Strings(defaults);
        return format == EnumWireFormat.Number ? Number(defaults[0]) : new OpenApiString(defaults[0]);
    }

    private static OpenApiSchema MakeNullable(OpenApiSchema schema)
    {
        OpenApiSchema nullable = Wrap(schema);
        nullable.Nullable = true;
        return nullable;
    }

    // A $ref cannot carry siblings in OpenAPI 3.0, so keywords of the usage go on an allOf wrapper.
    private static OpenApiSchema Wrap(OpenApiSchema schema) =>
        schema.Reference is null ? schema : new OpenApiSchema { AllOf = [schema] };

    private static OpenApiArray Strings(IEnumerable<string> values)
    {
        var array = new OpenApiArray();
        array.AddRange(values.Select(static value => (IOpenApiAny)new OpenApiString(value)));
        return array;
    }

    private static IOpenApiAny Number(string text) =>
        EnumUsage.TryParseNumber(text, out long number)
            ? new OpenApiLong(number)
            : new OpenApiUnsignedLong(ulong.Parse(text, NumberStyles.None, CultureInfo.InvariantCulture));

    // A ulong value above long.MaxValue: Microsoft.OpenApi 1.x has no unsigned primitive and OpenApiDouble would round it,
    // so the exact value is written through the writer's decimal overload, which holds every ulong.
    private sealed class OpenApiUnsignedLong(ulong value) : IOpenApiPrimitive
    {
        public AnyType AnyType => AnyType.Primitive;

        public PrimitiveType PrimitiveType => PrimitiveType.Long;

        public void Write(IOpenApiWriter writer, OpenApiSpecVersion specVersion) => writer.WriteValue(value);
    }
}
