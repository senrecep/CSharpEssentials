using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Endpoints;

[RequiresUnreferencedCode("Scans assemblies for IEndpoint types.")]
[RequiresDynamicCode("Closes EndpointMapper generic methods at runtime.")]
internal static class ReflectionEndpointScanner
{
    private static readonly MethodInfo MapGroupMethod = typeof(EndpointMapper).GetMethod(nameof(EndpointMapper.MapGroup))!;

    private static readonly MethodInfo MapEndpointMethod = typeof(EndpointMapper).GetMethod(nameof(EndpointMapper.MapEndpoint))!;

    public static void Map(IEndpointRouteBuilder app, Assembly assembly, EndpointMappingOptions options, ILogger? logger)
    {
        if (assembly.IsDefined(typeof(ExcludeFromMappingAttribute), inherit: false))
        {
            return;
        }

        ReflectionGroupNode root = new(null);
        foreach (Type endpointType in GetLoadableTypes(assembly, logger)
            .Where(static type => !type.IsInterface && typeof(IEndpoint).IsAssignableFrom(type))
            .OrderBy(static type => type.FullName, StringComparer.Ordinal))
        {
            if (TryResolveGroups(endpointType, logger, out List<Type> groups))
            {
                Add(root, endpointType, groups);
            }
        }

        MapNode(app, root, options);
    }

    private static Type[] GetLoadableTypes(Assembly assembly, ILogger? logger)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            if (logger is not null)
            {
                string assemblyName = assembly.GetName().Name ?? assembly.FullName ?? string.Empty;
                foreach (Exception? loaderException in exception.LoaderExceptions)
                {
                    if (loaderException is not null)
                    {
                        string typeName = (loaderException as TypeLoadException)?.TypeName ?? "<unknown>";
                        EndpointLog.TypeLoadFailed(logger, typeName, assemblyName, loaderException);
                    }
                }
            }

            return [.. exception.Types.OfType<Type>()];
        }
    }

    private static bool TryResolveGroups(Type endpointType, ILogger? logger, out List<Type> groups)
    {
        groups = [];
        if (IsExcluded(endpointType) || IsAbstractOrOpenGeneric(endpointType))
        {
            return false;
        }

        if (!IsAccessible(endpointType))
        {
            return Skip(logger, endpointType, "the type is not accessible from generated code (CSE1001)");
        }

        HashSet<Type> visited = [endpointType];
        Type current = endpointType;
        while (true)
        {
            Type[] targets = GetGroupTargets(current);
            if (targets.Length == 0)
            {
                return true;
            }

            if (targets.Length > 1)
            {
                return Skip(logger, endpointType, $"'{current}' has more than one group attribute (CSE1003)");
            }

            Type group = targets[0];
            if (!typeof(IEndpointGroup).IsAssignableFrom(group) || group.IsInterface || IsAbstractOrOpenGeneric(group))
            {
                return Skip(logger, endpointType, $"group target '{group}' of '{current}' is not a concrete IEndpointGroup (CSE1007)");
            }

            if (!visited.Add(group))
            {
                return Skip(logger, endpointType, $"the group chain contains a cycle at '{group}' (CSE1002)");
            }

            if (!IsAccessible(group))
            {
                return Skip(logger, endpointType, $"group '{group}' is not accessible from generated code (CSE1001)");
            }

            if (IsExcluded(group))
            {
                groups.Clear();
                return false;
            }

            groups.Insert(0, group);
            current = group;
        }
    }

    private static bool Skip(ILogger? logger, Type endpointType, string reason)
    {
        if (logger is not null)
        {
            EndpointLog.InvalidType(logger, endpointType, reason);
        }

        return false;
    }

    private static Type[] GetGroupTargets(Type type) =>
        [.. type.GetCustomAttributes(inherit: false)
            .Select(static attribute => attribute switch
            {
                EndpointGroupAttribute plain => plain.GroupType,
                _ when attribute.GetType() is { IsGenericType: true } attributeType &&
                       attributeType.GetGenericTypeDefinition() == typeof(EndpointGroupAttribute<>) => attributeType.GetGenericArguments()[0],
                _ => null,
            })
            .OfType<Type>()];

    private static bool IsExcluded(Type type) => type.IsDefined(typeof(ExcludeFromMappingAttribute), inherit: false);

    private static bool IsAbstractOrOpenGeneric(Type type) => type.IsAbstract || type.ContainsGenericParameters;

    private static bool IsAccessible(Type type)
    {
        for (Type? current = type; current is not null; current = current.DeclaringType)
        {
            bool visible = current.IsNested
                ? current.IsNestedPublic || current.IsNestedAssembly || current.IsNestedFamORAssem
                : current.IsPublic || current.IsNotPublic;
            if (!visible || current.Name.Contains('<', StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static void Add(ReflectionGroupNode root, Type endpointType, List<Type> groups)
    {
        ReflectionGroupNode node = root;
        foreach (Type group in groups)
        {
            string key = group.FullName ?? group.Name;
            if (!node.Children.TryGetValue(key, out ReflectionGroupNode? child))
            {
                child = new ReflectionGroupNode(group);
                node.Children.Add(key, child);
            }

            node = child;
        }

        node.Endpoints.Add(endpointType);
    }

    private static void MapNode(IEndpointRouteBuilder parent, ReflectionGroupNode node, EndpointMappingOptions options)
    {
        foreach (Type endpointType in node.Endpoints)
        {
            Invoke(MapEndpointMethod, endpointType, parent, node.GroupType, options);
        }

        foreach (ReflectionGroupNode child in node.Children.Values)
        {
            if (EndpointMapper.ShouldMapAny(parent, options, [.. child.InMappingOrder()]))
            {
                var group = (IEndpointRouteBuilder)Invoke(MapGroupMethod, child.GroupType!, parent)!;
                MapNode(group, child, options);
            }
        }
    }

    private static object? Invoke(MethodInfo method, Type typeArgument, params object?[] arguments) =>
        method.MakeGenericMethod(typeArgument).Invoke(null, BindingFlags.DoNotWrapExceptions, null, arguments, null);
}
