using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.DependencyInjection;

internal static class ServiceDescriptorExtensions
{
    public static bool Matches(this ServiceDescriptor descriptor, Type serviceType, object? serviceKey) =>
        descriptor.ServiceType == serviceType && Equals(descriptor.ServiceKey, serviceKey);

    public static Type? GetImplementationType(this ServiceDescriptor descriptor) =>
        descriptor.IsKeyedService ? descriptor.KeyedImplementationType : descriptor.ImplementationType;

    public static object? GetImplementationInstance(this ServiceDescriptor descriptor) =>
        descriptor.IsKeyedService ? descriptor.KeyedImplementationInstance : descriptor.ImplementationInstance;

    public static Delegate? GetImplementationFactory(this ServiceDescriptor descriptor) =>
        descriptor.IsKeyedService ? descriptor.KeyedImplementationFactory : descriptor.ImplementationFactory;

    public static object? GetImplementationIdentity(this ServiceDescriptor descriptor) =>
        descriptor.GetImplementationType()
        ?? descriptor.GetImplementationInstance()?.GetType()
        ?? (object?)descriptor.GetImplementationFactory();

    public static string DescribeImplementation(this ServiceDescriptor descriptor)
    {
        if (descriptor.GetImplementationType() is { } implementationType)
        {
            return implementationType.FullName ?? implementationType.Name;
        }

        if (descriptor.GetImplementationInstance() is { } instance)
        {
            Type instanceType = instance.GetType();
            return $"instance of {instanceType.FullName ?? instanceType.Name}";
        }

        return "factory";
    }

    public static string DescribeKey(object? serviceKey) =>
        serviceKey is null ? "(none)" : serviceKey.ToString() ?? serviceKey.GetType().Name;
}
