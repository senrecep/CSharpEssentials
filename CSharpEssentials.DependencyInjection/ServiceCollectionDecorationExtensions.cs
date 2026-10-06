using System.Diagnostics.CodeAnalysis;
using CSharpEssentials.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Decorates registered services. Each matched registration is moved to a hidden registration under a private key,
/// and the decorator takes its place with the same service type, key and lifetime.
/// </summary>
public static class ServiceCollectionDecorationExtensions
{
    /// <summary>
    /// Decorates every registration of <typeparamref name="TService"/> with <paramref name="serviceKey"/> using
    /// <typeparamref name="TDecorator"/>, constructed through its single public constructor.
    /// </summary>
    /// <typeparam name="TService">The decorated service type.</typeparam>
    /// <typeparam name="TDecorator">The decorator type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="serviceKey">The service key of the decorated registrations, or <see langword="null"/> for non-keyed registrations.</param>
    /// <returns><paramref name="services"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// No registration matches, the decorator cannot be constructed, or an original implementation injects <see cref="ServiceKeyAttribute"/>.
    /// </exception>
    public static IServiceCollection Decorate<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TDecorator>(
        this IServiceCollection services,
        object? serviceKey = null)
        where TService : class
        where TDecorator : class, TService
    {
        if (!services.TryDecorate<TService, TDecorator>(serviceKey))
        {
            throw ServiceDecoration.NothingToDecorate(typeof(TService), serviceKey, typeof(TDecorator));
        }

        return services;
    }

    /// <summary>
    /// Decorates every registration of <typeparamref name="TService"/> with <paramref name="serviceKey"/> using
    /// <typeparamref name="TDecorator"/>, when at least one registration exists.
    /// </summary>
    /// <typeparam name="TService">The decorated service type.</typeparam>
    /// <typeparam name="TDecorator">The decorator type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="serviceKey">The service key of the decorated registrations, or <see langword="null"/> for non-keyed registrations.</param>
    /// <returns><see langword="true"/> when at least one registration was decorated; otherwise <see langword="false"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// The decorator cannot be constructed, or an original implementation injects <see cref="ServiceKeyAttribute"/>.
    /// </exception>
    public static bool TryDecorate<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TDecorator>(
        this IServiceCollection services,
        object? serviceKey = null)
        where TService : class
        where TDecorator : class, TService
    {
        Guard.NotNull(services);

        Func<object, IServiceProvider, object> decorator = DecoratorActivator.Create(typeof(TDecorator), typeof(TService), serviceKey);
        return ServiceDecoration.TryDecorate(services, typeof(TService), serviceKey, decorator);
    }

    /// <summary>
    /// Decorates every registration of <typeparamref name="TService"/> with <paramref name="serviceKey"/> using
    /// <paramref name="decorator"/>.
    /// </summary>
    /// <typeparam name="TService">The decorated service type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="decorator">Creates the decorator from the inner service and the service provider.</param>
    /// <param name="serviceKey">The service key of the decorated registrations, or <see langword="null"/> for non-keyed registrations.</param>
    /// <returns><paramref name="services"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// No registration matches, or an original implementation injects <see cref="ServiceKeyAttribute"/>.
    /// </exception>
    public static IServiceCollection Decorate<TService>(
        this IServiceCollection services,
        Func<TService, IServiceProvider, TService> decorator,
        object? serviceKey = null)
        where TService : class
    {
        Guard.NotNull(services);
        Guard.NotNull(decorator);

        if (!ServiceDecoration.TryDecorate(services, typeof(TService), serviceKey, (inner, provider) => decorator((TService)inner, provider)))
        {
            throw ServiceDecoration.NothingToDecorate(typeof(TService), serviceKey, decoratorType: null);
        }

        return services;
    }

    /// <summary>
    /// Decorates every closed registration constructed from the open generic <paramref name="serviceType"/> with the
    /// open generic <paramref name="decoratorType"/>, closed over the same type arguments. Each registration keeps its key.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="serviceType">The open generic service type, such as <c>typeof(IRepository&lt;&gt;)</c>.</param>
    /// <param name="decoratorType">The open generic decorator type, such as <c>typeof(CachedRepository&lt;&gt;)</c>.</param>
    /// <returns><paramref name="services"/>.</returns>
    /// <exception cref="ArgumentException">A type is not an open generic type definition.</exception>
    /// <exception cref="InvalidOperationException">
    /// Open-generic registrations exist, no closed registration matches, or a decorator cannot be constructed.
    /// </exception>
    [RequiresDynamicCode("Closes the open-generic decorator for each matching closed registration.")]
    [RequiresUnreferencedCode("Closes the open-generic decorator for each matching closed registration; its constructors may be trimmed.")]
    public static IServiceCollection Decorate(this IServiceCollection services, Type serviceType, Type decoratorType)
    {
        Guard.NotNull(services);
        Guard.NotNull(serviceType);
        Guard.NotNull(decoratorType);

        ServiceDecoration.DecorateOpenGeneric(services, serviceType, decoratorType, static _ => true);
        return services;
    }
}
