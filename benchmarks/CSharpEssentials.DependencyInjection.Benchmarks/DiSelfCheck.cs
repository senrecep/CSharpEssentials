using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.DependencyInjection.Benchmarks;

/// <summary>Fails unless every variant registers and resolves the same services, so no row measures a different container.</summary>
public static class DiSelfCheck
{
    /// <summary>What every generated decorator adds to its inner service's <c>Value()</c>.</summary>
    public const int DecoratorOffset = 1000;

    /// <summary>The decorated services: IService0..IService9.</summary>
    public const int DecoratedCount = 10;

    /// <summary>The service counts with generated libraries.</summary>
    public static IReadOnlyList<int> Sizes { get; } = [50, 500];

    /// <summary>Every variant that registers services, without <see cref="RegistrationVariant.Baseline"/>.</summary>
    public static IReadOnlyList<RegistrationVariant> Registering { get; } =
        [RegistrationVariant.Manual, RegistrationVariant.CseGenerated, RegistrationVariant.CseReflection, RegistrationVariant.Scrutor];

    /// <summary>Every decoration variant.</summary>
    public static IReadOnlyList<DecorationVariant> Decorating { get; } =
        [DecorationVariant.Manual, DecorationVariant.CseGenerated, DecorationVariant.CseRuntime, DecorationVariant.Scrutor];

    /// <summary>
    /// The marker CSharpEssentials.DependencyInjection adds once per registered assembly (a singleton instance of an
    /// internal type, never resolved). It is real CSE registration cost, so it stays in the measured unit; the
    /// descriptor comparison skips it and expects exactly one for the CSE variants.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <returns><see langword="true"/> for the CSE marker.</returns>
    public static bool IsCseMarker(ServiceDescriptor descriptor) =>
        descriptor.ServiceType.Assembly == typeof(ServiceRegistration).Assembly;

    /// <summary>Runs every registration, resolve and decoration check for N = 50 and 500.</summary>
    public static void VerifyAll()
    {
        foreach (int n in Sizes)
        {
            VerifyRegistrations(n);
        }

        VerifyDecorations();
    }

    /// <summary>Checks the descriptors and the resolved services of every registration variant.</summary>
    /// <param name="n">The service count.</param>
    public static void VerifyRegistrations(int n)
    {
        VerifyRegistration(RegistrationVariant.Baseline, n, DiApp.Register(RegistrationVariant.Baseline, n));
        foreach (RegistrationVariant variant in Registering)
        {
            IServiceCollection services = DiApp.Register(variant, n);
            VerifyRegistration(variant, n, services);
            using ServiceProvider provider = services.BuildServiceProvider();
            using IServiceScope scope = provider.CreateScope();
            VerifyValues(variant, n, decorated: 0, DiApp.Values(n, scope.ServiceProvider));
            DiApp.RegisterBuildResolve(variant, n);
        }
    }

    /// <summary>Checks that every decoration variant decorates IService0..IService9 exactly once and nothing else.</summary>
    public static void VerifyDecorations()
    {
        foreach (DecorationVariant variant in Decorating)
        {
            using ServiceProvider provider = DiApp.RegisterDecorated(variant).BuildServiceProvider();
            using IServiceScope scope = provider.CreateScope();
            VerifyValues(variant, 50, DecoratedCount, DiApp.Values(50, scope.ServiceProvider));
        }
    }

    /// <summary>
    /// Throws unless <paramref name="services"/> holds exactly one non-keyed scoped (IService{i}, Service{i}) type
    /// registration per service (plus the CSE marker for the CSE variants).
    /// </summary>
    /// <param name="variant">The variant that registered the services.</param>
    /// <param name="n">The service count.</param>
    /// <param name="services">The registered services.</param>
    public static void VerifyRegistration(RegistrationVariant variant, int n, IServiceCollection services)
    {
        int expected = variant == RegistrationVariant.Baseline ? 0 : n;
        int expectedMarkers = variant is RegistrationVariant.CseGenerated or RegistrationVariant.CseReflection ? 1 : 0;
        Assembly assembly = DiApp.ServicesAssembly(n);
        string ns = "CSharpEssentials.DependencyInjection.Benchmarks.Services.S" + n.ToString(CultureInfo.InvariantCulture) + ".";
        HashSet<(Type, Type?, ServiceLifetime)> want =
        [
            .. Enumerable.Range(0, expected).Select(i =>
            {
                string index = i.ToString(CultureInfo.InvariantCulture);
                return (assembly.GetType(ns + "IService" + index, throwOnError: true)!, assembly.GetType(ns + "Service" + index, throwOnError: true), ServiceLifetime.Scoped);
            }),
        ];
        List<ServiceDescriptor> registered = [.. services.Where(static d => !IsCseMarker(d))];
        int markers = services.Count - registered.Count;
        bool typeRegistrations = registered.TrueForAll(static d => !d.IsKeyedService && d.ImplementationType is not null);

        if (registered.Count != expected || markers != expectedMarkers || !typeRegistrations
            || !want.SetEquals(registered.Select(static d => (d.ServiceType, d.ImplementationType, d.Lifetime))))
        {
            throw new InvalidOperationException(
                $"{variant} with N={n} registered {registered.Count} descriptors and {markers} CSE markers (type registrations only: {typeRegistrations}) "
                + $"instead of {expected} scoped type registrations and {expectedMarkers} markers: "
                + string.Join(", ", registered.Take(5)));
        }
    }

    /// <summary>
    /// Throws unless <paramref name="values"/> is 0..n-1, with <see cref="DecoratorOffset"/> added to the first
    /// <paramref name="decorated"/> services.
    /// </summary>
    /// <typeparam name="TVariant">The variant enum.</typeparam>
    /// <param name="variant">The variant that built the provider.</param>
    /// <param name="n">The service count.</param>
    /// <param name="decorated">How many leading services are decorated.</param>
    /// <param name="values">The resolved values.</param>
    public static void VerifyValues<TVariant>(TVariant variant, int n, int decorated, int[] values)
        where TVariant : struct, Enum
    {
        bool ok = values.Length == n;
        for (int i = 0; ok && i < n; i++)
        {
            ok = values[i] == (i < decorated ? i + DecoratorOffset : i);
        }

        if (!ok)
        {
            throw new InvalidOperationException(
                $"{variant} with N={n} resolved values [{string.Join(", ", values.Take(12))}] instead of 0..{n - 1} with the first {decorated} decorated.");
        }
    }
}
