using Microsoft.OpenApi;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Copies an operation one level deeper than the Microsoft.OpenApi copy constructors, which copy the collections but keep
/// their items. Parameters (with their inline schema, examples and media types), the request body, responses (with their
/// media types), security requirements, servers, tags and extensions are copied. Callbacks, links and headers are copied
/// shallowly, so their contents stay shared with the source, as do media type schemas and component references.
/// </summary>
internal static class OpenApiOperationCloner
{
    public static OpenApiOperation Clone(OpenApiOperation source) =>
        new(source)
        {
            Parameters = source.Parameters is null ? null : [.. source.Parameters.Select(CloneParameter)],
            RequestBody = source.RequestBody is null ? null : CloneRequestBody(source.RequestBody),
            Responses = source.Responses is null ? null : CloneResponses(source.Responses),
            Callbacks = source.Callbacks?.ToDictionary(static pair => pair.Key, static pair => pair.Value.CreateShallowCopy(), StringComparer.Ordinal),
            Security = source.Security is null ? null : [.. source.Security.Select(CloneSecurityRequirement)],
            Servers = source.Servers is null ? null : [.. source.Servers.Select(static server => new OpenApiServer(server))],
            Extensions = CloneExtensions(source.Extensions),
        };

    private static IOpenApiParameter CloneParameter(IOpenApiParameter source)
    {
        IOpenApiParameter copy = source.CreateShallowCopy();
        if (copy is OpenApiParameter parameter)
        {
            if (parameter.Schema is OpenApiSchema schema)
                parameter.Schema = schema.CreateShallowCopy();
            parameter.Content = CloneContent(parameter.Content);
            parameter.Examples = CloneExamples(parameter.Examples);
            parameter.Extensions = CloneExtensions(parameter.Extensions);
        }
        return copy;
    }

    private static IOpenApiRequestBody CloneRequestBody(IOpenApiRequestBody source)
    {
        IOpenApiRequestBody copy = source.CreateShallowCopy();
        if (copy is OpenApiRequestBody body)
        {
            body.Content = CloneContent(body.Content);
            body.Extensions = CloneExtensions(body.Extensions);
        }
        return copy;
    }

    private static OpenApiResponses CloneResponses(OpenApiResponses source)
    {
        var copy = new OpenApiResponses { Extensions = CloneExtensions(source.Extensions) };
        foreach (KeyValuePair<string, IOpenApiResponse> pair in source)
        {
            IOpenApiResponse response = pair.Value.CreateShallowCopy();
            if (response is OpenApiResponse concrete)
            {
                concrete.Content = CloneContent(concrete.Content);
                concrete.Headers = concrete.Headers?.ToDictionary(static header => header.Key, static header => header.Value.CreateShallowCopy(), StringComparer.Ordinal);
                concrete.Links = concrete.Links?.ToDictionary(static link => link.Key, static link => link.Value.CreateShallowCopy(), StringComparer.Ordinal);
                concrete.Extensions = CloneExtensions(concrete.Extensions);
            }
            copy.Add(pair.Key, response);
        }
        return copy;
    }

    private static OpenApiSecurityRequirement CloneSecurityRequirement(OpenApiSecurityRequirement source)
    {
        var copy = new OpenApiSecurityRequirement();
        foreach (KeyValuePair<OpenApiSecuritySchemeReference, List<string>> pair in source)
            copy.Add(pair.Key, [.. pair.Value]);
        return copy;
    }

    private static Dictionary<string, OpenApiMediaType>? CloneContent(IDictionary<string, OpenApiMediaType>? source) =>
        source?.ToDictionary(static pair => pair.Key, static pair => new OpenApiMediaType(pair.Value)
        {
            Examples = CloneExamples(pair.Value.Examples),
            Extensions = CloneExtensions(pair.Value.Extensions),
        }, StringComparer.Ordinal);

    private static Dictionary<string, IOpenApiExample>? CloneExamples(IDictionary<string, IOpenApiExample>? source) =>
        source?.ToDictionary(static pair => pair.Key, static pair => pair.Value.CreateShallowCopy(), StringComparer.Ordinal);

    private static Dictionary<string, IOpenApiExtension>? CloneExtensions(IDictionary<string, IOpenApiExtension>? source) =>
        source?.ToDictionary(
            static pair => pair.Key,
            static pair => pair.Value is JsonNodeExtension { Node: { } node } ? new JsonNodeExtension(node.DeepClone()) : pair.Value,
            StringComparer.Ordinal);
}
