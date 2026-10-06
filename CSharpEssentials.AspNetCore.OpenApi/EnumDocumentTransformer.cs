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
        Dictionary<Type, JsonTypeInfo> types = [];
        Dictionary<Type, List<JsonTypeInfo>> bases = [];
        CollectTypes(context.DescriptionGroups, enumDocument, types, bases);
        foreach ((Type type, JsonTypeInfo typeInfo) in types)
        {
            foreach (string id in GetComponentIds(type, typeInfo, bases, enumDocument))
            {
                if (!components.TryGetValue(id, out IOpenApiSchema? component) || component is not OpenApiSchema schema)
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

    /// <summary>
    /// The component ids of <paramref name="type"/>: its own, and for a derived type of a polymorphic base the id the framework
    /// gives it under that base (the base id followed by its own).
    /// </summary>
    private static IEnumerable<string> GetComponentIds(Type type, JsonTypeInfo typeInfo, Dictionary<Type, List<JsonTypeInfo>> bases, OpenApiEnumDocument document)
    {
        if (document.GetReferenceId(typeInfo) is not { } id)
            yield break;
        yield return id;
        foreach (JsonTypeInfo baseType in bases.GetValueOrDefault(type) ?? [])
        {
            if (document.GetReferenceId(baseType) is { } baseId)
                yield return baseId + id;
        }
    }

    private static void CollectTypes(
        IReadOnlyList<ApiDescriptionGroup> groups, OpenApiEnumDocument document, Dictionary<Type, JsonTypeInfo> types, Dictionary<Type, List<JsonTypeInfo>> bases)
    {
        foreach (ApiDescription description in groups.SelectMany(static group => group.Items))
        {
            foreach (ApiParameterDescription parameter in description.ParameterDescriptions)
                Collect(parameter.Type, types, bases, document);
            foreach (ApiResponseType response in description.SupportedResponseTypes)
                Collect(response.Type, types, bases, document);
        }
    }

    private static void Collect(Type? type, Dictionary<Type, JsonTypeInfo> types, Dictionary<Type, List<JsonTypeInfo>> bases, OpenApiEnumDocument document)
    {
        if (type is null || type == typeof(void) || type.IsPointer || type.IsByRef || type.ContainsGenericParameters)
            return;
        type = Nullable.GetUnderlyingType(type) ?? type;
        if (types.ContainsKey(type) || !TryGetTypeInfo(type, document, out JsonTypeInfo? typeInfo))
            return;

        // Recorded before the walk, so a type reached again (a cycle, a derived type that refers to its base) is visited once.
        types[type] = typeInfo;
        if (typeInfo.Kind == JsonTypeInfoKind.Object)
        {
            foreach (JsonPropertyInfo property in typeInfo.Properties)
                Collect(property.PropertyType, types, bases, document);
            // The derived types of a polymorphic type get their own components; their properties are not on the base type.
            foreach (JsonDerivedType derived in typeInfo.PolymorphismOptions?.DerivedTypes ?? [])
            {
                if (derived.DerivedType == type)
                    continue;
                if (!bases.TryGetValue(derived.DerivedType, out List<JsonTypeInfo>? derivedBases))
                    bases[derived.DerivedType] = derivedBases = [];
                derivedBases.Add(typeInfo);
                Collect(derived.DerivedType, types, bases, document);
            }
        }
        else if (typeInfo.Kind is JsonTypeInfoKind.Enumerable or JsonTypeInfoKind.Dictionary)
        {
            Collect(typeInfo.ElementType, types, bases, document);
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
