using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CSharpEssentials.AspNetCore.Swagger.Filters;

/// <summary>
/// Describes optional route parameters with valid OpenAPI (see <see cref="OptionalRouteParameterMode.SplitPaths"/> and
/// <see cref="OptionalRouteParameterMode.RequiredOnly"/>) through <see cref="OptionalRouteParameterSplitter"/>, on the paths
/// that <see cref="SwaggerGeneratorOptions.PathGroupSelector"/> returns.
/// </summary>
internal sealed class OptionalRouteParameterDocumentFilter(IServiceProvider services, OptionalRouteParameterSettings settings) : IDocumentFilter
{
    private const char PathSeparator = '/';
    private static readonly string LoggerCategory = typeof(OptionalRouteParameterDocumentFilter).FullName!;

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        Func<ApiDescription, string> pathGroupSelector = services.GetRequiredService<IOptions<SwaggerGeneratorOptions>>().Value.PathGroupSelector;
        var splitter = new OptionalRouteParameterSplitter(
            settings.Mode == OptionalRouteParameterMode.SplitPaths,
            settings.OperationIdSelector,
            services.GetService<ILoggerFactory>(),
            LoggerCategory);
        splitter.Apply(swaggerDoc, context.DocumentName, context.ApiDescriptions, description => PathSeparator + pathGroupSelector(description));
    }
}
