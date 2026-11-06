using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using T2000 = CSharpEssentials.DependencyInjection.Benchmarks.Services.T2000;
using T50 = CSharpEssentials.DependencyInjection.Benchmarks.Services.T50;
using T500 = CSharpEssentials.DependencyInjection.Benchmarks.Services.T500;

namespace CSharpEssentials.DependencyInjection.Benchmarks;

/// <summary>
/// Binds every <see cref="TryAddVariant"/> statically to the generated T50/T500/T2000 assemblies, whose classes carry
/// <c>[RegisterScoped(Strategy = RegistrationStrategy.TryAdd)]</c>, so all variants register the same types.
/// </summary>
public static class TryAddApp
{
    /// <summary>The service counts with generated TryAdd libraries.</summary>
    public static IReadOnlyList<int> Sizes { get; } = [50, 500, 2000];

    /// <summary>A new collection with the N services registered by <paramref name="variant"/>.</summary>
    /// <param name="variant">The registration variant.</param>
    /// <param name="n">The service count: 50, 500 or 2000.</param>
    /// <returns>The collection.</returns>
    public static IServiceCollection Register(TryAddVariant variant, int n) => Apply(new ServiceCollection(), variant, n);

    /// <summary>Registers the N services into <paramref name="services"/> with <paramref name="variant"/>.</summary>
    /// <param name="services">The collection, empty or with registrations TryAdd must keep.</param>
    /// <param name="variant">The registration variant.</param>
    /// <param name="n">The service count: 50, 500 or 2000.</param>
    /// <returns><paramref name="services"/>.</returns>
    public static IServiceCollection Apply(IServiceCollection services, TryAddVariant variant, int n)
    {
        switch (variant)
        {
            case TryAddVariant.ManualTryAdd:
                ManualTryAdd(services, n);
                break;
            case TryAddVariant.CseGenerated:
                CseGenerated(services, n);
                break;
            case TryAddVariant.CseReflection:
                services.AddServicesFromAssemblies(ServicesAssembly(n));
                break;
            case TryAddVariant.Scrutor:
                Scrutor(services, n);
                break;
            case TryAddVariant.ManualAdd:
                ManualAdd(services, n);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(variant), variant, null);
        }

        return services;
    }

    /// <summary>The <c>Value()</c> of every <c>IService{i}</c> of T{N}, resolved from <paramref name="provider"/> in index order.</summary>
    /// <param name="n">The service count: 50, 500 or 2000.</param>
    /// <param name="provider">The provider to resolve from.</param>
    /// <returns>The values.</returns>
    public static int[] Values(int n, IServiceProvider provider) => n switch
    {
        50 => T50.CatalogT50.Values(provider),
        500 => T500.CatalogT500.Values(provider),
        2000 => T2000.CatalogT2000.Values(provider),
        _ => throw Unsupported(n),
    };

    /// <summary>The generated T{N} assembly.</summary>
    /// <param name="n">The service count: 50, 500 or 2000.</param>
    /// <returns>The assembly.</returns>
    public static Assembly ServicesAssembly(int n) => n switch
    {
        50 => typeof(T50.IService0).Assembly,
        500 => typeof(T500.IService0).Assembly,
        2000 => typeof(T2000.IService0).Assembly,
        _ => throw Unsupported(n),
    };

    /// <summary>The <c>IService0</c> and <c>Service0</c> types of T{N}.</summary>
    /// <param name="n">The service count: 50, 500 or 2000.</param>
    /// <returns>The service and implementation type.</returns>
    public static (Type Service, Type Implementation) First(int n) => n switch
    {
        50 => (typeof(T50.IService0), typeof(T50.Service0)),
        500 => (typeof(T500.IService0), typeof(T500.Service0)),
        2000 => (typeof(T2000.IService0), typeof(T2000.Service0)),
        _ => throw Unsupported(n),
    };

    // https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.extensions.servicecollectiondescriptorextensions.tryaddscoped
    private static void ManualTryAdd(IServiceCollection services, int n)
    {
        switch (n)
        {
            case 50:
                T50.ManualTryAddT50.Register(services);
                break;
            case 500:
                T500.ManualTryAddT500.Register(services);
                break;
            case 2000:
                T2000.ManualTryAddT2000.Register(services);
                break;
            default:
                throw Unsupported(n);
        }
    }

    private static void CseGenerated(IServiceCollection services, int n)
    {
        switch (n)
        {
            case 50:
                services.AddT50Services();
                break;
            case 500:
                services.AddT500Services();
                break;
            case 2000:
                services.AddT2000Services();
                break;
            default:
                throw Unsupported(n);
        }
    }

    // https://github.com/khellang/Scrutor/blob/v7.0.0/README.md#scanning
    // https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/IServiceTypeSelector.cs#L79
    // https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/RegistrationStrategy.cs#L47-L50
    private static void Scrutor(IServiceCollection services, int n)
    {
        switch (n)
        {
            case 50:
                services.Scan(scan => scan.FromAssemblyOf<T50.IService0>().AddClasses()
                    .UsingRegistrationStrategy(global::Scrutor.RegistrationStrategy.Skip).AsMatchingInterface().WithScopedLifetime());
                break;
            case 500:
                services.Scan(scan => scan.FromAssemblyOf<T500.IService0>().AddClasses()
                    .UsingRegistrationStrategy(global::Scrutor.RegistrationStrategy.Skip).AsMatchingInterface().WithScopedLifetime());
                break;
            case 2000:
                services.Scan(scan => scan.FromAssemblyOf<T2000.IService0>().AddClasses()
                    .UsingRegistrationStrategy(global::Scrutor.RegistrationStrategy.Skip).AsMatchingInterface().WithScopedLifetime());
                break;
            default:
                throw Unsupported(n);
        }
    }

    // https://learn.microsoft.com/dotnet/core/extensions/dependency-injection/service-registration
    private static void ManualAdd(IServiceCollection services, int n)
    {
        switch (n)
        {
            case 50:
                T50.ManualAddT50.Register(services);
                break;
            case 500:
                T500.ManualAddT500.Register(services);
                break;
            case 2000:
                T2000.ManualAddT2000.Register(services);
                break;
            default:
                throw Unsupported(n);
        }
    }

    private static ArgumentOutOfRangeException Unsupported(int n) =>
        new(nameof(n), n, "Generated TryAdd service libraries exist for N = 50, 500 and 2000.");
}
