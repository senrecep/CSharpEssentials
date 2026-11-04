using System.Reflection;
using CSharpEssentials.DependencyInjection.Benchmarks.Services.S50;
using Microsoft.Extensions.DependencyInjection;
using S500 = CSharpEssentials.DependencyInjection.Benchmarks.Services.S500;

namespace CSharpEssentials.DependencyInjection.Benchmarks;

/// <summary>
/// Binds every variant statically to the generated S50/S500 assemblies, so all variants register the same types.
/// Decoration always starts from <see cref="ManualS50"/> and decorates IService0..IService9 with the same decorator classes.
/// </summary>
public static class DiApp
{
    /// <summary>A new collection with the N services registered by <paramref name="variant"/>.</summary>
    /// <param name="variant">The registration variant.</param>
    /// <param name="n">The service count: 50 or 500.</param>
    /// <returns>The collection.</returns>
    public static IServiceCollection Register(RegistrationVariant variant, int n)
    {
        ServiceCollection services = [];
        switch (variant)
        {
            case RegistrationVariant.Baseline:
                break;
            case RegistrationVariant.Manual:
                RegisterManual(services, n);
                break;
            case RegistrationVariant.CseGenerated:
                RegisterCseGenerated(services, n);
                break;
            case RegistrationVariant.CseReflection:
                RegisterCseReflection(services, n);
                break;
            case RegistrationVariant.Scrutor:
                RegisterScrutor(services, n);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(variant), variant, null);
        }

        return services;
    }

    /// <summary>
    /// Registers with <paramref name="variant"/>, builds the provider and resolves every service once in a scope.
    /// Throws unless every service resolves to its own implementation.
    /// </summary>
    /// <param name="variant">The registration variant.</param>
    /// <param name="n">The service count: 50 or 500.</param>
    /// <returns>The number of resolved services.</returns>
    public static int RegisterBuildResolve(RegistrationVariant variant, int n)
    {
        using ServiceProvider provider = Register(variant, n).BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();
        int[] values = Values(n, scope.ServiceProvider);
        DiSelfCheck.VerifyValues(variant, n, decorated: 0, values);
        return values.Length;
    }

    /// <summary>A new collection with the 50 services registered by hand, then decorated by <paramref name="variant"/>.</summary>
    /// <param name="variant">The decoration variant.</param>
    /// <returns>The collection.</returns>
    public static IServiceCollection RegisterDecorated(DecorationVariant variant)
    {
        ServiceCollection services = [];
        ManualS50.Register(services);
        switch (variant)
        {
            case DecorationVariant.Manual:
                ManualDecorationS50.Apply(services);
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

    /// <summary>The <c>Value()</c> of every <c>IService{i}</c>, resolved from <paramref name="provider"/> in index order.</summary>
    /// <param name="n">The service count: 50 or 500.</param>
    /// <param name="provider">The provider to resolve from.</param>
    /// <returns>The values.</returns>
    public static int[] Values(int n, IServiceProvider provider) => n switch
    {
        50 => Values50(provider),
        500 => Values500(provider),
        _ => throw Unsupported(n),
    };

    /// <summary>The generated service assembly for <paramref name="n"/>.</summary>
    /// <param name="n">The service count: 50 or 500.</param>
    /// <returns>The assembly.</returns>
    public static Assembly ServicesAssembly(int n) => n switch
    {
        50 => ServicesAssembly50(),
        500 => ServicesAssembly500(),
        _ => throw Unsupported(n),
    };

    private static int[] Values50(IServiceProvider provider) => CatalogS50.Values(provider);

    private static int[] Values500(IServiceProvider provider) => S500.CatalogS500.Values(provider);

    private static Assembly ServicesAssembly50() => typeof(IService0).Assembly;

    private static Assembly ServicesAssembly500() => typeof(S500.IService0).Assembly;

    private static void RegisterManual(IServiceCollection services, int n)
    {
        switch (n)
        {
            case 50:
                RegisterManual50(services);
                break;
            case 500:
                RegisterManual500(services);
                break;
            default:
                throw Unsupported(n);
        }
    }

    private static void RegisterManual50(IServiceCollection services) => ManualS50.Register(services);

    private static void RegisterManual500(IServiceCollection services) => S500.ManualS500.Register(services);

    private static void RegisterCseGenerated(IServiceCollection services, int n)
    {
        switch (n)
        {
            case 50:
                RegisterCseGenerated50(services);
                break;
            case 500:
                RegisterCseGenerated500(services);
                break;
            default:
                throw Unsupported(n);
        }
    }

    private static void RegisterCseGenerated50(IServiceCollection services) => services.AddS50Services();

    private static void RegisterCseGenerated500(IServiceCollection services) => services.AddS500Services();

    private static void RegisterCseReflection(IServiceCollection services, int n) =>
        services.AddServicesFromAssemblies(ServicesAssembly(n));

    private static void RegisterScrutor(IServiceCollection services, int n)
    {
        switch (n)
        {
            case 50:
                RegisterScrutor50(services);
                break;
            case 500:
                RegisterScrutor500(services);
                break;
            default:
                throw Unsupported(n);
        }
    }

    // https://github.com/khellang/Scrutor/blob/v7.0.0/README.md#scanning
    // https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/IServiceTypeSelector.cs#L55-L58
    private static void RegisterScrutor50(IServiceCollection services) =>
        services.Scan(scan => scan.FromAssemblyOf<IService0>().AddClasses().AsMatchingInterface().WithScopedLifetime());

    // https://github.com/khellang/Scrutor/blob/v7.0.0/README.md#scanning
    // https://github.com/khellang/Scrutor/blob/v7.0.0/src/Scrutor/IServiceTypeSelector.cs#L55-L58
    private static void RegisterScrutor500(IServiceCollection services) =>
        services.Scan(scan => scan.FromAssemblyOf<S500.IService0>().AddClasses().AsMatchingInterface().WithScopedLifetime());

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

    private static ArgumentOutOfRangeException Unsupported(int n) =>
        new(nameof(n), n, "Generated service libraries exist for N = 50 and 500.");
}
