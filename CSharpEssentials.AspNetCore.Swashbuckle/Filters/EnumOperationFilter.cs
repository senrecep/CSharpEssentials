using CSharpEssentials.Enums;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
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
        if (markers.WireFormat is { } wireFormat)
            operation.Extensions[OpenApiEnumConventions.WireFormatExtension] = new OpenApiString(wireFormat);
        if (markers.Header is { } header)
            operation.Extensions[OpenApiEnumConventions.WireFormatHeaderExtension] = new OpenApiString(header);
        operation.Description = markers.AppendNote(operation.Description);
    }

    private void ApplyParameters(OpenApiOperation operation, OperationFilterContext context, EnumWireFormat format)
    {
        foreach (OpenApiParameter parameter in operation.Parameters)
        {
            ApiParameterDescription? description = context.ApiDescription.ParameterDescriptions.FirstOrDefault(candidate =>
                string.Equals(candidate.Name, parameter.Name, StringComparison.OrdinalIgnoreCase) && candidate.Source != BindingSource.Body);
            if (description is null || parameter.Schema is null || EnumUsage.Classify(description.Type, _conventions) is not { } usage)
                continue;

            OpenApiSchema reference = context.SchemaGenerator.GenerateSchema(usage.EnumType, context.SchemaRepository);
            parameter.Schema = SwashbuckleEnumSchemas.CreateUsage(
                usage, format, reference, usage.FormatDefault(description.DefaultValue, format), description: null);
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
        if (body is not null && operation.RequestBody is { } requestBody)
            Replace(requestBody.Content, body.Type, context, format);

        foreach (ApiResponseType response in context.ApiDescription.SupportedResponseTypes)
        {
            string statusCode = response.IsDefaultResponse ? "default" : response.StatusCode.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (operation.Responses.TryGetValue(statusCode, out OpenApiResponse? openApiResponse))
                Replace(openApiResponse.Content, response.Type, context, format);
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
            OpenApiSchema reference = context.SchemaGenerator.GenerateSchema(usage.EnumType, context.SchemaRepository);
            mediaType.Schema = SwashbuckleEnumSchemas.CreateUsage(usage, format, reference, defaults: null, description: null);
        }
    }
}
