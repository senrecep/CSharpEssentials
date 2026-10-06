using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Endpoints;

/// <summary>
/// Reflection-based endpoint mapping for scenarios where the generated registries cannot be used.
/// Prefer the generated <c>Map{Asm}Endpoints</c> and <c>MapAllEndpoints</c> methods, which are trim and AOT safe.
/// </summary>
public static class EndpointRouteBuilderExtensions
{
    private const string UnreferencedCodeMessage = "Scans assemblies for IEndpoint types; use the generated Map{Asm}Endpoints for trimmed/AOT apps.";

    private const string DynamicCodeMessage = "Closes EndpointMapper generic methods at runtime.";

    /// <summary>
    /// Maps every endpoint type found in <paramref name="assemblies"/> by reflection, using the same discovery rules,
    /// ordering and conventions as the generated registries.
    /// </summary>
    /// <param name="app">The route builder.</param>
    /// <param name="assemblies">The assemblies to scan, mapped in the given order.</param>
    /// <returns>The same route builder.</returns>
    [RequiresUnreferencedCode(UnreferencedCodeMessage)]
    [RequiresDynamicCode(DynamicCodeMessage)]
    public static IEndpointRouteBuilder MapEndpointsFromAssemblies(this IEndpointRouteBuilder app, params Assembly[] assemblies) =>
        app.MapEndpointsFromAssemblies(null, assemblies);

    /// <summary>
    /// Maps every endpoint type found in <paramref name="assemblies"/> by reflection, using the same discovery rules,
    /// ordering and conventions as the generated registries.
    /// </summary>
    /// <param name="app">The route builder.</param>
    /// <param name="configure">Optional mapping options.</param>
    /// <param name="assemblies">The assemblies to scan, mapped in the given order.</param>
    /// <returns>The same route builder.</returns>
    [RequiresUnreferencedCode(UnreferencedCodeMessage)]
    [RequiresDynamicCode(DynamicCodeMessage)]
    public static IEndpointRouteBuilder MapEndpointsFromAssemblies(
        this IEndpointRouteBuilder app,
        Action<EndpointMappingOptions>? configure,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(assemblies);

        EndpointMappingOptions options = EndpointMapper.CreateOptions(configure);
        ILogger? logger = app.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger(EndpointLog.Category);
        foreach (Assembly assembly in assemblies.Distinct())
        {
            ReflectionEndpointScanner.Map(app, assembly, options, logger);
        }

        return app;
    }
}
