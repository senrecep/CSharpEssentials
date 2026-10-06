#if NET7_0_OR_GREATER
namespace CSharpEssentials.DependencyInjection;

/// <summary>
/// Applies the annotated class as a decorator of <typeparamref name="TService"/>.
/// </summary>
/// <typeparam name="TService">The decorated service type.</typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class DecoratesAttribute<TService> : Attribute
{
    /// <summary>
    /// Gets the decorated service type.
    /// </summary>
    public Type ServiceType => typeof(TService);

    /// <summary>
    /// Gets or sets the order. Lower values are applied first and end up innermost.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets the key of the decorated registration. <see langword="null"/> decorates non-keyed registrations.
    /// </summary>
    public object? Key { get; set; }
}
#endif
