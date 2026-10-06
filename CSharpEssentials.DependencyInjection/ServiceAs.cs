namespace CSharpEssentials.DependencyInjection;

/// <summary>
/// Selects the service types a registration attribute registers.
/// </summary>
/// <remarks>
/// Interface expansion ignores <see cref="IDisposable"/>, <c>IAsyncDisposable</c>, <see cref="IEquatable{T}"/>,
/// <see cref="IComparable{T}"/> and <see cref="System.Collections.IEnumerable"/>.
/// </remarks>
public enum ServiceAs
{
    /// <summary>
    /// The class registers as itself.
    /// </summary>
    Self,

    /// <summary>
    /// The class registers as itself, and each implemented interface forwards to that registration, so
    /// singleton and scoped instances are shared across the interfaces.
    /// </summary>
    SelfWithInterfaces,

    /// <summary>
    /// Each implemented interface gets its own registration with the class as implementation type.
    /// </summary>
    ImplementedInterfaces,
}
