using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.DependencyInjection.Benchmarks;

/// <summary>
/// Fails unless every <see cref="TryAddBenchmarks"/> and <see cref="TransientDecoratorBenchmarks"/> variant registers and
/// resolves the same services, so no row measures a different container.
/// </summary>
public static class DiExtSelfCheck
{
    /// <summary>The variants that must keep an existing registration: every one except <see cref="TryAddVariant.ManualAdd"/>.</summary>
    public static IReadOnlyList<TryAddVariant> TryAdding { get; } =
        [TryAddVariant.ManualTryAdd, TryAddVariant.CseGenerated, TryAddVariant.CseReflection, TryAddVariant.Scrutor];

    /// <summary>Runs the setup and cleanup of every new class for every parameter value.</summary>
    public static void VerifyAll()
    {
        foreach (int n in TryAddApp.Sizes)
        {
            new TryAddBenchmarks { N = n }.Setup();
        }

        TransientDecoratorBenchmarks transient = new();
        transient.Setup();
        transient.Cleanup();
    }

    /// <summary>
    /// Checks the descriptors and resolved values of every <see cref="TryAddVariant"/> for <paramref name="n"/>, and that
    /// every TryAdd variant keeps an <c>IService0</c> registration added before it.
    /// </summary>
    /// <param name="n">The service count.</param>
    public static void VerifyTryAdd(int n)
    {
        foreach (TryAddVariant variant in Enum.GetValues<TryAddVariant>())
        {
            IServiceCollection services = TryAddApp.Register(variant, n);
            VerifyTryAddRegistration(variant, n, services);
            using ServiceProvider provider = services.BuildServiceProvider();
            using IServiceScope scope = provider.CreateScope();
            DiSelfCheck.VerifyValues(variant, n, decorated: 0, TryAddApp.Values(n, scope.ServiceProvider));
        }

        (Type service, Type implementation) = TryAddApp.First(n);
        foreach (TryAddVariant variant in TryAdding)
        {
            ServiceCollection services = [];
            services.AddScoped(service, implementation);
            TryAddApp.Apply(services, variant, n);
            int registered = services.Count(d => d.ServiceType == service);
            if (registered != 1 || services.Count(d => !DiSelfCheck.IsCseMarker(d)) != n)
            {
                throw new InvalidOperationException(
                    $"{variant} with N={n} over an existing IService0 left {registered} IService0 descriptors and {services.Count} in total, expected 1 and {n} (plus the CSE marker).");
            }
        }
    }

    /// <summary>
    /// Builds the provider of <paramref name="variant"/> from <see cref="TransientDecorationApp.Register"/> and throws unless
    /// IService0..IService9 resolve to 1000 + i, each service's last non-keyed descriptor is transient, and two resolves
    /// of IService0 return different instances.
    /// </summary>
    /// <param name="variant">The decoration variant.</param>
    /// <returns>The root provider.</returns>
    public static ServiceProvider BuildTransientDecorated(DecorationVariant variant)
    {
        IServiceCollection services = TransientDecorationApp.Register(variant);
        foreach (Type type in TransientDecorationApp.ServiceTypes)
        {
            ServiceDescriptor? last = services.LastOrDefault(d => d.ServiceType == type && !d.IsKeyedService);
            if (last?.Lifetime != ServiceLifetime.Transient)
            {
                throw new InvalidOperationException($"{variant} left {type.Name} as {last?.Lifetime.ToString() ?? "missing"}, expected Transient.");
            }
        }

        ServiceProvider provider = services.BuildServiceProvider();
        int[] values = TransientDecorationApp.Values(provider);
        bool ok = values.Length == DiSelfCheck.DecoratedCount;
        for (int i = 0; ok && i < values.Length; i++)
        {
            ok = values[i] == i + DiSelfCheck.DecoratorOffset;
        }

        object first = provider.GetRequiredService(TransientDecorationApp.ServiceTypes[0]);
        object second = provider.GetRequiredService(TransientDecorationApp.ServiceTypes[0]);
        if (!ok || ReferenceEquals(first, second))
        {
            provider.Dispose();
            throw new InvalidOperationException(
                $"{variant} resolved [{string.Join(", ", values)}] (same instance twice: {ReferenceEquals(first, second)}), expected 1000..1009 and new instances.");
        }

        return provider;
    }

    private static void VerifyTryAddRegistration(TryAddVariant variant, int n, IServiceCollection services)
    {
        int expectedMarkers = variant is TryAddVariant.CseGenerated or TryAddVariant.CseReflection ? 1 : 0;
        Assembly assembly = TryAddApp.ServicesAssembly(n);
        string ns = "CSharpEssentials.DependencyInjection.Benchmarks.Services.T" + n.ToString(CultureInfo.InvariantCulture) + ".";
        HashSet<(Type, Type?, ServiceLifetime)> want =
        [
            .. Enumerable.Range(0, n).Select(i =>
            {
                string index = i.ToString(CultureInfo.InvariantCulture);
                return (assembly.GetType(ns + "IService" + index, throwOnError: true)!, assembly.GetType(ns + "Service" + index, throwOnError: true), ServiceLifetime.Scoped);
            }),
        ];
        List<ServiceDescriptor> registered = [.. services.Where(static d => !DiSelfCheck.IsCseMarker(d))];
        int markers = services.Count - registered.Count;
        bool typeRegistrations = registered.TrueForAll(static d => !d.IsKeyedService && d.ImplementationType is not null);

        if (registered.Count != n || markers != expectedMarkers || !typeRegistrations
            || !want.SetEquals(registered.Select(static d => (d.ServiceType, d.ImplementationType, d.Lifetime))))
        {
            throw new InvalidOperationException(
                $"{variant} with N={n} registered {registered.Count} descriptors and {markers} CSE markers (type registrations only: {typeRegistrations}) "
                + $"instead of {n} scoped type registrations and {expectedMarkers} markers: "
                + string.Join(", ", registered.Take(5)));
        }
    }
}
