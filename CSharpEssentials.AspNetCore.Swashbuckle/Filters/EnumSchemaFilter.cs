using System.ComponentModel;
using System.Reflection;
using CSharpEssentials.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CSharpEssentials.AspNetCore.Swagger.Filters;

/// <summary>
/// Describes the enums handled by the enum conventions (design section 10.1): one component per enum whose <c>enum</c> lists
/// the wire names (or the numbers in a document whose operations all write numbers), with <c>x-enum-varnames</c>,
/// <c>x-enum-descriptions</c>, <c>x-enum-numeric-values</c> and a value table appended to the description. Properties that use
/// such an enum get the nullable, flags, collection and default shape. Plain enums keep the Swashbuckle schema.
/// Added by <see cref="SwaggerEnumConventionsExtensions.AddEnumConventions"/>.
/// </summary>
/// <param name="services">The application services.</param>
public class EnumSchemaFilter(IServiceProvider services) : ISchemaFilter
{
    private readonly OpenApiEnumConventions _conventions = OpenApiEnumConventions.For(services);

    /// <inheritdoc />
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(schema);
        ArgumentNullException.ThrowIfNull(context);

        // A reference has nothing to change; its component gets the filter on its own.
        if (schema is not OpenApiSchema concrete)
            return;

        if (context.Type.IsEnum)
        {
            ApplyEnum(concrete, context);
            return;
        }

        if (concrete.Properties is { Count: > 0 })
            ApplyProperties(concrete, context);
    }

    private void ApplyEnum(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (_conventions.Resolve(context.Type) is not { } info)
            return;

        EnumWireFormat format = GetFormat(services, _conventions, context.DocumentName);
        OpenApiEnumSchemas.ApplyContent(schema, _conventions.GetContent(info, format));
    }

    private void ApplyProperties(OpenApiSchema schema, SchemaFilterContext context)
    {
        DataContract contract = services.GetRequiredService<ISerializerDataContractResolver>().GetDataContractForType(context.Type);
        if (contract.ObjectProperties is null || schema.Properties is null)
            return;

        EnumWireFormat? format = null;
        foreach (DataProperty property in contract.ObjectProperties)
        {
            if (!schema.Properties.TryGetValue(property.Name, out IOpenApiSchema? existing)
                || EnumUsage.Classify(property.MemberType, _conventions) is not { } usage)
                continue;

            format ??= GetFormat(services, _conventions, context.DocumentName);
            object? defaultValue = property.MemberInfo?.GetCustomAttribute<DefaultValueAttribute>()?.Value;
            IOpenApiSchema reference = context.SchemaGenerator.GenerateSchema(usage.EnumType, context.SchemaRepository);
            schema.Properties[property.Name] = OpenApiEnumSchemas.CreateUsage(
                usage,
                format.Value,
                reference,
                usage.FormatDefault(defaultValue, format.Value),
                GetPropertyDescription(existing, context.SchemaRepository),
                OpenApiSpecVersion.OpenApi3_0);
        }
    }

    // The description of the property itself, never the one of the enum component: Swashbuckle copies the component
    // description onto the reference it returns when it first adds the component.
    private static string? GetPropertyDescription(IOpenApiSchema existing, SchemaRepository repository)
    {
        if (existing is not OpenApiSchemaReference reference)
            return existing.Description;

        string? description = reference.Reference.Description;
        bool copiedFromComponent = reference.Reference.Id is { } id
            && repository.Schemas.TryGetValue(id, out IOpenApiSchema? component)
            && string.Equals(component.Description, description, StringComparison.Ordinal);
        return copiedFromComponent ? null : description;
    }

    internal static EnumWireFormat GetFormat(IServiceProvider services, OpenApiEnumConventions conventions, string? documentName)
    {
        if (documentName is null)
            return conventions.WriteAs;

        SwaggerGeneratorOptions options = services.GetRequiredService<IOptions<SwaggerGeneratorOptions>>().Value;
        return conventions.GetDocumentFormat(documentName, description => options.DocInclusionPredicate(documentName, description)).Format;
    }
}
