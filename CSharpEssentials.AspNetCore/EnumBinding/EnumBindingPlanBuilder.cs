using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Finds the query and route values an endpoint binds to enums. Minimal API endpoints are read from their
/// handler <see cref="MethodInfo"/> (including <see cref="AsParametersAttribute"/> types), MVC actions from their
/// <see cref="ControllerActionDescriptor"/> (including one level of complex type properties).
/// </summary>
internal sealed class EnumBindingPlanBuilder(Predicate<Type> canBind)
{
    private readonly List<EnumBindingTarget> _targets = [];
    private readonly HashSet<(EnumBindingSource, string)> _keys = [];

    public EnumBindingTarget[] Build(Endpoint endpoint)
    {
        ControllerActionDescriptor? action = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
        if (action is not null)
            AddMvcAction(action);
        else if (endpoint.Metadata.GetMetadata<MethodInfo>() is { } method)
            AddMinimalApiHandler(method, (endpoint as RouteEndpoint)?.RoutePattern);
        return [.. _targets];
    }

    private void AddMinimalApiHandler(MethodInfo method, RoutePattern? pattern)
    {
        foreach (ParameterInfo parameter in method.GetParameters())
        {
            if (parameter.Name is null)
                continue;
            if (parameter.IsDefined(typeof(AsParametersAttribute), inherit: false))
            {
                foreach ((string name, Type type, object[] attributes) in GetAsParametersMembers(parameter.ParameterType))
                    AddMinimalApiValue(name, type, attributes, pattern);
                continue;
            }
            AddMinimalApiValue(parameter.Name, parameter.ParameterType, parameter.GetCustomAttributes(inherit: true), pattern);
        }
    }

    private void AddMinimalApiValue(string name, Type type, object[] attributes, RoutePattern? pattern)
    {
        if (!TryGetEnumType(type, out Type? enumType, out bool allowEmpty))
            return;

        foreach (object attribute in attributes)
        {
            switch (attribute)
            {
                case IFromQueryMetadata query:
                    Add(EnumBindingSource.Query, query.Name ?? name, enumType, allowEmpty);
                    return;
                case IFromRouteMetadata route:
                    Add(EnumBindingSource.Route, route.Name ?? name, enumType, allowEmpty);
                    return;
                case IFromBodyMetadata or IFromHeaderMetadata or IFromFormMetadata or IFromServiceMetadata:
                    return;
                default:
                    break;
            }
        }

        // Without an attribute Minimal API binds from the route when the pattern has the parameter, otherwise from the query.
        EnumBindingSource source = pattern?.GetParameter(name) is null ? EnumBindingSource.Query : EnumBindingSource.Route;
        Add(source, name, enumType, allowEmpty);
    }

    private static IEnumerable<(string Name, Type Type, object[] Attributes)> GetAsParametersMembers(Type type)
    {
        ParameterInfo[] constructorParameters = type.GetConstructors()
            .OrderByDescending(static c => c.GetParameters().Length)
            .FirstOrDefault()?.GetParameters() ?? [];
        HashSet<string> seen = [with(StringComparer.OrdinalIgnoreCase)];

        foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            ParameterInfo? constructorParameter = Array.Find(constructorParameters,
                p => string.Equals(p.Name, property.Name, StringComparison.OrdinalIgnoreCase));
            if (constructorParameter is null && property.SetMethod?.IsPublic != true)
                continue;

            object[] attributes = constructorParameter is null
                ? property.GetCustomAttributes(inherit: true)
                : [.. constructorParameter.GetCustomAttributes(inherit: true), .. property.GetCustomAttributes(inherit: true)];
            seen.Add(property.Name);
            yield return (property.Name, property.PropertyType, attributes);
        }

