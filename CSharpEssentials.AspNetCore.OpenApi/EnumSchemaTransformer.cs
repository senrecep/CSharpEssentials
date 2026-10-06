using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace CSharpEssentials.AspNetCore.OpenApi;

/// <summary>
/// Gives the schema of every enum the conventions handle the component content of design section 10.1: <c>enum</c> with the
/// wire names (or the numbers in a document whose operations all write numbers), <c>x-enum-varnames</c>,
/// <c>x-enum-descriptions</c>, <c>x-enum-numeric-values</c> and a value table appended to the description. The usage shape
/// (nullable, arrays, <c>default</c>) is written by <see cref="EnumOperationTransformer"/> and <see cref="EnumDocumentTransformer"/>.
/// Plain enums keep the framework schema.
/// </summary>
internal sealed class EnumSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        Type type = context.JsonTypeInfo.Type;
        Type? enumType = Nullable.GetUnderlyingType(type) ?? type;
        if (!enumType.IsEnum)
            return Task.CompletedTask;

        var conventions = OpenApiEnumConventions.For(context.ApplicationServices);
        if (conventions.Resolve(enumType) is not { } info)
            return Task.CompletedTask;

        var document = OpenApiEnumDocument.Create(context.ApplicationServices, context.DocumentName);
        EnumSchemaContent content = conventions.GetContent(info, document.WireFormat);
        if (enumType == type)
        {
            OpenApiEnumSchemas.ApplyContent(schema, content);
            return Task.CompletedTask;
        }

        // Nullable<TEnum>: the framework writes oneOf [{type: null}, <enum>]; the enum member becomes the shared component.
        foreach (IOpenApiSchema member in schema.OneOf ?? [])
        {
            if (member is OpenApiSchema inline && inline.Type != JsonSchemaType.Null)
                OpenApiEnumSchemas.ApplyContent(inline, content);
        }

        return Task.CompletedTask;
    }
}
