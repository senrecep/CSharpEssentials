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
        object? serviceKey)
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
                $"Decorator '{decoratorType.FullName}' must have exactly one public constructor, but has {constructors.Length}.");
        }

        ConstructorInfo constructor = constructors[0];
        ParameterInfo[] parameters = constructor.GetParameters();
        if (parameters.Count(parameter => parameter.ParameterType == serviceType) != 1)
        {
            throw new InvalidOperationException(
                $"The constructor of decorator '{decoratorType.FullName}' must have exactly one parameter of the decorated service type '{serviceType.FullName}'.");
        }

        Func<object, IServiceProvider, object?>[] resolvers = [.. parameters.Select(parameter => CreateResolver(parameter, serviceType, serviceKey))];
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
