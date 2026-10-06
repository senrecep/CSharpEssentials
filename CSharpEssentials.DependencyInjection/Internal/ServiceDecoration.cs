using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.DependencyInjection;

internal static class ServiceDecoration
{
    public static bool TryDecorate(
        IServiceCollection services,
        Type serviceType,
        object? serviceKey,
        Func<object, IServiceProvider, object> decorator)
    {
        List<int> matches = [];
        for (int index = 0; index < services.Count; index++)
        {
            if (services[index].Matches(serviceType, serviceKey))
            {
                EnsureDecoratable(services[index]);
                matches.Add(index);
            }
        }

        foreach (int index in matches)
        {
            Replace(services, index, decorator);
        }

        return matches.Count > 0;
    }

    [RequiresDynamicCode("Closes the open-generic decorator for each matching closed registration.")]
    [RequiresUnreferencedCode("Closes the open-generic decorator for each matching closed registration; its constructors may be trimmed.")]
    public static void DecorateOpenGeneric(
        IServiceCollection services,
        Type serviceType,
        Type decoratorType,
        Func<object?, bool> keyFilter)
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
                EnsureDecoratable(descriptor);
                Type closedDecorator = decoratorType.MakeGenericType(descriptor.ServiceType.GenericTypeArguments);
                plan.Add((index, DecoratorActivator.Create(closedDecorator, descriptor.ServiceType, descriptor.ServiceKey)));
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

    private static void EnsureDecoratable(ServiceDescriptor descriptor)
    {
        Type? implementationType = descriptor.IsKeyedService ? descriptor.KeyedImplementationType : descriptor.ImplementationType;
        if (implementationType is null)
        {
            return;
        }

        bool injectsServiceKey = implementationType
            .GetConstructors()
            .SelectMany(static constructor => constructor.GetParameters())
            .Any(static parameter => parameter.GetCustomAttribute<ServiceKeyAttribute>() is not null);
        if (injectsServiceKey)
        {
            throw new InvalidOperationException(
                $"'{implementationType.FullName}' injects [ServiceKey] and cannot be decorated, because its hidden inner registration uses a private key.");
        }
    }

    private static void Replace(IServiceCollection services, int index, Func<object, IServiceProvider, object> decorator)
    {
        ServiceDescriptor original = services[index];
        DecoratedServiceKey innerKey = new(original.ServiceType);
        ServiceDescriptor inner = CreateInner(original, innerKey);
        Type innerServiceType = inner.ServiceType;

        services[index] = new ServiceDescriptor(
            original.ServiceType,
            original.ServiceKey,
            (provider, _) => decorator(provider.GetRequiredKeyedService(innerServiceType, innerKey), provider),
            original.Lifetime);
        services.Add(inner);
    }

    /// <remarks>
    /// The inner registration is never keyed under <see cref="object"/> or the decorated service type, so
    /// <c>GetKeyedServices&lt;object&gt;(KeyedService.AnyKey)</c> and <c>GetKeyedServices&lt;TService&gt;(KeyedService.AnyKey)</c>
    /// do not return it. Microsoft.Extensions.DependencyInjection requires the implementation or instance to be assignable to the
    /// service type, so those originals are keyed under their own concrete type; factories are keyed under <see cref="DecoratedService"/>.
    /// </remarks>
    private static ServiceDescriptor CreateInner(ServiceDescriptor original, DecoratedServiceKey innerKey)
    {
        Type? implementationType = original.IsKeyedService ? original.KeyedImplementationType : original.ImplementationType;
        if (implementationType is not null)
        {
            return new ServiceDescriptor(implementationType, innerKey, implementationType, original.Lifetime);
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
            return new ServiceDescriptor(typeof(DecoratedService), innerKey, (provider, _) => keyedFactory(provider, originalKey), original.Lifetime);
        }

        Func<IServiceProvider, object> factory = original.ImplementationFactory!;
        return new ServiceDescriptor(typeof(DecoratedService), innerKey, (provider, _) => factory(provider), original.Lifetime);
    }

    private sealed class DecoratedService;

    private sealed class DecoratedServiceKey(Type serviceType)
    {
        public override string ToString() => $"decorated inner of {serviceType.FullName}";
    }
}
