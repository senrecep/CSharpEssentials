using System.ComponentModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Endpoints;

/// <summary>
/// Runtime mapping helper shared by generated registries and the reflection fallback.
/// It implements wrapping, convention ordering, naming, tagging, filtering and logging.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class EndpointMapper
{
    private const string GroupSuffix = "Group";

    /// <summary>
    /// Creates the options for one mapping call.
    /// </summary>
    /// <param name="configure">Optional configuration callback.</param>
    /// <returns>The configured options.</returns>
    public static EndpointMappingOptions CreateOptions(Action<EndpointMappingOptions>? configure)
    {
        EndpointMappingOptions options = new();
        configure?.Invoke(options);
        return options;
    }

    /// <summary>
    /// Determines whether at least one of <paramref name="endpointTypes"/> passes the mapping filters,
    /// so that a group without mapped endpoints is not created.
    /// </summary>
    /// <param name="app">The builder the endpoints are mapped on.</param>
    /// <param name="options">The mapping options.</param>
    /// <param name="endpointTypes">The endpoint types placed under the group, directly or through nested groups.</param>
    /// <returns><see langword="true"/> when at least one endpoint type is mapped.</returns>
    public static bool ShouldMapAny(IEndpointRouteBuilder app, EndpointMappingOptions options, params Type[] endpointTypes)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(endpointTypes);

        ILogger? logger = CreateLogger(app, options);
        bool any = false;
        foreach (Type endpointType in endpointTypes)
        {
            any |= Includes(options, endpointType, logger);
        }

        return any;
    }

    /// <summary>
    /// Creates the route group for <typeparamref name="TGroup"/> under <paramref name="parent"/> and applies its conventions.
    /// </summary>
    /// <typeparam name="TGroup">The group type.</typeparam>
    /// <param name="parent">The parent builder.</param>
    /// <returns>The created group builder.</returns>
    public static RouteGroupBuilder MapGroup<TGroup>(IEndpointRouteBuilder parent)
        where TGroup : IEndpointGroup
    {
        ArgumentNullException.ThrowIfNull(parent);

        RouteGroupBuilder group = parent.MapGroup(TGroup.Prefix);
        TGroup.Configure(group);
        return group;
    }

    /// <summary>
    /// Maps <typeparamref name="TEndpoint"/> inside its own empty-prefix group under <paramref name="parent"/>,
    /// then applies per-type metadata and the <see cref="EndpointMappingOptions.ConfigureEach"/> callbacks.
    /// </summary>
    /// <typeparam name="TEndpoint">The endpoint type.</typeparam>
    /// <param name="parent">The parent builder.</param>
    /// <param name="innermostGroup">The innermost group type, or <see langword="null"/> for a root-level endpoint.</param>
    /// <param name="options">The mapping options.</param>
    public static void MapEndpoint<TEndpoint>(IEndpointRouteBuilder parent, Type? innermostGroup, EndpointMappingOptions options)
        where TEndpoint : IEndpoint
    {
        ArgumentNullException.ThrowIfNull(parent);
        ArgumentNullException.ThrowIfNull(options);

        Type endpointType = typeof(TEndpoint);
        ILogger? logger = CreateLogger(parent, options);
        if (!Includes(options, endpointType, logger))
        {
            return;
        }

        RouteGroupBuilder group = parent.MapGroup(string.Empty);
        TEndpoint.Map(group);
        group.WithMetadata(new EndpointTypeMetadata(endpointType));
        IEndpointConventionBuilder conventions = group;

        if (options.AutoTagFromGroup && innermostGroup is not null)
        {
            string tag = GetGroupTag(innermostGroup);
            conventions.Finally(endpoint =>
            {
                if (!endpoint.Metadata.OfType<ITagsMetadata>().Any())
                {
                    endpoint.Metadata.Add(new TagsAttribute(tag));
                }
            });
        }

        OperationNaming naming = options.OperationNaming;
        if (naming.IsTypeName)
        {
            var registry = OperationNameRegistry.For(parent.ServiceProvider);
            int mapping = registry.Register(endpointType);
            conventions.Finally(endpoint => registry.Apply(endpoint, endpointType, mapping));
        }
        else if (!naming.IsNone)
        {
            conventions.Finally(endpoint => ApplyName(endpoint, endpointType, naming));
        }

        foreach (Action<IEndpointConventionBuilder, Type> configure in options.ConfigureEachCallbacks)
        {
            configure(group, endpointType);
        }

        if (logger is not null)
        {
            EndpointLog.Mapped(logger, endpointType);
        }
    }

    /// <summary>
    /// Reserves endpoint names set explicitly in the application, so that <see cref="OperationNaming.TypeName"/>
    /// gives a numeric suffix to a generated name that would collide with one of them.
    /// </summary>
    /// <param name="app">The builder the endpoints are mapped on.</param>
    /// <param name="options">The mapping options.</param>
    /// <param name="names">The explicit endpoint names found at build time.</param>
    public static void ReserveEndpointNames(IEndpointRouteBuilder app, EndpointMappingOptions options, IReadOnlyList<string> names)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(names);

        if (options.OperationNaming.IsTypeName)
        {
            OperationNameRegistry.For(app.ServiceProvider).Reserve(names);
        }
    }

    private static bool Includes(EndpointMappingOptions options, Type endpointType, ILogger? logger)
    {
        bool included = options.Includes(endpointType, out bool evaluated);
        if (!included && evaluated && logger is not null)
        {
            EndpointLog.Filtered(logger, endpointType);
        }

        return included;
    }

    private static void ApplyName(EndpointBuilder endpoint, Type endpointType, OperationNaming naming)
    {
        if (endpoint.Metadata.OfType<IEndpointNameMetadata>().Any())
        {
            return;
        }

        string? name = naming.GetName(endpointType, endpoint);
        if (name is null)
        {
            return;
        }

        endpoint.Metadata.Add(new EndpointNameMetadata(name));
        endpoint.Metadata.Add(new RouteNameMetadata(name));
    }

    private static string GetGroupTag(Type groupType)
    {
        string name = groupType.Name;
        return name.Length > GroupSuffix.Length && name.EndsWith(GroupSuffix, StringComparison.Ordinal)
            ? name[..^GroupSuffix.Length]
            : name;
    }

    private static ILogger? CreateLogger(IEndpointRouteBuilder app, EndpointMappingOptions options) =>
        options.LogDiscovered
            ? app.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger(EndpointLog.Category)
            : null;
}
