using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.DependencyInjection;

internal static class DecoratorActivator
{
    private static readonly PropertyInfo? LookupModeProperty = typeof(FromKeyedServicesAttribute).GetProperty("LookupMode");

    public static Func<object, IServiceProvider, object> Create(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type decoratorType,
        Type serviceType,
        object? serviceKey,
        string? missingConstructorHint = null)
    {
        if (decoratorType.IsAbstract || decoratorType.IsInterface)
        {
            throw new InvalidOperationException($"Decorator '{decoratorType.FullName}' is abstract or static and cannot be constructed.");
        }

        if (!serviceType.IsAssignableFrom(decoratorType))
        {
            throw new InvalidOperationException($"Decorator '{decoratorType.FullName}' does not implement '{serviceType.FullName}'.");
        }

        ConstructorInfo[] constructors = decoratorType.GetConstructors();
        if (constructors.Length != 1)
        {
            throw new InvalidOperationException(
                $"Decorator '{decoratorType.FullName}' must have exactly one public constructor, but has {constructors.Length}." +
                (constructors.Length == 0 && missingConstructorHint is not null ? " " + missingConstructorHint : string.Empty));
        }

        ConstructorInfo constructor = constructors[0];
        ParameterInfo[] parameters = constructor.GetParameters();
        int innerCount = 0;
        foreach (ParameterInfo parameter in parameters)
        {
            if (parameter.ParameterType == serviceType)
            {
                innerCount++;
            }
        }

        if (innerCount != 1)
        {
            throw new InvalidOperationException(
                $"The constructor of decorator '{decoratorType.FullName}' must have exactly one parameter of the decorated service type '{serviceType.FullName}'.");
        }

        var resolvers = new Func<object, IServiceProvider, object?>[parameters.Length];
        for (int index = 0; index < parameters.Length; index++)
        {
            resolvers[index] = CreateResolver(parameters[index], serviceType, serviceKey);
        }

#if NET8_0_OR_GREATER
        var invoker = ConstructorInvoker.Create(constructor);
        return resolvers.Length switch
        {
            1 => (inner, provider) => invoker.Invoke(resolvers[0](inner, provider)),
            2 => (inner, provider) => invoker.Invoke(resolvers[0](inner, provider), resolvers[1](inner, provider)),
            3 => (inner, provider) => invoker.Invoke(
                resolvers[0](inner, provider),
                resolvers[1](inner, provider),
                resolvers[2](inner, provider)),
            4 => (inner, provider) => invoker.Invoke(
                resolvers[0](inner, provider),
                resolvers[1](inner, provider),
                resolvers[2](inner, provider),
                resolvers[3](inner, provider)),
            _ => (inner, provider) => invoker.Invoke(Resolve(resolvers, inner, provider)),
        };
#else
        return (inner, provider) =>
            constructor.Invoke(BindingFlags.DoNotWrapExceptions, binder: null, Resolve(resolvers, inner, provider), culture: null);
#endif
    }

    private static object?[] Resolve(Func<object, IServiceProvider, object?>[] resolvers, object inner, IServiceProvider provider)
    {
        object?[] arguments = new object?[resolvers.Length];
        for (int index = 0; index < resolvers.Length; index++)
        {
            arguments[index] = resolvers[index](inner, provider);
        }

        return arguments;
    }

    private static Func<object, IServiceProvider, object?> CreateResolver(ParameterInfo parameter, Type serviceType, object? serviceKey)
    {
        Type parameterType = parameter.ParameterType;
        if (parameterType == serviceType)
        {
            return static (inner, _) => inner;
        }

        if (parameter.GetCustomAttribute<ServiceKeyAttribute>() is not null)
        {
            return (_, _) => serviceKey;
        }

        if (parameter.GetCustomAttribute<FromKeyedServicesAttribute>() is { } fromKeyed)
        {
            object? key = InheritsKey(fromKeyed) ? serviceKey : fromKeyed.Key;
            return key is null
                ? (_, provider) => provider.GetRequiredService(parameterType)
                : (_, provider) => provider.GetRequiredKeyedService(parameterType, key);
        }

        if (parameter.HasDefaultValue)
        {
            object? defaultValue = parameter.DefaultValue;
            return (_, provider) => provider.GetService(parameterType) ?? defaultValue;
        }

        return (_, provider) => provider.GetRequiredService(parameterType);
    }

    private static bool InheritsKey(FromKeyedServicesAttribute attribute) =>
        LookupModeProperty?.GetValue(attribute) is { } mode &&
        string.Equals(mode.ToString(), "InheritKey", StringComparison.Ordinal);
}
