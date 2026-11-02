using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.DependencyInjection;

public class DecoratorActivationTests
{
    public interface IGreeter
    {
        string Greet();
    }

    public interface IPrefix
    {
        string Value { get; }
    }

    private interface IRepository<T>
    {
        string Describe(T sample);
    }

    private sealed class Greeter : IGreeter
    {
        public string Greet() => "hello";
    }

    private sealed class Prefix(string value) : IPrefix
    {
        public string Value { get; } = value;
    }

    private abstract class AbstractGreeter(IGreeter inner) : IGreeter
    {
        public string Greet() => inner.Greet();
    }

    private sealed class NonKeyedPrefixGreeter(IGreeter inner, [FromKeyedServices(null!)] IPrefix prefix) : IGreeter
    {
        public string Greet() => prefix.Value + inner.Greet();
    }

    private sealed class Repository<T> : IRepository<T>
    {
        public string Describe(T sample) => $"{sample}";
    }

    private sealed class UnrelatedDecorator<T>(IRepository<T> inner)
    {
        public override string ToString() => inner.Describe(default!);
    }

    [Fact]
    public void Decorate_Should_Throw_When_DecoratorIsAbstract()
    {
        ServiceCollection services = new();
        services.AddScoped<IGreeter, Greeter>();

        Action act = () => services.Decorate<IGreeter, AbstractGreeter>();

        act.Should().Throw<InvalidOperationException>().WithMessage($"*{typeof(AbstractGreeter).FullName}*abstract*");
        services.Should().ContainSingle().Which.ImplementationType.Should().Be<Greeter>();
    }

    [Fact]
    public void DecorateOpenGeneric_Should_Throw_When_ClosedDecoratorDoesNotImplementService()
    {
        ServiceCollection services = new();
        services.AddScoped<IRepository<int>, Repository<int>>();

        Action act = () => services.Decorate(typeof(IRepository<>), typeof(UnrelatedDecorator<>));

        act.Should().Throw<InvalidOperationException>().WithMessage("*UnrelatedDecorator*does not implement*IRepository*");
        services.Should().ContainSingle().Which.ImplementationType.Should().Be<Repository<int>>();
    }

    [Fact]
    public void Decorate_Should_ResolveNonKeyedDependency_When_FromKeyedServicesKeyIsNull()
    {
        ServiceCollection services = new();
        services.AddKeyedScoped<IGreeter, Greeter>("k");
        services.AddSingleton<IPrefix>(new Prefix("plain:"));
        services.AddKeyedSingleton<IPrefix>("k", new Prefix("keyed:"));

        services.Decorate<IGreeter, NonKeyedPrefixGreeter>("k");

        using ServiceProvider provider = services.BuildServiceProvider();
        provider.GetRequiredKeyedService<IGreeter>("k").Greet().Should().Be("plain:hello");
    }

    [Fact]
    public void Decorate_Should_KeyInnerRegistrationWithDescriptiveKey()
    {
        ServiceCollection services = new();
        services.AddScoped<IGreeter, Greeter>();

        services.Decorate<IGreeter>((inner, _) => inner);

        services.Should().HaveCount(2);
        services[1].ServiceKey!.ToString().Should().Be($"decorated inner of {typeof(IGreeter).FullName}");
    }
}
