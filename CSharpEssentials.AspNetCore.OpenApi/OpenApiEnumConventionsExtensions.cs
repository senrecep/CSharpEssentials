using CSharpEssentials.AspNetCore.OpenApi;
using Microsoft.AspNetCore.OpenApi;

namespace CSharpEssentials.AspNetCore;

/// <summary>Describes the enum conventions in the documents of Microsoft.AspNetCore.OpenApi.</summary>
public static class OpenApiEnumConventionsExtensions
{
    /// <summary>
    /// Describes the enums handled by the enum conventions (design section 10.1): one component per enum with the wire names
    /// (or the numbers in a document whose operations all write numbers), <c>x-enum-varnames</c>, <c>x-enum-descriptions</c>,
    /// <c>x-enum-numeric-values</c> and a value table; flags and collections as arrays; nullable per OpenAPI version;
    /// <c>default</c> as wire names; operations whose output format differs are marked. Plain enums keep the framework schema.
    /// </summary>
    /// <example><code>services.AddOpenApi(options => options.AddEnumConventions());</code></example>
    /// <param name="options">The options of one OpenAPI document.</param>
    /// <returns><paramref name="options"/>.</returns>
    public static OpenApiOptions AddEnumConventions(this OpenApiOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.AddSchemaTransformer<EnumSchemaTransformer>();
        options.AddOperationTransformer<EnumOperationTransformer>();
        options.AddDocumentTransformer<EnumDocumentTransformer>();
        return options;
    }
}
