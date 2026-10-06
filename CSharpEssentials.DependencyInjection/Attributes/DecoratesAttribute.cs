namespace CSharpEssentials.DependencyInjection;

/// <summary>
/// Applies the annotated class as a decorator of <see cref="ServiceType"/>.
/// </summary>
/// <remarks>
/// The decorator must implement the service type and have exactly one public constructor with exactly one
/// parameter of the service type, which receives the decorated instance. Decorators are applied after all
/// registrations, ordered by <see cref="Order"/> and then by fully qualified name. A decorator is not registered as a service.
/// </remarks>
/// <param name="serviceType">The decorated service type.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class DecoratesAttribute(Type serviceType) : Attribute
{
    /// <summary>
    /// Gets the decorated service type.
    /// </summary>
    public Type ServiceType { get; } = serviceType;

    /// <summary>
    /// Gets or sets the order. Lower values are applied first and end up innermost.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets the key of the decorated registration. <see langword="null"/> decorates non-keyed registrations.
    /// </summary>
    public object? Key { get; set; }
}
