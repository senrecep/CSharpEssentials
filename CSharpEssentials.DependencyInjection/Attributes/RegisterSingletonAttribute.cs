namespace CSharpEssentials.DependencyInjection;

/// <summary>
/// Registers the annotated class with a <see cref="Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton"/> lifetime.
/// </summary>
/// <remarks>
/// Without a service type and without <see cref="As"/>, the service type is resolved by the default rule:
/// an implemented interface named <c>I{TypeName}</c> with the same generic arity, otherwise the class itself.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class RegisterSingletonAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance that resolves the service type by the default rule.
    /// </summary>
    public RegisterSingletonAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance that registers the class as <paramref name="serviceType"/>.
    /// </summary>
    /// <param name="serviceType">The service type. May be an open generic type definition for open generic classes.</param>
    public RegisterSingletonAttribute(Type serviceType) => ServiceType = serviceType;

    /// <summary>
    /// Gets the explicit service type, or <see langword="null"/> when the default rule applies.
    /// </summary>
    public Type? ServiceType { get; }

    /// <summary>
    /// Gets or sets the service key. <see langword="null"/> registers a non-keyed service.
    /// </summary>
    public object? Key { get; set; }

    /// <summary>
    /// Gets or sets which service types are registered. Honored only when set explicitly.
    /// </summary>
    public ServiceAs As { get; set; }

    /// <summary>
    /// Gets or sets how the registration treats existing registrations with the same service type and key.
    /// </summary>
    public RegistrationStrategy Strategy { get; set; }
}
