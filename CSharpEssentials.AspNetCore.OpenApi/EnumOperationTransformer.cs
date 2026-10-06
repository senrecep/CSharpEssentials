using System.Globalization;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace CSharpEssentials.AspNetCore.OpenApi;

/// <summary>
/// Writes the usage shape of enum parameters, request bodies and responses (nullable, flags, collections, <c>default</c>;
/// query arrays get <c>style: form</c>, <c>explode: true</c>) and marks the operations whose enum output format differs from
/// the document (<c>x-enum-wire-format</c>) or depends on a request header (<c>x-enum-wire-format-header</c>).
/// </summary>
internal sealed class EnumOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var document = OpenApiEnumDocument.Create(context.ApplicationServices, context.DocumentName);
        if (context.Document is { } openApiDocument)
        {
            ApplyParameters(operation, context.Description, document, openApiDocument);
            ApplyBodies(operation, context.Description, document, openApiDocument);
        }

        if (document.Format.GetMarkers(context.Description.ActionDescriptor) is not { } markers)
            return Task.CompletedTask;

        operation.Extensions ??= new Dictionary<string, IOpenApiExtension>(StringComparer.Ordinal);
        if (markers.WireFormat is { } wireFormat)
            operation.Extensions[OpenApiEnumConventions.WireFormatExtension] = new JsonNodeExtension(JsonValue.Create(wireFormat));
        if (markers.Header is { } header)
            operation.Extensions[OpenApiEnumConventions.WireFormatHeaderExtension] = new JsonNodeExtension(JsonValue.Create(header));
        operation.Description = markers.AppendNote(operation.Description);
        return Task.CompletedTask;
    }

    private static void ApplyParameters(OpenApiOperation operation, ApiDescription description, OpenApiEnumDocument document, OpenApiDocument openApiDocument)
    {
        foreach (IOpenApiParameter candidate in operation.Parameters ?? [])
        {
            if (candidate is not OpenApiParameter parameter || parameter.Schema is null)
                continue;

            ApiParameterDescription? parameterDescription = description.ParameterDescriptions.FirstOrDefault(item =>
                string.Equals(item.Name, parameter.Name, StringComparison.OrdinalIgnoreCase) && item.Source != BindingSource.Body);
            // MVC reports string as the Type of a parameter whose type converts from string (enums do); its model type is the enum.
            if (parameterDescription is null || (EnumUsage.Classify(parameterDescription.Type, document.Conventions) ?? EnumUsage.Classify(parameterDescription.ModelMetadata?.ModelType, document.Conventions)) is not { } usage)
                continue;

            parameter.Schema = OpenApiEnumSchemas.CreateUsage(
                usage,
                document.WireFormat,
                document.GetReference(usage, openApiDocument),
                usage.FormatDefault(parameterDescription.DefaultValue, document.WireFormat),
                description: null,
                document.Version);
            if (parameter.In == ParameterLocation.Query && usage.IsArray(document.WireFormat))
            {
                parameter.Style = ParameterStyle.Form;
                parameter.Explode = true;
            }
        }
    }

    private static void ApplyBodies(OpenApiOperation operation, ApiDescription description, OpenApiEnumDocument document, OpenApiDocument openApiDocument)
    {
        ApiParameterDescription? body = description.ParameterDescriptions.FirstOrDefault(static candidate => candidate.Source == BindingSource.Body);
        if (body is not null && operation.RequestBody?.Content is { } requestContent)
            Replace(requestContent, body.Type, document, openApiDocument);

        if (operation.Responses is null)
            return;
        foreach (ApiResponseType response in description.SupportedResponseTypes)
        {
            string statusCode = response.IsDefaultResponse ? "default" : response.StatusCode.ToString(CultureInfo.InvariantCulture);
            if (operation.Responses.TryGetValue(statusCode, out IOpenApiResponse? openApiResponse) && openApiResponse.Content is { } content)
                Replace(content, response.Type, document, openApiDocument);
        }
    }

    private static void Replace(IDictionary<string, OpenApiMediaType> content, Type? type, OpenApiEnumDocument document, OpenApiDocument openApiDocument)
    {
        if (EnumUsage.Classify(type, document.Conventions) is not { } usage)
            return;

        foreach (OpenApiMediaType mediaType in content.Values)
        {
            if (mediaType.Schema is null)
                continue;
            mediaType.Schema = OpenApiEnumSchemas.CreateUsage(
                usage, document.WireFormat, document.GetReference(usage, openApiDocument), defaults: null, description: null, document.Version);
        }
    }
}
