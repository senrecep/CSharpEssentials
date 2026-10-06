using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CSharpEssentials.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Registers attribute-marked services by scanning assemblies at runtime. Prefer the generated
/// <c>Add{Asm}Services</c> method, which needs no reflection and is trim and AOT safe.
/// </summary>
public static class ServiceCollectionRegistrationExtensions
{
    /// <summary>
    /// Registers every class marked with a registration attribute in <paramref name="assemblies"/>, then applies
    /// every <see cref="DecoratesAttribute"/> decorator, ordered by <see cref="DecoratesAttribute.Order"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">The assemblies to scan.</param>
    /// <returns><paramref name="services"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// A marked type is invalid, or the types of an assembly cannot all be loaded.
    /// </exception>
    [RequiresUnreferencedCode(AssemblyServiceScanner.UnreferencedCodeMessage)]
    [RequiresDynamicCode(AssemblyServiceScanner.DynamicCodeMessage)]
    public static IServiceCollection AddServicesFromAssemblies(this IServiceCollection services, params Assembly[] assemblies) =>
        services.AddServicesFromAssemblies(logger: null, assemblies);

    /// <summary>
    /// Registers every class marked with a registration attribute in <paramref name="assemblies"/>, then applies
    /// every <see cref="DecoratesAttribute"/> decorator, ordered by <see cref="DecoratesAttribute.Order"/>.
    /// Types of an assembly that cannot be loaded are skipped and logged as a warning; duplicate registrations are
    /// logged at <see cref="LogLevel.Debug"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="logger">The logger for skipped types and duplicate registrations, or <see langword="null"/> to throw on unloadable types.</param>
    /// <param name="assemblies">The assemblies to scan.</param>
    /// <returns><paramref name="services"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// A marked type is invalid, or <paramref name="logger"/> is <see langword="null"/> and the types of an assembly cannot all be loaded.
    /// </exception>
    [RequiresUnreferencedCode(AssemblyServiceScanner.UnreferencedCodeMessage)]
    [RequiresDynamicCode(AssemblyServiceScanner.DynamicCodeMessage)]
    public static IServiceCollection AddServicesFromAssemblies(this IServiceCollection services, ILogger? logger, params Assembly[] assemblies)
    {
        Guard.NotNull(services);
        Guard.NotNull(assemblies);
        foreach (Assembly assembly in assemblies)
        {
            Guard.NotNull(assembly);
        }

        AssemblyServiceScanner.Scan(services, logger, assemblies);
        return services;
    }
}
