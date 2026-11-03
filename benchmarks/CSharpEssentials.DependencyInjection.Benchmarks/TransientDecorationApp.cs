using CSharpEssentials.DependencyInjection.Benchmarks.Services.S50;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CSharpEssentials.DependencyInjection.Benchmarks;

/// <summary>
/// Registers S50's IService0..IService9 as transient by hand, then decorates all ten with the S50.Decorators classes,
/// once per <see cref="DecorationVariant"/>. Every variant starts from the same registrations and uses the same decorators.
/// </summary>
public static class TransientDecorationApp
{
    /// <summary>The decorated service types, IService0..IService9.</summary>
    public static IReadOnlyList<Type> ServiceTypes { get; } =
    [
        typeof(IService0),
        typeof(IService1),
        typeof(IService2),
        typeof(IService3),
        typeof(IService4),
        typeof(IService5),
        typeof(IService6),
        typeof(IService7),
        typeof(IService8),
        typeof(IService9),
    ];

    /// <summary>A new collection with the ten transient services, decorated by <paramref name="variant"/>.</summary>
    /// <param name="variant">The decoration variant.</param>
    /// <returns>The collection.</returns>
    public static IServiceCollection Register(DecorationVariant variant)
    {
        ServiceCollection services = [];
        RegisterTransient(services);
        switch (variant)
        {
            case DecorationVariant.Manual:
                DecorateManual(services);
                break;
            case DecorationVariant.CseGenerated:
                services.AddS50DecoratorsServices();
                break;
            case DecorationVariant.CseRuntime:
                DecorateCseRuntime(services);
                break;
            case DecorationVariant.Scrutor:
                DecorateScrutor(services);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(variant), variant, null);
        }

        return services;
    }

    /// <summary>The <c>Value()</c> of IService0..IService9, resolved from <paramref name="provider"/> in index order.</summary>
    /// <param name="provider">The provider to resolve from.</param>
    /// <returns>The values.</returns>
    public static int[] Values(IServiceProvider provider) =>
    [
        provider.GetRequiredService<IService0>().Value(),
        provider.GetRequiredService<IService1>().Value(),
        provider.GetRequiredService<IService2>().Value(),
        provider.GetRequiredService<IService3>().Value(),
        provider.GetRequiredService<IService4>().Value(),
        provider.GetRequiredService<IService5>().Value(),
        provider.GetRequiredService<IService6>().Value(),
        provider.GetRequiredService<IService7>().Value(),
        provider.GetRequiredService<IService8>().Value(),
        provider.GetRequiredService<IService9>().Value(),
    ];

    // https://learn.microsoft.com/dotnet/core/extensions/dependency-injection/service-registration
    private static void RegisterTransient(IServiceCollection services)
    {
        services.AddTransient<IService0, Service0>();
        services.AddTransient<IService1, Service1>();
        services.AddTransient<IService2, Service2>();
        services.AddTransient<IService3, Service3>();
        services.AddTransient<IService4, Service4>();
        services.AddTransient<IService5, Service5>();
        services.AddTransient<IService6, Service6>();
        services.AddTransient<IService7, Service7>();
        services.AddTransient<IService8, Service8>();
        services.AddTransient<IService9, Service9>();
    }

    // https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.extensions.servicecollectiondescriptorextensions.replace
    private static void DecorateManual(IServiceCollection services)
    {
        services.Replace(ServiceDescriptor.Transient<IService0>(sp => new Service0Decorator(new Service0())));
        services.Replace(ServiceDescriptor.Transient<IService1>(sp => new Service1Decorator(new Service1())));
        services.Replace(ServiceDescriptor.Transient<IService2>(sp => new Service2Decorator(new Service2())));
        services.Replace(ServiceDescriptor.Transient<IService3>(sp => new Service3Decorator(new Service3())));
        services.Replace(ServiceDescriptor.Transient<IService4>(sp => new Service4Decorator(new Service4())));
        services.Replace(ServiceDescriptor.Transient<IService5>(sp => new Service5Decorator(new Service5(sp.GetRequiredService<IService4>()))));
        services.Replace(ServiceDescriptor.Transient<IService6>(sp => new Service6Decorator(new Service6())));
        services.Replace(ServiceDescriptor.Transient<IService7>(sp => new Service7Decorator(new Service7())));
        services.Replace(ServiceDescriptor.Transient<IService8>(sp => new Service8Decorator(new Service8())));
        services.Replace(ServiceDescriptor.Transient<IService9>(sp => new Service9Decorator(new Service9())));
    }

    private static void DecorateCseRuntime(IServiceCollection services)
    {
        ServiceCollectionDecorationExtensions.Decorate<IService0, Service0Decorator>(services);
        ServiceCollectionDecorationExtensions.Decorate<IService1, Service1Decorator>(services);
        ServiceCollectionDecorationExtensions.Decorate<IService2, Service2Decorator>(services);
        ServiceCollectionDecorationExtensions.Decorate<IService3, Service3Decorator>(services);
        ServiceCollectionDecorationExtensions.Decorate<IService4, Service4Decorator>(services);
        ServiceCollectionDecorationExtensions.Decorate<IService5, Service5Decorator>(services);
        ServiceCollectionDecorationExtensions.Decorate<IService6, Service6Decorator>(services);
        ServiceCollectionDecorationExtensions.Decorate<IService7, Service7Decorator>(services);
        ServiceCollectionDecorationExtensions.Decorate<IService8, Service8Decorator>(services);
        ServiceCollectionDecorationExtensions.Decorate<IService9, Service9Decorator>(services);
    }

    // https://github.com/khellang/Scrutor/blob/v7.0.0/README.md#decoration
    private static void DecorateScrutor(IServiceCollection services)
    {
        ServiceCollectionExtensions.Decorate<IService0, Service0Decorator>(services);
        ServiceCollectionExtensions.Decorate<IService1, Service1Decorator>(services);
        ServiceCollectionExtensions.Decorate<IService2, Service2Decorator>(services);
        ServiceCollectionExtensions.Decorate<IService3, Service3Decorator>(services);
        ServiceCollectionExtensions.Decorate<IService4, Service4Decorator>(services);
        ServiceCollectionExtensions.Decorate<IService5, Service5Decorator>(services);
        ServiceCollectionExtensions.Decorate<IService6, Service6Decorator>(services);
        ServiceCollectionExtensions.Decorate<IService7, Service7Decorator>(services);
        ServiceCollectionExtensions.Decorate<IService8, Service8Decorator>(services);
        ServiceCollectionExtensions.Decorate<IService9, Service9Decorator>(services);
    }
}
