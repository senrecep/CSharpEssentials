using CSharpEssentials.DependencyInjection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.DependencyInjection;

public class ServiceRegistrationDescriptorTests
{
    private interface IStore;

    private sealed class FirstStore : IStore;

    private sealed class SecondStore : IStore;

    private sealed class NamelessKey
    {
        public override string? ToString() => null;
    }

    private static readonly Func<IServiceProvider, object> Factory = static _ => new FirstStore();

    private static readonly Func<IServiceProvider, object?, object> KeyedFactory = static (_, _) => new FirstStore();

    private static IServiceCollection Services(params ServiceDescriptor[] descriptors)
    {
        IServiceCollection services = new ServiceCollection();
        foreach (ServiceDescriptor descriptor in descriptors)
        {
            services.Add(descriptor);
        }

        return services;
    }

    [Fact]
    public void Apply_Should_DescribeInstanceAndFactory_When_StrategyIsThrowAndNonKeyedMatchExists()
    {
        IServiceCollection services = Services(new ServiceDescriptor(typeof(IStore), new FirstStore()));

        Action act = () => ServiceRegistration.Apply(
            services, null, new ServiceDescriptor(typeof(IStore), Factory, ServiceLifetime.Singleton), RegistrationStrategy.Throw);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*key '(none)'*by 'instance of {typeof(FirstStore).FullName}'*allow 'factory' to be added*");
        services.Should().ContainSingle();
    }

    [Fact]
    public void Apply_Should_DescribeKeyedInstanceAndFactory_When_StrategyIsThrowAndKeyedMatchExists()
    {
        IServiceCollection services = Services(new ServiceDescriptor(typeof(IStore), "a", new SecondStore()));

        Action act = () => ServiceRegistration.Apply(
            services, null, new ServiceDescriptor(typeof(IStore), "a", KeyedFactory, ServiceLifetime.Scoped), RegistrationStrategy.Throw);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*key 'a'*by 'instance of {typeof(SecondStore).FullName}'*allow 'factory' to be added*");
        services.Should().ContainSingle();
    }

    [Fact]
    public void Apply_Should_DescribeKeyByTypeName_When_KeyToStringReturnsNull()
    {
        NamelessKey key = new();
        IServiceCollection services = Services(new ServiceDescriptor(typeof(IStore), key, typeof(FirstStore), ServiceLifetime.Scoped));

        Action act = () => ServiceRegistration.Apply(
            services, null, new ServiceDescriptor(typeof(IStore), key, typeof(SecondStore), ServiceLifetime.Scoped), RegistrationStrategy.Throw);

        act.Should().Throw<InvalidOperationException>().WithMessage($"*key '{nameof(NamelessKey)}'*");
    }

    [Fact]
    public void Apply_Should_SkipInstanceOfSameType_When_StrategyIsTryAddEnumerable()
    {
        IServiceCollection services = Services(
            new ServiceDescriptor(typeof(IStore), new FirstStore()),
            new ServiceDescriptor(typeof(IStore), "a", new FirstStore()));

        ServiceRegistration.Apply(services, null, new ServiceDescriptor(typeof(IStore), new FirstStore()), RegistrationStrategy.TryAddEnumerable);
        ServiceRegistration.Apply(services, null, new ServiceDescriptor(typeof(IStore), "a", new FirstStore()), RegistrationStrategy.TryAddEnumerable);
        ServiceRegistration.Apply(services, null, new ServiceDescriptor(typeof(IStore), "a", new SecondStore()), RegistrationStrategy.TryAddEnumerable);

        services.Should().HaveCount(3);
        services[2].KeyedImplementationInstance.Should().BeOfType<SecondStore>();
    }

    [Fact]
    public void Apply_Should_SkipSameFactory_When_StrategyIsTryAddEnumerable()
    {
        IServiceCollection services = Services(
            new ServiceDescriptor(typeof(IStore), Factory, ServiceLifetime.Transient),
            new ServiceDescriptor(typeof(IStore), "a", KeyedFactory, ServiceLifetime.Transient));

        ServiceRegistration.Apply(services, null, new ServiceDescriptor(typeof(IStore), Factory, ServiceLifetime.Transient), RegistrationStrategy.TryAddEnumerable);
        ServiceRegistration.Apply(services, null, new ServiceDescriptor(typeof(IStore), "a", KeyedFactory, ServiceLifetime.Transient), RegistrationStrategy.TryAddEnumerable);
        ServiceRegistration.Apply(services, null, new ServiceDescriptor(typeof(IStore), "b", KeyedFactory, ServiceLifetime.Transient), RegistrationStrategy.TryAddEnumerable);

        services.Should().HaveCount(3);
        services[2].ServiceKey.Should().Be("b");
    }
}
