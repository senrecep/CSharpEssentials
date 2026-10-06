using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace CSharpEssentials.AspNetCore.OpenApi;

/// <summary>
/// Completes the document once every schema exists: the enum components get the content of the document format (a component
/// shared by documents of different formats was described in the first one), and the properties of object components that use
/// a handled enum get the usage shape (nullable by the document version, flags and collections as arrays, <c>default</c> from
/// <see cref="DefaultValueAttribute"/>, the property description on an <c>allOf</c> wrapper).
/// </summary>
internal sealed class EnumDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        if (document.Components?.Schemas is not { Count: > 0 } components)
            return Task.CompletedTask;

        var enumDocument = OpenApiEnumDocument.Create(context.ApplicationServices, context.DocumentName);
        Dictionary<Type, JsonTypeInfo> types = CollectTypes(context.DescriptionGroups, enumDocument);
        foreach ((Type type, JsonTypeInfo typeInfo) in types)
        {
            if (enumDocument.GetReferenceId(typeInfo) is not { } id || !components.TryGetValue(id, out IOpenApiSchema? component)
                || component is not OpenApiSchema schema)
                continue;

            if (type.IsEnum)
            {
                if (enumDocument.Conventions.Resolve(type) is { } info)
                    OpenApiEnumSchemas.ApplyContent(schema, enumDocument.Conventions.GetContent(info, enumDocument.WireFormat));
            }
            else if (typeInfo.Kind == JsonTypeInfoKind.Object && schema.Properties is { Count: > 0 })
            {
                ApplyProperties(schema, typeInfo, enumDocument, document);
            }
        }

        return Task.CompletedTask;
    }

    private static void ApplyProperties(OpenApiSchema schema, JsonTypeInfo typeInfo, OpenApiEnumDocument enumDocument, OpenApiDocument document)
    {
        foreach (JsonPropertyInfo property in typeInfo.Properties)
        {
            if (schema.Properties is null || !schema.Properties.TryGetValue(property.Name, out IOpenApiSchema? existing)
                || EnumUsage.Classify(property.PropertyType, enumDocument.Conventions) is not { } usage)
                continue;

            object? defaultValue = property.AttributeProvider?.GetCustomAttributes(typeof(DefaultValueAttribute), inherit: true)
                .OfType<DefaultValueAttribute>().FirstOrDefault()?.Value;
            schema.Properties[property.Name] = OpenApiEnumSchemas.CreateUsage(
                usage,
                enumDocument.WireFormat,
                enumDocument.GetReference(usage, document),
                usage.FormatDefault(defaultValue, enumDocument.WireFormat),
                GetPropertyDescription(existing),
                enumDocument.Version);
        }
    }

    // The description of the property itself, never the one the reference reads from the enum component.
    private static string? GetPropertyDescription(IOpenApiSchema existing) => existing switch
    {
        OpenApiSchemaReference reference => reference.Reference.Description,
        OpenApiSchema { AllOf: [OpenApiSchemaReference], Description: var description } => description,
        OpenApiSchema { OneOf.Count: > 0 } wrapper => wrapper.Description,
        _ => null,
    };

    private static Dictionary<Type, JsonTypeInfo> CollectTypes(IReadOnlyList<ApiDescriptionGroup> groups, OpenApiEnumDocument document)
    {
        Dictionary<Type, JsonTypeInfo> types = [];
        foreach (ApiDescription description in groups.SelectMany(static group => group.Items))
        {
            foreach (ApiParameterDescription parameter in description.ParameterDescriptions)
                Collect(parameter.Type, types, document);
            foreach (ApiResponseType response in description.SupportedResponseTypes)
                Collect(response.Type, types, document);
        }

        return types;
    }

    private static void Collect(Type? type, Dictionary<Type, JsonTypeInfo> types, OpenApiEnumDocument document)
    {
        if (type is null || type == typeof(void) || type.IsPointer || type.IsByRef || type.ContainsGenericParameters)
            return;
        type = Nullable.GetUnderlyingType(type) ?? type;
        if (types.ContainsKey(type) || !TryGetTypeInfo(type, document, out JsonTypeInfo? typeInfo))
            return;

        types[type] = typeInfo;
        if (typeInfo.Kind == JsonTypeInfoKind.Object)
        {
            foreach (JsonPropertyInfo property in typeInfo.Properties)
                Collect(property.PropertyType, types, document);
        }
        else if (typeInfo.Kind is JsonTypeInfoKind.Enumerable or JsonTypeInfoKind.Dictionary)
        {
            Collect(typeInfo.ElementType, types, document);
        }
    }

    private static bool TryGetTypeInfo(Type type, OpenApiEnumDocument document, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out JsonTypeInfo? typeInfo)
    {
        // Types the serializer cannot describe (Stream, delegates, ...) have no schema to complete.
        try
        {
            typeInfo = document.JsonOptions.GetTypeInfo(type);
            return true;
        }
        catch (Exception exception) when (exception is NotSupportedException or InvalidOperationException or ArgumentException)
        {
            typeInfo = null;
            return false;
        }
    }
}
