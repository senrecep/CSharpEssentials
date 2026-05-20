using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Validation.Extensions;

/// <summary>
/// Extension methods for registering <see cref="IValidator{T}"/> implementations
/// with the .NET dependency injection container.
/// </summary>
public static class ValidatorServiceCollectionExtensions
{
    /// <summary>
    /// Registers a single validator explicitly.
    /// </summary>
    /// <example>
    /// <code>
    /// services.AddValidator&lt;CreateUserRequest, CreateUserValidator&gt;();
    /// </code>
    /// </example>
    public static IServiceCollection AddValidator<T, TValidator>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TValidator : class, IValidator<T>
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(services);
#else
        if (services is null)
            throw new ArgumentNullException(nameof(services));
#endif
        services.Add(ServiceDescriptor.Describe(
            typeof(IValidator<T>),
            typeof(TValidator),
            lifetime));
        return services;
    }

    /// <summary>
    /// Scans <paramref name="assembly"/> and registers all concrete, non-abstract
    /// <see cref="IValidator{T}"/> implementations found within it.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assembly">The assembly to scan.</param>
    /// <param name="lifetime">The service lifetime for all registered validators. Defaults to <see cref="ServiceLifetime.Scoped"/>.</param>
    /// <example>
    /// <code>
    /// services.AddValidatorsFromAssembly(typeof(CreateUserValidator).Assembly);
    /// </code>
    /// </example>
    public static IServiceCollection AddValidatorsFromAssembly(
        this IServiceCollection services,
        Assembly assembly,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assembly);
#else
        if (services is null)
            throw new ArgumentNullException(nameof(services));
        if (assembly is null)
            throw new ArgumentNullException(nameof(assembly));
#endif
        Type validatorInterface = typeof(IValidator<>);

        Type[] types;
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
#pragma warning disable IDE0305
            types = [.. ex.Types.OfType<Type>()];
#pragma warning restore IDE0305
        }

        foreach (Type type in types)
        {
            if (type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition)
                continue;

            foreach (Type iface in type.GetInterfaces())
            {
                if (!iface.IsGenericType)
                    continue;
                if (iface.GetGenericTypeDefinition() != validatorInterface)
                    continue;

                services.Add(ServiceDescriptor.Describe(iface, type, lifetime));
            }
        }

        return services;
    }

    /// <summary>
    /// Scans all assemblies in <paramref name="assemblies"/> and registers all
    /// <see cref="IValidator{T}"/> implementations found within them.
    /// </summary>
    public static IServiceCollection AddValidatorsFromAssemblies(
        this IServiceCollection services,
        IEnumerable<Assembly> assemblies,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);
#else
        if (services is null)
            throw new ArgumentNullException(nameof(services));
        if (assemblies is null)
            throw new ArgumentNullException(nameof(assemblies));
#endif
        foreach (Assembly assembly in assemblies)
            services.AddValidatorsFromAssembly(assembly, lifetime);
        return services;
    }
}