        foreach (ParameterInfo parameter in constructorParameters)
            if (parameter.Name is not null && !seen.Contains(parameter.Name))
                yield return (parameter.Name, parameter.ParameterType, parameter.GetCustomAttributes(inherit: true));
    }

    private void AddMvcAction(ControllerActionDescriptor action)
    {
        foreach (ParameterDescriptor parameter in action.Parameters)
        {
            BindingSource? source = parameter.BindingInfo?.BindingSource;
            string name = parameter.BindingInfo?.BinderModelName ?? parameter.Name;

            if (TryGetEnumType(parameter.ParameterType, out Type? enumType, out bool allowEmpty))
            {
                AddMvcValue(source, name, enumType, allowEmpty);
                continue;
            }

            if (IsComplexType(parameter.ParameterType) && (source is null || source == BindingSource.Query || source == BindingSource.ModelBinding))
                AddMvcComplexType(parameter.ParameterType, name);
        }
    }

    private void AddMvcValue(BindingSource? source, string name, Type enumType, bool allowEmpty)
    {
        if (source == BindingSource.Query)
        {
            Add(EnumBindingSource.Query, name, enumType, allowEmpty);
        }
        else if (source == BindingSource.Path)
        {
            Add(EnumBindingSource.Route, name, enumType, allowEmpty);
        }
        else if (source is null || source == BindingSource.ModelBinding)
        {
            // MVC value providers read the route first, then the query string.
            Add(EnumBindingSource.Route, name, enumType, allowEmpty);
            Add(EnumBindingSource.Query, name, enumType, allowEmpty);
        }
    }

    private void AddMvcComplexType(Type type, string prefix)
    {
        foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.SetMethod?.IsPublic != true ||
                !TryGetEnumType(property.PropertyType, out Type? enumType, out bool allowEmpty))
                continue;

            string name = property.Name;
            EnumBindingSource? source = null;
            foreach (object attribute in property.GetCustomAttributes(inherit: true))
            {
                if (attribute is IFromQueryMetadata query)
                {
                    (name, source) = (query.Name ?? name, EnumBindingSource.Query);
                    break;
                }
                if (attribute is IFromRouteMetadata route)
                {
                    (name, source) = (route.Name ?? name, EnumBindingSource.Route);
                    break;
                }
            }

            // MVC binds "prefix.Name" when the request has a value under the prefix, otherwise "Name".
            foreach ((string key, string? skipWhenPrefixPresent) in ((string, string?)[])[($"{prefix}.{name}", null), (name, prefix)])
            {
                if (source is { } explicitSource)
                {
                    Add(explicitSource, key, enumType, allowEmpty, skipWhenPrefixPresent);
                    continue;
                }
                Add(EnumBindingSource.Route, key, enumType, allowEmpty, skipWhenPrefixPresent);
                Add(EnumBindingSource.Query, key, enumType, allowEmpty, skipWhenPrefixPresent);
            }
        }
    }

    private void Add(EnumBindingSource source, string key, Type enumType, bool allowEmpty, string? skipWhenPrefixPresent = null)
    {
        if (_keys.Add((source, key.ToUpperInvariant())))
            _targets.Add(new EnumBindingTarget(source, key, enumType, allowEmpty, skipWhenPrefixPresent));
    }

    private bool TryGetEnumType(Type type, [NotNullWhen(true)] out Type? enumType, out bool allowEmpty)
    {
        allowEmpty = false;
        Type candidate = type;

        if (candidate != typeof(string))
        {
            Type? elementType = candidate.IsArray ? candidate.GetElementType() : GetEnumerableElementType(candidate);
            if (elementType is not null)
            {
                candidate = elementType;
                allowEmpty = true;
            }
        }

        if (Nullable.GetUnderlyingType(candidate) is { } underlying)
        {
            candidate = underlying;
            allowEmpty = true;
        }

        enumType = candidate.IsEnum && canBind(candidate) ? candidate : null;
        return enumType is not null;
    }

    private static Type? GetEnumerableElementType(Type type)
    {
        if (!type.IsGenericType)
            return null;
        Type[] arguments = type.GetGenericArguments();
        if (arguments.Length != 1)
            return null;
        Type enumerable = typeof(IEnumerable<>).MakeGenericType(arguments[0]);
        return enumerable.IsAssignableFrom(type) ? arguments[0] : null;
    }

    private static bool IsComplexType(Type type) =>
        type.IsClass && type != typeof(string) && !type.IsArray && !typeof(System.Collections.IEnumerable).IsAssignableFrom(type);
}
