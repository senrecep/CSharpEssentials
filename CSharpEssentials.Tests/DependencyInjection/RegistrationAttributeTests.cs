using CSharpEssentials.DependencyInjection;
using FluentAssertions;

namespace CSharpEssentials.Tests.DependencyInjection;

public class RegistrationAttributeTests
{
    private interface IStore;

    [Fact]
    public void RegisterAttributes_Should_LeaveServiceTypeNull_When_ParameterlessConstructorIsUsed()
    {
        RegisterScopedAttribute scoped = new();
        RegisterSingletonAttribute singleton = new();
        RegisterTransientAttribute transient = new();

        scoped.ServiceType.Should().BeNull();
        singleton.ServiceType.Should().BeNull();
        transient.ServiceType.Should().BeNull();
        scoped.Key.Should().BeNull();
        scoped.As.Should().Be(ServiceAs.Self);
        scoped.Strategy.Should().Be(RegistrationStrategy.Add);
    }

    [Fact]
    public void RegisterScopedAttribute_Should_ExposeServiceTypeAndSettings()
    {
        RegisterScopedAttribute attribute = new(typeof(IStore)) { Key = "a", As = ServiceAs.ImplementedInterfaces, Strategy = RegistrationStrategy.Replace };

        attribute.ServiceType.Should().Be<IStore>();
        attribute.Key.Should().Be("a");
        attribute.As.Should().Be(ServiceAs.ImplementedInterfaces);
        attribute.Strategy.Should().Be(RegistrationStrategy.Replace);
    }

    [Fact]
    public void RegisterSingletonAttribute_Should_ExposeServiceTypeAndSettings()
    {
        RegisterSingletonAttribute attribute = new(typeof(IStore)) { Key = 1, As = ServiceAs.SelfWithInterfaces, Strategy = RegistrationStrategy.TryAdd };

        attribute.ServiceType.Should().Be<IStore>();
        attribute.Key.Should().Be(1);
        attribute.As.Should().Be(ServiceAs.SelfWithInterfaces);
        attribute.Strategy.Should().Be(RegistrationStrategy.TryAdd);
    }

    [Fact]
    public void RegisterTransientAttribute_Should_ExposeServiceTypeAndSettings()
    {
        RegisterTransientAttribute attribute = new(typeof(IStore)) { Key = "t", As = ServiceAs.Self, Strategy = RegistrationStrategy.Throw };

        attribute.ServiceType.Should().Be<IStore>();
        attribute.Key.Should().Be("t");
        attribute.As.Should().Be(ServiceAs.Self);
        attribute.Strategy.Should().Be(RegistrationStrategy.Throw);
    }

    [Fact]
    public void GenericRegisterAttributes_Should_UseTypeArgumentAsServiceType()
    {
        RegisterScopedAttribute<IStore> scoped = new() { Key = "s", As = ServiceAs.SelfWithInterfaces, Strategy = RegistrationStrategy.TryAddEnumerable };
        RegisterSingletonAttribute<IStore> singleton = new() { Key = "g", As = ServiceAs.ImplementedInterfaces, Strategy = RegistrationStrategy.Replace };
        RegisterTransientAttribute<IStore> transient = new() { Key = "t", As = ServiceAs.Self, Strategy = RegistrationStrategy.Throw };

        scoped.ServiceType.Should().Be<IStore>();
        scoped.Key.Should().Be("s");
        scoped.As.Should().Be(ServiceAs.SelfWithInterfaces);
        scoped.Strategy.Should().Be(RegistrationStrategy.TryAddEnumerable);
        singleton.ServiceType.Should().Be<IStore>();
        singleton.Key.Should().Be("g");
        singleton.As.Should().Be(ServiceAs.ImplementedInterfaces);
        singleton.Strategy.Should().Be(RegistrationStrategy.Replace);
        transient.ServiceType.Should().Be<IStore>();
        transient.Key.Should().Be("t");
        transient.As.Should().Be(ServiceAs.Self);
        transient.Strategy.Should().Be(RegistrationStrategy.Throw);
    }

    [Fact]
    public void DecoratesAttributes_Should_ExposeServiceTypeOrderAndKey()
    {
        DecoratesAttribute plain = new(typeof(IStore)) { Order = 3, Key = "k" };
        DecoratesAttribute<IStore> generic = new() { Order = -1, Key = 7 };

        plain.ServiceType.Should().Be<IStore>();
        plain.Order.Should().Be(3);
        plain.Key.Should().Be("k");
        generic.ServiceType.Should().Be<IStore>();
        generic.Order.Should().Be(-1);
        generic.Key.Should().Be(7);
        new DecoratesAttribute<IStore>().Order.Should().Be(0);
    }

    [Fact]
    public void ServiceRegistryNameAttribute_Should_ExposeName()
    {
        new ServiceRegistryNameAttribute("Billing").Name.Should().Be("Billing");
    }
}
