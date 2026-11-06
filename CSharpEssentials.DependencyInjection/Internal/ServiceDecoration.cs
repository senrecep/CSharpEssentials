using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.DependencyInjection;

internal static class ServiceDecoration
{
    private static readonly ConditionalWeakTable<Type, object> ServiceKeyInjection = [];

    private static readonly object InjectsKey = new();

    private static readonly object InjectsNoKey = new();

    public static bool TryDecorate(
        IServiceCollection services,
        Type serviceType,
        object? serviceKey,
        Func<object, IServiceProvider, object> decorator,
        Type[] verifiedImplementations)
    {
        int count = services.Count;
        int first = -1;
        List<int>? rest = null;
        for (int index = 0; index < count; index++)
        {
            ServiceDescriptor descriptor = services[index];
            if (descriptor.Matches(serviceType, serviceKey))
            {
                EnsureDecoratable(descriptor, verifiedImplementations);
                if (first < 0)
                {
                    first = index;
                }
                else
                {
                    (rest ??= []).Add(index);
                }
            }
        }

        if (first < 0)
        {
            return false;
        }

        Replace(services, first, decorator);
        if (rest is not null)
        {
            foreach (int index in rest)
            {
                Replace(services, index, decorator);
            }
        }

        return true;
    }

    public static void DecorateAll(IServiceCollection services, DecoratorRegistration[] decorators)
    {
        int count = services.Count;
        int[] matchedIndexes = ArrayPool<int>.Shared.Rent(count);
        int[] matchedOwners = ArrayPool<int>.Shared.Rent(count);
        try
        {
            ulong serviceTypes = 0;
            foreach (DecoratorRegistration decorator in decorators)
            {
                serviceTypes |= HashBit(decorator.ServiceType);
            }

            int matched = 0;
            for (int index = 0; index < count; index++)
            {
                ServiceDescriptor descriptor = services[index];
                int owner = (serviceTypes & HashBit(descriptor.ServiceType)) == 0 ? -1 : FindOwner(descriptor, decorators);
                if (owner >= 0)
                {
                    matchedIndexes[matched] = index;
                    matchedOwners[matched] = owner;
                    matched++;
                }
            }

            for (int position = 0; position < decorators.Length; position++)
            {
                if (FindOwner(decorators[position], decorators, position) == position)
                {
                    Validate(services, decorators[position], position, matchedIndexes, matchedOwners, matched);
                }
            }

            for (int position = 0; position < decorators.Length; position++)
            {
                int owner = FindOwner(decorators[position], decorators, position);
                for (int match = 0; match < matched; match++)
                {
                    if (matchedOwners[match] == owner)
                    {
                        Replace(services, matchedIndexes[match], decorators[position].Decorator);
                    }
                }
            }
        }
        finally
        {
            ArrayPool<int>.Shared.Return(matchedIndexes);
            ArrayPool<int>.Shared.Return(matchedOwners);
        }
    }

    private static void Validate(
        IServiceCollection services,
        DecoratorRegistration decorator,
        int owner,
        int[] matchedIndexes,
        int[] matchedOwners,
        int matched)
    {
        bool found = false;
        for (int match = 0; match < matched; match++)
        {
            if (matchedOwners[match] == owner)
            {
                EnsureDecoratable(services[matchedIndexes[match]], decorator.VerifiedImplementations);
                found = true;
            }
        }

        if (!found)
        {
            throw NothingToDecorate(decorator.ServiceType, decorator.ServiceKey, decoratorType: null);
        }
    }

    [RequiresDynamicCode("Closes the open-generic decorator for each matching closed registration.")]
    [RequiresUnreferencedCode("Closes the open-generic decorator for each matching closed registration; its constructors may be trimmed.")]
    public static void DecorateOpenGeneric(
        IServiceCollection services,
        Type serviceType,
        Type decoratorType,
        Func<object?, bool> keyFilter,
        string? missingConstructorHint = null)
    {
        if (!serviceType.IsGenericTypeDefinition || !decoratorType.IsGenericTypeDefinition)
        {
            throw new ArgumentException(
                $"Both '{serviceType.FullName}' and '{decoratorType.FullName}' must be open generic type definitions.",
                nameof(decoratorType));
        }

        string[] openRegistrations = [.. services
            .Where(descriptor => descriptor.ServiceType == serviceType)
            .Select(descriptor => descriptor.DescribeImplementation())];
        if (openRegistrations.Length > 0)
        {
            throw new InvalidOperationException(
                $"Open-generic registrations of '{serviceType.FullName}' cannot be decorated by Microsoft.Extensions.DependencyInjection: " +
                $"{string.Join(", ", openRegistrations)}. Register closed service types to decorate them.");
        }

        List<(int Index, Func<object, IServiceProvider, object> Decorator)> plan = [];
        for (int index = 0; index < services.Count; index++)
        {
            ServiceDescriptor descriptor = services[index];
            if (descriptor.ServiceType.IsConstructedGenericType &&
                descriptor.ServiceType.GetGenericTypeDefinition() == serviceType &&
                keyFilter(descriptor.ServiceKey))
            {
                EnsureDecoratable(descriptor, Type.EmptyTypes);
                Type closedDecorator = decoratorType.MakeGenericType(descriptor.ServiceType.GenericTypeArguments);
                plan.Add((index, DecoratorActivator.Create(closedDecorator, descriptor.ServiceType, descriptor.ServiceKey, missingConstructorHint)));
            }
        }

        if (plan.Count == 0)
        {
            throw new InvalidOperationException(
                $"No closed registration of '{serviceType.FullName}' was found to decorate with '{decoratorType.FullName}'.");
        }

        foreach ((int index, Func<object, IServiceProvider, object> decorator) in plan)
        {
            Replace(services, index, decorator);
        }
    }

