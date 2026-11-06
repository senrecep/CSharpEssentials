using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CSharpEssentials.Validation.Extensions;

/// <summary>
/// Extension methods for registering <see cref="IValidator{T}"/> implementations
/// with the .NET dependency injection container.
/// </summary>
public static class ValidatorServiceCollectionExtensions
{
    private const string ScanMessage = "Discovers validators by reflection; their constructors may be trimmed. Use AddValidator<T, TValidator>() in trimmed or Native AOT apps.";

    /// <summary>
    /// Registers a single validator explicitly.
    /// Registering the same implementation type more than once is a no-op.
    /// Several <b>different</b> implementations for the same <typeparamref name="T"/> are all kept.
    /// </summary>
    /// <example>
    /// <code>
    /// services.AddValidator&lt;CreateUserRequest, CreateUserValidator&gt;();
    /// </code>
    /// </example>
    public static IServiceCollection AddValidator<T, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TValidator>(
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
        services.TryAddEnumerable(ServiceDescriptor.Describe(
            typeof(IValidator<T>),
            typeof(TValidator),
            lifetime));
        return services;
    }

    /// <summary>
    /// Scans <paramref name="assembly"/> and registers all concrete, non-abstract
    /// <see cref="IValidator{T}"/> implementations found within it.
    /// Scanning the same assembly more than once does not produce duplicate registrations.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assembly">The assembly to scan.</param>
    /// <param name="lifetime">The service lifetime for all registered validators. Defaults to <see cref="ServiceLifetime.Scoped"/>.</param>
    /// <example>
    /// <code>
    /// services.AddValidatorsFromAssembly(typeof(CreateUserValidator).Assembly);
    /// </code>
    /// </example>
    [RequiresUnreferencedCode(ScanMessage)]
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
            types = [.. ex.Types.OfType<Type>()];
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

                services.TryAddEnumerable(ServiceDescriptor.Describe(iface, type, lifetime));
            }
        }

        return services;
    }

    /// <summary>
    /// Scans all assemblies in <paramref name="assemblies"/> and registers all
    /// <see cref="IValidator{T}"/> implementations found within them.
    /// </summary>
    [RequiresUnreferencedCode(ScanMessage)]
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
