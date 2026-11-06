using System.ComponentModel;

namespace CSharpEssentials.DependencyInjection;

/// <summary>
/// One decorator of a generated registry, applied by <see cref="ServiceRegistration.Decorate(Microsoft.Extensions.DependencyInjection.IServiceCollection, DecoratorRegistration[])"/>.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class DecoratorRegistration
{
    /// <summary>Initializes a new instance of the <see cref="DecoratorRegistration"/> class.</summary>
    /// <param name="serviceType">The decorated service type.</param>
    /// <param name="serviceKey">The service key of the decorated registrations, or <see langword="null"/> for non-keyed registrations.</param>
    /// <param name="decorator">Creates the decorator from the inner service and the service provider.</param>
    /// <param name="verifiedImplementations">
    /// Implementation types already known not to inject <see cref="Microsoft.Extensions.DependencyInjection.ServiceKeyAttribute"/>; their constructors are not inspected.
    /// </param>
    public DecoratorRegistration(
        Type serviceType,
        object? serviceKey,
        Func<object, IServiceProvider, object> decorator,
        Type[] verifiedImplementations)
    {
        Guard.NotNull(serviceType);
        Guard.NotNull(decorator);
        Guard.NotNull(verifiedImplementations);

        ServiceType = serviceType;
        ServiceKey = serviceKey;
        Decorator = decorator;
        VerifiedImplementations = verifiedImplementations;
    }

    internal Type ServiceType { get; }

    internal object? ServiceKey { get; }

    internal Func<object, IServiceProvider, object> Decorator { get; }

    internal Type[] VerifiedImplementations { get; }
}
