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

        return (inner, provider) =>
        {
            object?[] arguments = new object?[resolvers.Length];
            for (int index = 0; index < resolvers.Length; index++)
            {
                arguments[index] = resolvers[index](inner, provider);
            }

            return constructor.Invoke(BindingFlags.DoNotWrapExceptions, binder: null, arguments, culture: null);
        };
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
