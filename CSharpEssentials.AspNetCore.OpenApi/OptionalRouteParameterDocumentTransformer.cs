using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;

namespace CSharpEssentials.AspNetCore.OpenApi;

/// <summary>
/// Describes optional route parameters with valid OpenAPI (see <see cref="OpenApiOptionalRouteParameterMode"/>) through
/// <see cref="OptionalRouteParameterSplitter"/>, once every path of the document exists. As an operation transformer it records
/// the <see cref="ApiDescription"/> of each generated operation, so the document transformer reads every path key from
/// <see cref="OpenApiDocument.Paths"/> instead of recomputing it.
/// </summary>
internal sealed class OptionalRouteParameterDocumentTransformer : IOpenApiDocumentTransformer, IOpenApiOperationTransformer
{
    private static readonly string LoggerCategory = typeof(OptionalRouteParameterDocumentTransformer).FullName!;

    // Operation transformers run before document transformers; every generation creates new operations.
    private readonly ConditionalWeakTable<OpenApiOperation, ApiDescription> _descriptions = [];

    public OpenApiOptionalRouteParameterMode Mode { get; set; }

    public Func<string, IReadOnlyList<string>, string>? OperationIdSelector { get; set; }

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        _descriptions.AddOrUpdate(operation, context.Description);
        return Task.CompletedTask;
    }

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        if (document.Paths is not { Count: > 0 } paths)
            return Task.CompletedTask;

        Dictionary<ApiDescription, string> pathKeys = [with(ReferenceEqualityComparer.Instance)];
        foreach (KeyValuePair<string, IOpenApiPathItem> item in paths)
        {
            foreach (OpenApiOperation operation in item.Value.Operations?.Values ?? Enumerable.Empty<OpenApiOperation>())
            {
                if (_descriptions.TryGetValue(operation, out ApiDescription? description))
                    pathKeys.TryAdd(description, item.Key);
            }
        }

        var splitter = new OptionalRouteParameterSplitter(
            Mode == OpenApiOptionalRouteParameterMode.SplitPaths,
            OperationIdSelector,
            context.ApplicationServices.GetService<ILoggerFactory>(),
            LoggerCategory);
        splitter.Apply(document, context.DocumentName, [.. pathKeys.Keys], description => pathKeys[description]);
        return Task.CompletedTask;
    }
}
