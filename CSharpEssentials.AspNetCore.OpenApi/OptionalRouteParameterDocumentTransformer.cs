using System.Text;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

namespace CSharpEssentials.AspNetCore.OpenApi;

/// <summary>
/// Describes optional route parameters with valid OpenAPI (see <see cref="OpenApiOptionalRouteParameterMode"/>) through
/// <see cref="OptionalRouteParameterSplitter"/>, once every path of the document exists.
/// </summary>
internal sealed class OptionalRouteParameterDocumentTransformer : IOpenApiDocumentTransformer
{
    private const char PathSeparator = '/';
    private static readonly string LoggerCategory = typeof(OptionalRouteParameterDocumentTransformer).FullName!;

    public OpenApiOptionalRouteParameterMode Mode { get; set; }

    public Func<string, IReadOnlyList<string>, string>? OperationIdSelector { get; set; }

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        // The description groups of the context are not filtered by document.
        Func<ApiDescription, bool> shouldInclude = context.ApplicationServices
            .GetRequiredService<IOptionsMonitor<OpenApiOptions>>().Get(context.DocumentName).ShouldInclude;
        IEnumerable<ApiDescription> descriptions = context.DescriptionGroups
            .SelectMany(static group => group.Items)
            .Where(shouldInclude);

        var splitter = new OptionalRouteParameterSplitter(
            Mode == OpenApiOptionalRouteParameterMode.SplitPaths,
            OperationIdSelector,
            context.ApplicationServices.GetService<ILoggerFactory>(),
            LoggerCategory);
        splitter.Apply(document, context.DocumentName, descriptions, GetPathKey);
        return Task.CompletedTask;
    }

    // The path key Microsoft.AspNetCore.OpenApi gives a description: constraints, defaults and optional markers removed.
    private static string GetPathKey(ApiDescription description)
    {
        if (string.IsNullOrEmpty(description.RelativePath))
            return PathSeparator.ToString();

        var path = new StringBuilder();
        foreach (RoutePatternPathSegment segment in RoutePatternFactory.Parse(description.RelativePath).PathSegments)
        {
            path.Append(PathSeparator);
            foreach (RoutePatternPart part in segment.Parts)
            {
                if (part is RoutePatternLiteralPart literal)
                    path.Append(literal.Content);
                else if (part is RoutePatternParameterPart parameter)
                    path.Append('{').Append(parameter.Name).Append('}');
                else if (part is RoutePatternSeparatorPart separator)
                    path.Append(separator.Content);
            }
        }
        return path.ToString();
    }
}
