namespace CSharpEssentials.DependencyInjection;

/// <summary>
/// Defines how a registration treats existing registrations. Every strategy is key-aware: a registration matches
/// when it has the same service type and an equal service key (<see langword="null"/> matches only <see langword="null"/>).
/// </summary>
public enum RegistrationStrategy
{
    /// <summary>
    /// Always appends the registration.
    /// </summary>
    Add,

    /// <summary>
    /// Appends the registration only if no registration matches the service type and key.
    /// </summary>
    TryAdd,

    /// <summary>
    /// Appends the registration only if no registration matches the service type, key and implementation.
    /// </summary>
    TryAddEnumerable,

    /// <summary>
    /// Removes every registration that matches the service type and key, then appends the registration.
    /// </summary>
    Replace,

    /// <summary>
    /// Throws <see cref="InvalidOperationException"/> if a registration matches the service type and key.
    /// </summary>
    Throw,
}
