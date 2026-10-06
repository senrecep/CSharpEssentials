using System.ComponentModel;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.DependencyInjection;

/// <summary>
/// Applies service descriptors through a key-aware <see cref="RegistrationStrategy"/>. Used by generated registries
/// and by <c>AddServicesFromAssemblies</c>.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ServiceRegistration
{
    /// <summary>
    /// Applies <paramref name="descriptor"/> to <paramref name="services"/> with <paramref name="strategy"/>.
    /// When <paramref name="logger"/> has <see cref="LogLevel.Debug"/> enabled, a registration whose service type and
    /// key are already registered is logged.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="logger">An optional logger for duplicate registrations.</param>
    /// <param name="descriptor">The descriptor to apply.</param>
    /// <param name="strategy">The registration strategy.</param>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="strategy"/> is <see cref="RegistrationStrategy.Throw"/> and a matching registration exists.
    /// </exception>
    public static void Apply(IServiceCollection services, ILogger? logger, ServiceDescriptor descriptor, RegistrationStrategy strategy)
    {
        Guard.NotNull(services);
        Guard.NotNull(descriptor);

        bool logDuplicates = logger is not null && logger.IsEnabled(LogLevel.Debug);
        ServiceDescriptor? existing = logDuplicates || strategy is RegistrationStrategy.TryAdd or RegistrationStrategy.Throw
            ? FindFirst(services, descriptor)
            : null;
        if (existing is not null && logDuplicates)
        {
            LogDuplicate(logger!, existing, descriptor, strategy);
        }

        switch (strategy)
        {
            case RegistrationStrategy.Add:
                services.Add(descriptor);
                break;
            case RegistrationStrategy.TryAdd:
                AddIf(services, descriptor, existing is null);
                break;
            case RegistrationStrategy.TryAddEnumerable:
                AddIf(services, descriptor, !ContainsImplementation(services, descriptor));
                break;
            case RegistrationStrategy.Replace:
                RemoveAll(services, descriptor);
                services.Add(descriptor);
                break;
            case RegistrationStrategy.Throw:
                ThrowIfExists(existing, descriptor);
                services.Add(descriptor);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(strategy), strategy, "Unknown registration strategy.");
        }
    }

    /// <summary>
    /// Determines whether the services of <paramref name="assembly"/> are already registered in <paramref name="services"/>.
    /// Generated registries and <c>AddServicesFromAssemblies</c> check it so that registering an assembly again is a no-op.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assembly">The assembly whose services are about to be registered.</param>
    /// <returns><see langword="true"/> when the assembly is marked as registered; otherwise <see langword="false"/>.</returns>
    public static bool IsRegistered(IServiceCollection services, Assembly assembly)
    {
        Guard.NotNull(services);
        Guard.NotNull(assembly);

        foreach (ServiceDescriptor descriptor in services)
        {
            if (descriptor.ServiceType == typeof(RegisteredAssemblyMarker) &&
                !descriptor.IsKeyedService &&
                descriptor.ImplementationInstance is RegisteredAssemblyMarker marker &&
                marker.Assembly == assembly)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Marks the services of <paramref name="assembly"/> as registered in <paramref name="services"/> by adding a singleton
    /// marker descriptor. Generated registries and <c>AddServicesFromAssemblies</c> call it only after the registrations of
    /// the assembly succeeded, so a failed registration can be retried.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assembly">The assembly whose services were registered.</param>
    public static void MarkRegistered(IServiceCollection services, Assembly assembly)
    {
        Guard.NotNull(services);
        Guard.NotNull(assembly);

        services.Add(ServiceDescriptor.Singleton(new RegisteredAssemblyMarker(assembly)));
    }

    private static void LogDuplicate(ILogger logger, ServiceDescriptor existing, ServiceDescriptor descriptor, RegistrationStrategy strategy)
    {
        if (!logger.IsEnabled(LogLevel.Debug))
        {
            return;
        }

        string serviceKey = ServiceDescriptorExtensions.DescribeKey(descriptor.ServiceKey);
        string existingImplementation = existing.DescribeImplementation();
        string newImplementation = descriptor.DescribeImplementation();
        ServiceRegistrationLog.DuplicateRegistration(logger, descriptor.ServiceType, serviceKey, existingImplementation, newImplementation, strategy);
    }

    private static ServiceDescriptor? FindFirst(IServiceCollection services, ServiceDescriptor descriptor)
    {
        foreach (ServiceDescriptor candidate in services)
        {
            if (candidate.Matches(descriptor.ServiceType, descriptor.ServiceKey))
            {
                return candidate;
            }
        }

        return null;
    }

    private static bool ContainsImplementation(IServiceCollection services, ServiceDescriptor descriptor)
    {
        object? identity = descriptor.GetImplementationIdentity();
        foreach (ServiceDescriptor candidate in services)
        {
            if (candidate.Matches(descriptor.ServiceType, descriptor.ServiceKey) && Equals(candidate.GetImplementationIdentity(), identity))
            {
                return true;
            }
        }

        return false;
    }

    private static void AddIf(IServiceCollection services, ServiceDescriptor descriptor, bool condition)
    {
        if (condition)
        {
            services.Add(descriptor);
        }
    }

    private static void RemoveAll(IServiceCollection services, ServiceDescriptor descriptor)
    {
        for (int index = services.Count - 1; index >= 0; index--)
        {
            if (services[index].Matches(descriptor.ServiceType, descriptor.ServiceKey))
            {
                services.RemoveAt(index);
            }
        }
    }

    private static void ThrowIfExists(ServiceDescriptor? existing, ServiceDescriptor descriptor)
    {
        if (existing is not null)
        {
            throw new InvalidOperationException(
                $"Service '{descriptor.ServiceType.FullName}' with key '{ServiceDescriptorExtensions.DescribeKey(descriptor.ServiceKey)}' " +
                $"is already registered by '{existing.DescribeImplementation()}'; registration strategy Throw does not allow " +
                $"'{descriptor.DescribeImplementation()}' to be added.");
        }
    }
}