    public static InvalidOperationException NothingToDecorate(Type serviceType, object? serviceKey, Type? decoratorType) =>
        new(
            $"No registration of service '{serviceType.FullName}' with key '{ServiceDescriptorExtensions.DescribeKey(serviceKey)}' " +
            $"was found to decorate{(decoratorType is null ? string.Empty : $" with '{decoratorType.FullName}'")}. " +
            "Register the service before applying its decorators.");

    private static void EnsureDecoratable(ServiceDescriptor descriptor, Type[] verifiedImplementations)
    {
        Type? implementationType = descriptor.IsKeyedService ? descriptor.KeyedImplementationType : descriptor.ImplementationType;
        if (implementationType is null || Array.IndexOf(verifiedImplementations, implementationType) >= 0)
        {
            return;
        }

        if (!ServiceKeyInjection.TryGetValue(implementationType, out object? injection))
        {
            injection = InjectsServiceKey(implementationType) ? InjectsKey : InjectsNoKey;
            ServiceKeyInjection.AddOrUpdate(implementationType, injection);
        }

        if (injection == InjectsKey)
        {
            throw new InvalidOperationException(
                $"'{implementationType.FullName}' injects [ServiceKey] and cannot be decorated, because its hidden inner registration uses a private key.");
        }
    }

    private static ulong HashBit(Type type) => 1UL << (type.GetHashCode() & 63);

    private static int FindOwner(ServiceDescriptor descriptor, DecoratorRegistration[] decorators)
    {
        for (int position = 0; position < decorators.Length; position++)
        {
            if (descriptor.Matches(decorators[position].ServiceType, decorators[position].ServiceKey))
            {
                return position;
            }
        }

        return -1;
    }

    private static int FindOwner(DecoratorRegistration decorator, DecoratorRegistration[] decorators, int position)
    {
        for (int earlier = 0; earlier < position; earlier++)
        {
            if (decorators[earlier].ServiceType == decorator.ServiceType && Equals(decorators[earlier].ServiceKey, decorator.ServiceKey))
            {
                return earlier;
            }
        }

        return position;
    }

    private static bool InjectsServiceKey([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type)
    {
        foreach (ConstructorInfo constructor in type.GetConstructors())
        {
            foreach (ParameterInfo parameter in constructor.GetParameters())
            {
                if (parameter.IsDefined(typeof(ServiceKeyAttribute), inherit: false))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static void Replace(IServiceCollection services, int index, Func<object, IServiceProvider, object> decorator)
    {
        ServiceDescriptor original = services[index];
        DecoratedServiceKey innerKey = new(original.ServiceType, decorator);
        ServiceDescriptor inner = CreateInner(original, innerKey);
        innerKey.InnerServiceType = inner.ServiceType;

        services[index] = original.ServiceKey is null
            ? new ServiceDescriptor(original.ServiceType, innerKey.Decorate, original.Lifetime)
            : new ServiceDescriptor(original.ServiceType, original.ServiceKey, (provider, _) => innerKey.Decorate(provider), original.Lifetime);
        services.Add(inner);
    }

    /// <remarks>
    /// The inner registration is never keyed under <see cref="object"/> or the decorated service type, so
    /// <c>GetKeyedServices&lt;object&gt;(KeyedService.AnyKey)</c> and <c>GetKeyedServices&lt;TService&gt;(KeyedService.AnyKey)</c>
    /// do not return it. Microsoft.Extensions.DependencyInjection requires the implementation or instance to be assignable to the
    /// service type, so those originals are keyed under their own concrete type; factories are keyed under <see cref="DecoratedService"/>.
    /// A scoped original gets a transient inner: the decorating factory runs once per scope and resolves the inner from that scope,
    /// so the scope still creates and disposes one inner for each decorator it caches, without caching the inner a second time.
    /// An original keyed with <see cref="KeyedService.AnyKey"/> gets one cached decorator, and so one inner, for each requested key.
    /// </remarks>
    private static ServiceDescriptor CreateInner(ServiceDescriptor original, DecoratedServiceKey innerKey)
    {
        ServiceLifetime lifetime = original.Lifetime == ServiceLifetime.Scoped ? ServiceLifetime.Transient : original.Lifetime;
        Type? implementationType = original.IsKeyedService ? original.KeyedImplementationType : original.ImplementationType;
        if (implementationType is not null)
        {
            return new ServiceDescriptor(implementationType, innerKey, implementationType, lifetime);
        }

        object? instance = original.IsKeyedService ? original.KeyedImplementationInstance : original.ImplementationInstance;
        if (instance is not null)
        {
            return new ServiceDescriptor(instance.GetType(), innerKey, instance);
        }

        if (original.IsKeyedService)
        {
            Func<IServiceProvider, object?, object> keyedFactory = original.KeyedImplementationFactory!;
            object? originalKey = original.ServiceKey;
            return new ServiceDescriptor(typeof(DecoratedService), innerKey, (provider, _) => keyedFactory(provider, originalKey), lifetime);
        }

        Func<IServiceProvider, object> factory = original.ImplementationFactory!;
        return new ServiceDescriptor(typeof(DecoratedService), innerKey, (provider, _) => factory(provider), lifetime);
    }

    private sealed class DecoratedService;

    private sealed class DecoratedServiceKey(Type serviceType, Func<object, IServiceProvider, object> decorator)
    {
        public Type InnerServiceType { get; set; } = typeof(DecoratedService);

        public object Decorate(IServiceProvider provider) =>
            decorator(provider.GetRequiredKeyedService(InnerServiceType, this), provider);

        public override string ToString() => $"decorated inner of {serviceType.FullName}";
    }
}
