using System.Runtime.CompilerServices;
using CSharpEssentials.AspNetCore.OpenApi;
using Microsoft.AspNetCore.OpenApi;

namespace CSharpEssentials.AspNetCore;

/// <summary>Optional route parameter conventions for the documents of Microsoft.AspNetCore.OpenApi.</summary>
public static class OpenApiOptionalRouteParameterExtensions
{
    private static readonly ConditionalWeakTable<OpenApiOptions, OptionalRouteParameterDocumentTransformer> _transformers = [];

    /// <summary>
    /// Describes optional route parameters with valid OpenAPI (see <see cref="OpenApiOptionalRouteParameterMode"/>). A later call
    /// on the same options replaces the mode and selector of an earlier one.
    /// </summary>
    /// <example><code>services.AddOpenApi(options => options.AddOptionalRouteParameters());</code></example>
    /// <param name="options">The options of one OpenAPI document.</param>
    /// <param name="mode">The mode.</param>
    /// <param name="operationIdSelector">
    /// <see cref="OpenApiOptionalRouteParameterMode.SplitPaths"/> only: the operationId of a form without some optional segments,
    /// from the operationId of the full form and the names of the omitted parameters. The default is
    /// <c>{operationId}Without{Param}</c>, with <c>And</c> between several parameters. A form whose full operation has no
    /// operationId gets none. A generated operationId that another operation already uses throws an
    /// <see cref="InvalidOperationException"/> when the document is generated.
    /// </param>
    /// <returns><paramref name="options"/>.</returns>
    public static OpenApiOptions AddOptionalRouteParameters(
        this OpenApiOptions options,
        OpenApiOptionalRouteParameterMode mode = OpenApiOptionalRouteParameterMode.SplitPaths,
        Func<string, IReadOnlyList<string>, string>? operationIdSelector = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (!Enum.IsDefined(mode))
            throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown optional route parameter mode.");

        // The registered transformers of OpenApiOptions cannot be removed, so a later call updates the registered one.
        OptionalRouteParameterDocumentTransformer transformer = _transformers.GetValue(options, static registered =>
        {
            var created = new OptionalRouteParameterDocumentTransformer();
            registered.AddDocumentTransformer(created);
            return created;
        });
        transformer.Mode = mode;
        transformer.OperationIdSelector = operationIdSelector;
        return options;
    }
}
