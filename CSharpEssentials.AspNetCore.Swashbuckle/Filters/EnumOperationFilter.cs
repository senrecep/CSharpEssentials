using CSharpEssentials.Enums;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CSharpEssentials.AspNetCore.Swagger.Filters;

/// <summary>
/// Writes the usage shape of enum parameters, request bodies and responses (nullable, flags, collections, <c>default</c>;
/// query arrays get <c>style: form</c>, <c>explode: true</c>) and marks the operations whose enum output format differs from
/// the document (<c>x-enum-wire-format</c>) or depends on a request header (<c>x-enum-wire-format-header</c>).
/// </summary>
internal sealed class EnumOperationFilter(IServiceProvider services) : IOperationFilter
{
    private readonly OpenApiEnumConventions _conventions = OpenApiEnumConventions.For(services);

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.DocumentName is not { } documentName)
            return;

        SwaggerGeneratorOptions options = services.GetRequiredService<IOptions<SwaggerGeneratorOptions>>().Value;
        DocumentEnumFormat document = _conventions.GetDocumentFormat(documentName, description => options.DocInclusionPredicate(documentName, description));

        ApplyParameters(operation, context, document.Format);
        ApplyBodies(operation, context, document.Format);

        if (document.GetMarkers(context.ApiDescription.ActionDescriptor) is not { } markers)
            return;
        operation.Extensions ??= new Dictionary<string, IOpenApiExtension>(StringComparer.Ordinal);
        if (markers.WireFormat is { } wireFormat)
            operation.Extensions[OpenApiEnumConventions.WireFormatExtension] = new JsonNodeExtension(JsonValue.Create(wireFormat));
        if (markers.Header is { } header)
            operation.Extensions[OpenApiEnumConventions.WireFormatHeaderExtension] = new JsonNodeExtension(JsonValue.Create(header));
        operation.Description = markers.AppendNote(operation.Description);
    }

    private void ApplyParameters(OpenApiOperation operation, OperationFilterContext context, EnumWireFormat format)
    {
        foreach (IOpenApiParameter openApiParameter in operation.Parameters ?? [])
        {
            if (openApiParameter is not OpenApiParameter parameter)
                continue;

            ApiParameterDescription? description = context.ApiDescription.ParameterDescriptions.FirstOrDefault(candidate =>
                string.Equals(candidate.Name, parameter.Name, StringComparison.OrdinalIgnoreCase) && candidate.Source != BindingSource.Body);
            // MVC reports string as the Type of a parameter whose type converts from string (enums do); its model type is the enum.
            if (description is null || parameter.Schema is null || (EnumUsage.Classify(description.Type, _conventions) ?? EnumUsage.Classify(description.ModelMetadata?.ModelType, _conventions)) is not { } usage)
                continue;

            IOpenApiSchema reference = context.SchemaGenerator.GenerateSchema(usage.EnumType, context.SchemaRepository);
            parameter.Schema = OpenApiEnumSchemas.CreateUsage(
                usage, format, reference, usage.FormatDefault(description.DefaultValue, format), description: null, OpenApiSpecVersion.OpenApi3_0);
            if (parameter.In == ParameterLocation.Query && usage.IsArray(format))
            {
                parameter.Style = ParameterStyle.Form;
                parameter.Explode = true;
            }
        }
    }

    private void ApplyBodies(OpenApiOperation operation, OperationFilterContext context, EnumWireFormat format)
    {
        ApiParameterDescription? body = context.ApiDescription.ParameterDescriptions.FirstOrDefault(static candidate => candidate.Source == BindingSource.Body);
        if (body is not null && operation.RequestBody?.Content is { } requestContent)
            Replace(requestContent, body.Type, context, format);

        if (operation.Responses is null)
            return;
        foreach (ApiResponseType response in context.ApiDescription.SupportedResponseTypes)
        {
            string statusCode = response.IsDefaultResponse ? "default" : response.StatusCode.ToString(CultureInfo.InvariantCulture);
            if (operation.Responses.TryGetValue(statusCode, out IOpenApiResponse? openApiResponse) && openApiResponse.Content is { } content)
                Replace(content, response.Type, context, format);
        }
    }

    private void Replace(IDictionary<string, OpenApiMediaType> content, Type? type, OperationFilterContext context, EnumWireFormat format)
    {
        if (EnumUsage.Classify(type, _conventions) is not { } usage)
            return;

        foreach (OpenApiMediaType mediaType in content.Values)
        {
            if (mediaType.Schema is null)
                continue;
            IOpenApiSchema reference = context.SchemaGenerator.GenerateSchema(usage.EnumType, context.SchemaRepository);
            mediaType.Schema = OpenApiEnumSchemas.CreateUsage(usage, format, reference, defaults: null, description: null, OpenApiSpecVersion.OpenApi3_0);
        }
    }
}
