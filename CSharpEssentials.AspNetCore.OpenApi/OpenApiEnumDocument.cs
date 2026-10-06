using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using CSharpEssentials.Enums;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using HttpJsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

namespace CSharpEssentials.AspNetCore.OpenApi;

/// <summary>
/// The enum view of one OpenAPI document as the transformers see it: the enum format of the document and of its operations,
/// the document version, and the component references of the handled enums.
/// </summary>
internal sealed class OpenApiEnumDocument
{
    private readonly OpenApiOptions _options;

    private OpenApiEnumDocument(IServiceProvider services, string documentName)
    {
        Conventions = OpenApiEnumConventions.For(services);
        _options = services.GetRequiredService<IOptionsMonitor<OpenApiOptions>>().Get(documentName);
        JsonOptions = services.GetRequiredService<IOptions<HttpJsonOptions>>().Value.SerializerOptions;
        Format = Conventions.GetDocumentFormat(documentName, _options.ShouldInclude);
    }

    public OpenApiEnumConventions Conventions { get; }

    public DocumentEnumFormat Format { get; }

    public EnumWireFormat WireFormat => Format.Format;

    public OpenApiSpecVersion Version => _options.OpenApiVersion;

    public JsonSerializerOptions JsonOptions { get; }

    public static OpenApiEnumDocument Create(IServiceProvider services, string documentName) => new(services, documentName);

    /// <summary>The component id of <paramref name="type"/>; <see langword="null"/> when the framework inlines its schema.</summary>
    public string? GetReferenceId(Type type) => GetReferenceId(JsonOptions.GetTypeInfo(type));

    public string? GetReferenceId(JsonTypeInfo typeInfo) => _options.CreateSchemaReferenceId(typeInfo);

    /// <summary>
    /// The schema that points at the component of <paramref name="usage"/>'s enum, adding the component when the document does
    /// not have it yet; an inline copy of the content when the framework does not give the enum a component.
    /// </summary>
    public IOpenApiSchema GetReference(EnumUsage usage, OpenApiDocument document)
    {
        EnumSchemaContent content = Conventions.GetContent(usage.Info, WireFormat);
        if (GetReferenceId(usage.EnumType) is not { } id)
        {
            var inline = new OpenApiSchema();
            OpenApiEnumSchemas.ApplyContent(inline, content);
            return inline;
        }

        document.Components ??= new OpenApiComponents();
        document.Components.Schemas ??= new Dictionary<string, IOpenApiSchema>(StringComparer.Ordinal);
        if (!document.Components.Schemas.TryGetValue(id, out IOpenApiSchema? component) || component is not OpenApiSchema)
        {
            var created = new OpenApiSchema();
            OpenApiEnumSchemas.ApplyContent(created, content);
            document.Components.Schemas[id] = created;
        }

        return new OpenApiSchemaReference(id, document);
    }
}
