using System.ComponentModel;
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
/// Finds the route, query, header and form values an endpoint binds to enums. Minimal API endpoints are read from their
/// handler <see cref="MethodInfo"/> (including <see cref="AsParametersAttribute"/> types), MVC actions from their
/// <see cref="ControllerActionDescriptor"/> (including nested complex type properties, up to 8 levels).
/// </summary>
internal sealed class EnumBindingPlanBuilder(Func<Type, EnumBindingNormalizer?> resolve)
{
    // Nested query models deeper than this are left to the MVC binder (and its own error response).
    private const int MaxComplexTypeDepth = 8;

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
        if (!TryGetEnumType(type, out EnumBindingValue value))
            return;

        foreach (object attribute in attributes)
        {
            switch (attribute)
            {
                case IFromQueryMetadata query:
                    Add(EnumBindingSource.Query, query.Name ?? name, value);
                    return;
                case IFromRouteMetadata route:
                    Add(EnumBindingSource.Route, route.Name ?? name, value);
                    return;
                case IFromHeaderMetadata header:
                    Add(EnumBindingSource.Header, header.Name ?? name, value);
                    return;
                case IFromFormMetadata form:
                    Add(EnumBindingSource.Form, form.Name ?? name, value);
                    return;
                case IFromBodyMetadata or IFromServiceMetadata:
                    return;
                default:
                    break;
            }
        }

        // Without an attribute Minimal API binds from the route when the pattern has the parameter, otherwise from the query.
        EnumBindingSource source = pattern?.GetParameter(name) is null ? EnumBindingSource.Query : EnumBindingSource.Route;
        Add(source, name, value);
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

            if (TryGetEnumType(parameter.ParameterType, out EnumBindingValue value))
            {
                AddMvcValue(source, name, value);
                continue;
            }

            if (IsComplexType(parameter.ParameterType) &&
                (source is null || source == BindingSource.Query || source == BindingSource.ModelBinding || source == BindingSource.Form))
                AddMvcComplexType(parameter.ParameterType, name);
        }
    }

    private void AddMvcValue(BindingSource? source, string name, EnumBindingValue value)
    {
        if (source == BindingSource.Query)
        {
            Add(EnumBindingSource.Query, name, value);
        }
        else if (source == BindingSource.Path)
        {
            Add(EnumBindingSource.Route, name, value);
        }
        else if (source == BindingSource.Header)
        {
            Add(EnumBindingSource.Header, name, value);
        }
        else if (source == BindingSource.Form)
        {
            Add(EnumBindingSource.Form, name, value);
        }
        else if (source is null || source == BindingSource.ModelBinding)
        {
            // MVC value providers read the form, the route and the query string.
            Add(EnumBindingSource.Form, name, value, firstSourceWins: true);
            Add(EnumBindingSource.Route, name, value, firstSourceWins: true);
            Add(EnumBindingSource.Query, name, value, firstSourceWins: true);
        }
    }

    private void AddMvcComplexType(Type type, string prefix) =>
        AddMvcComplexType(type, prefix, path: null, depth: 0, visiting: []);

    private void AddMvcComplexType(Type type, string prefix, string? path, int depth, HashSet<Type> visiting)
    {
        if (depth >= MaxComplexTypeDepth || !visiting.Add(type))
            return;

        foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.SetMethod?.IsPublic != true || property.GetIndexParameters().Length > 0)
                continue;

            (string name, EnumBindingSource? source, bool excluded) = GetMvcPropertyBinding(property);
            if (excluded)
                continue;

            string propertyPath = path is null ? name : $"{path}.{name}";
            if (TryGetEnumType(property.PropertyType, out EnumBindingValue value))
            {
                if (source == EnumBindingSource.Header)
                    Add(EnumBindingSource.Header, name, value); // MVC binds a header property by the header name only.
                else
                    AddMvcPropertyValue(prefix, propertyPath, source, value);
                continue;
            }

            if (source is null && IsComplexType(property.PropertyType))
                AddMvcComplexType(property.PropertyType, prefix, propertyPath, depth + 1, visiting);
        }

        visiting.Remove(type);
    }

    private static (string Name, EnumBindingSource? Source, bool Excluded) GetMvcPropertyBinding(PropertyInfo property)
    {
        foreach (object attribute in property.GetCustomAttributes(inherit: true))
        {
            switch (attribute)
            {
                case IFromQueryMetadata query:
                    return (query.Name ?? property.Name, EnumBindingSource.Query, false);
                case IFromRouteMetadata route:
                    return (route.Name ?? property.Name, EnumBindingSource.Route, false);
                case IFromHeaderMetadata header:
                    return (header.Name ?? property.Name, EnumBindingSource.Header, false);
                case IFromFormMetadata form:
                    return (form.Name ?? property.Name, EnumBindingSource.Form, false);
                case IFromBodyMetadata or IFromServiceMetadata or BindNeverAttribute:
                    return (property.Name, null, true);
                default:
                    break;
            }
        }
        return (property.Name, null, false);
    }

    private void AddMvcPropertyValue(string prefix, string path, EnumBindingSource? source, EnumBindingValue value)
    {
        // MVC binds "prefix.path" when the request has a value under the prefix, otherwise "path".
        foreach ((string key, string? skipWhenPrefixPresent) in ((string, string?)[])[($"{prefix}.{path}", null), (path, prefix)])
        {
            if (source is { } explicitSource)
            {
                Add(explicitSource, key, value, skipWhenPrefixPresent);
                continue;
            }
            Add(EnumBindingSource.Form, key, value, skipWhenPrefixPresent, firstSourceWins: true);
            Add(EnumBindingSource.Route, key, value, skipWhenPrefixPresent, firstSourceWins: true);
            Add(EnumBindingSource.Query, key, value, skipWhenPrefixPresent, firstSourceWins: true);
        }
    }

    private void Add(EnumBindingSource source, string key, EnumBindingValue value, string? skipWhenPrefixPresent = null, bool firstSourceWins = false)
    {
        if (_keys.Add((source, key.ToUpperInvariant())))
            _targets.Add(new EnumBindingTarget(source, key, value.Normalizer, value.AllowEmpty, value.IsCollection, skipWhenPrefixPresent, firstSourceWins));
    }

    private bool TryGetEnumType(Type type, out EnumBindingValue value)
    {
        value = default;
        bool isCollection = false;
        bool allowEmpty = false;
        Type candidate = type;

        if (candidate != typeof(string))
        {
            Type? elementType = candidate.IsArray ? candidate.GetElementType() : GetEnumerableElementType(candidate);
            if (elementType is not null)
            {
                candidate = elementType;
                isCollection = true;
                allowEmpty = true;
            }
        }

        if (Nullable.GetUnderlyingType(candidate) is { } underlying)
        {
            candidate = underlying;
            allowEmpty = true;
        }

        if (!candidate.IsEnum || resolve(candidate) is not { } normalizer)
            return false;

        value = new EnumBindingValue(normalizer, allowEmpty, isCollection);
        return true;
    }

    private readonly record struct EnumBindingValue(EnumBindingNormalizer Normalizer, bool AllowEmpty, bool IsCollection);

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

    // Mirrors MVC: a type with a string TypeConverter (Uri, Version, ...) is bound as a simple value, not by its properties.
    private static bool IsComplexType(Type type) =>
        type.IsClass && type != typeof(string) && !type.IsArray && !typeof(System.Collections.IEnumerable).IsAssignableFrom(type) &&
        !TypeDescriptor.GetConverter(type).CanConvertFrom(typeof(string));
}
