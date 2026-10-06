#if NET7_0_OR_GREATER
namespace CSharpEssentials.DependencyInjection;

/// <summary>
/// Registers the annotated class as <typeparamref name="TService"/> with a
/// <see cref="Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped"/> lifetime.
/// </summary>
/// <typeparam name="TService">The service type.</typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class RegisterScopedAttribute<TService> : Attribute
{
    /// <summary>
    /// Gets the service type.
    /// </summary>
    public Type ServiceType => typeof(TService);

    /// <summary>
    /// Gets or sets the service key. <see langword="null"/> registers a non-keyed service.
    /// </summary>
    public object? Key { get; set; }

    /// <summary>
    /// Gets or sets additional service types to register. Honored only when set explicitly.
    /// </summary>
    public ServiceAs As { get; set; }

    /// <summary>
    /// Gets or sets how the registration treats existing registrations with the same service type and key.
    /// </summary>
    public RegistrationStrategy Strategy { get; set; }
}
#endif
