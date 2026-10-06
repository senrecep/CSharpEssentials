using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.DependencyInjection;

public class ServiceCollectionDecorationExtensionsTests
{
    public interface IGreeter
    {
        string Greet();
    }

    private interface IRepository<T>
    {
        string Describe(T sample);
    }

    public interface IPrefix
    {
        string Value { get; }
    }

    private sealed class Greeter : IGreeter
    {
        public string Greet() => "hello";
    }

    private sealed class NamedGreeter(string name) : IGreeter
    {
        public string Greet() => name;
    }

    private sealed class LoudGreeter(IGreeter inner) : IGreeter
    {
        public string Greet() => inner.Greet().ToUpperInvariant();
    }

    private sealed class ExclaimingGreeter(IGreeter inner) : IGreeter
    {
        public string Greet() => inner.Greet() + "!";
    }

    private sealed class Prefix(string value) : IPrefix
    {
        public string Value { get; } = value;
    }

    private sealed class PrefixingGreeter(
        IGreeter inner,
        [FromKeyedServices("prefix")] IPrefix prefix,
        [ServiceKey] object? key,
        IServiceProvider? optional = null,
        string suffix = "?") : IGreeter
    {
        public string Greet() => $"{prefix.Value}{key}:{inner.Greet()}{suffix}{(optional is null ? "-" : "+")}";
    }

    private sealed class KeyAwareGreeter([ServiceKey] object key) : IGreeter
    {
        public string Greet() => key.ToString() ?? string.Empty;
    }

    public sealed class TwoConstructorGreeter : IGreeter
    {
        public TwoConstructorGreeter(IGreeter inner) => Inner = inner;

        public TwoConstructorGreeter(IGreeter inner, IPrefix prefix)
        {
            Inner = inner;
            Prefix = prefix;
        }

        public IGreeter Inner { get; }

        public IPrefix? Prefix { get; }

        public string Greet() => Inner.Greet();
    }

    private sealed class NoInnerGreeter : IGreeter
    {
        public string Greet() => "none";
    }

    private sealed class TrackingGreeter(IGreeter? inner = null) : IGreeter, IDisposable
    {
        public static List<string> Disposed { get; } = [];

        public string Greet() => inner?.Greet() ?? "tracked";

        public void Dispose() => Disposed.Add(inner is null ? "inner" : "decorator");
    }

    private sealed class Repository<T> : IRepository<T>
    {
        public string Describe(T sample) => $"{typeof(T).Name}:{sample}";
    }

    private sealed class LoggingRepository<T>(IRepository<T> inner) : IRepository<T>
    {
        public string Describe(T sample) => $"logged({inner.Describe(sample)})";
    }

    [Fact]
    public void Decorate_Should_WrapRegistration()
    {
        ServiceCollection services = new();
        services.AddScoped<IGreeter, Greeter>();

        services.Decorate<IGreeter, LoudGreeter>();

        using ServiceProvider provider = services.BuildServiceProvider();
        provider.GetRequiredService<IGreeter>().Greet().Should().Be("HELLO");
    }

    [Fact]
    public void Decorate_Should_ApplyInCallOrder_When_CalledTwice()
    {
        ServiceCollection services = new();
        services.AddScoped<IGreeter, Greeter>();

        services.Decorate<IGreeter, ExclaimingGreeter>().Decorate<IGreeter, LoudGreeter>();

        using ServiceProvider provider = services.BuildServiceProvider();
        provider.GetRequiredService<IGreeter>().Greet().Should().Be("HELLO!");
    }

    [Fact]
    public void Decorate_Should_DecorateEveryRegistrationInOrder_When_MultipleRegistrationsExist()
    {
        ServiceCollection services = new();
        services.AddSingleton<IGreeter>(new NamedGreeter("a"));
        services.AddTransient<IGreeter>(_ => new NamedGreeter("b"));
        services.AddScoped<IGreeter, Greeter>();

        services.Decorate<IGreeter, LoudGreeter>();

        using ServiceProvider provider = services.BuildServiceProvider();
        provider.GetServices<IGreeter>().Select(g => g.Greet()).Should().Equal("A", "B", "HELLO");
    }

    [Fact]
    public void Decorate_Should_PreserveLifetime()
    {
        ServiceCollection services = new();
        services.AddSingleton<IGreeter, Greeter>();

        services.Decorate<IGreeter, LoudGreeter>();

        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<IGreeter>().Should().BeSameAs(provider.GetRequiredService<IGreeter>());
        services.Single(d => d.ServiceType == typeof(IGreeter)).Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void Decorate_Should_DecorateOnlyKeyedRegistration_When_ServiceKeyGiven()
    {
        ServiceCollection services = new();
        services.AddKeyedScoped<IGreeter>("a", (_, _) => new NamedGreeter("a"));
        services.AddKeyedScoped<IGreeter>("b", (_, _) => new NamedGreeter("b"));
        services.AddScoped<IGreeter, Greeter>();

        services.Decorate<IGreeter, LoudGreeter>("a");

        using ServiceProvider provider = services.BuildServiceProvider();
        provider.GetRequiredKeyedService<IGreeter>("a").Greet().Should().Be("A");
        provider.GetRequiredKeyedService<IGreeter>("b").Greet().Should().Be("b");
        provider.GetRequiredService<IGreeter>().Greet().Should().Be("hello");
    }

    [Fact]
    public void Decorate_Should_PassOriginalKeyToKeyedFactory()
    {
        ServiceCollection services = new();
        services.AddKeyedSingleton<IGreeter>("eu", (_, key) => new NamedGreeter($"key={key}"));

        services.Decorate<IGreeter, LoudGreeter>("eu");

        using ServiceProvider provider = services.BuildServiceProvider();
        provider.GetRequiredKeyedService<IGreeter>("eu").Greet().Should().Be("KEY=EU");
    }

    [Fact]
    public void Decorate_Should_ResolveKeyedServiceKeyAndOptionalParameters()
    {
        ServiceCollection services = new();
        services.AddKeyedSingleton<IPrefix>("prefix", new Prefix(">"));
        services.AddKeyedScoped<IGreeter, Greeter>("k");

        services.Decorate<IGreeter, PrefixingGreeter>("k");

        using ServiceProvider provider = services.BuildServiceProvider();
        provider.GetRequiredKeyedService<IGreeter>("k").Greet().Should().Be(">k:hello?+");
    }

    [Fact]
    public void Decorate_Should_UseFactory_When_FuncOverloadUsed()
    {
        ServiceCollection services = new();
        services.AddKeyedScoped<IGreeter, Greeter>(7);

        services.Decorate<IGreeter>((inner, _) => new NamedGreeter(inner.Greet() + "7"), 7);

        using ServiceProvider provider = services.BuildServiceProvider();
        provider.GetRequiredKeyedService<IGreeter>(7).Greet().Should().Be("hello7");
    }

    [Fact]
    public void Decorate_Should_NotLeakInnerRegistration_When_ResolvingAllServices()
    {
        ServiceCollection services = new();
        services.AddScoped<IGreeter, Greeter>();
        services.AddKeyedScoped<IGreeter, Greeter>("a");

        services.Decorate<IGreeter, LoudGreeter>().Decorate<IGreeter, LoudGreeter>("a");

        using ServiceProvider provider = services.BuildServiceProvider();
        provider.GetServices<IGreeter>().Should().ContainSingle().Which.Should().BeOfType<LoudGreeter>();
        provider.GetKeyedServices<IGreeter>(KeyedService.AnyKey).Should().ContainSingle().Which.Should().BeOfType<LoudGreeter>();
        provider.GetKeyedServices<IGreeter>("a").Should().ContainSingle().Which.Should().BeOfType<LoudGreeter>();
    }

    [Fact]
    public void Decorate_Should_NotExposeInnerRegistration_When_ResolvingAllKeyedObjects()
    {
        ServiceCollection services = new();
        services.AddScoped<IGreeter, Greeter>();
        services.AddSingleton<IGreeter>(new NamedGreeter("instance"));
        services.AddTransient<IGreeter>(static _ => new NamedGreeter("factory"));
        services.AddKeyedTransient<IGreeter>("k", static (_, key) => new NamedGreeter("keyed-" + key));

        services.Decorate<IGreeter, LoudGreeter>().Decorate<IGreeter, LoudGreeter>("k");

        using ServiceProvider provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
        using IServiceScope scope = provider.CreateScope();
        scope.ServiceProvider.GetKeyedServices<object>(KeyedService.AnyKey).Should().BeEmpty();
        scope.ServiceProvider.GetKeyedServices<IGreeter>(KeyedService.AnyKey).Should().ContainSingle().Which.Greet().Should().Be("KEYED-K");
        scope.ServiceProvider.GetServices<IGreeter>().Select(static greeter => greeter.Greet()).Should().Equal("HELLO", "INSTANCE", "FACTORY");
    }

    [Fact]
    public void Decorate_Should_DisposeDecoratorAndInner()
    {
        TrackingGreeter.Disposed.Clear();
        ServiceCollection services = new();
        services.AddScoped<IGreeter>(_ => new TrackingGreeter());
        services.Decorate<IGreeter>((inner, _) => new TrackingGreeter(inner));
        ServiceProvider provider = services.BuildServiceProvider();
        IServiceScope scope = provider.CreateScope();
        _ = scope.ServiceProvider.GetRequiredService<IGreeter>();

        scope.Dispose();
        provider.Dispose();

        TrackingGreeter.Disposed.Should().BeEquivalentTo("inner", "decorator");
    }

    [Fact]
    public void Decorate_Should_PassContainerValidation_When_ValidateOnBuildAndScopesEnabled()
    {
        ServiceCollection services = new();
        services.AddScoped<IGreeter, Greeter>();
        services.AddKeyedSingleton<IGreeter, Greeter>("a");
        services.Decorate<IGreeter, LoudGreeter>().Decorate<IGreeter, ExclaimingGreeter>("a");

        Action act = () =>
        {
            using ServiceProvider provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
            using IServiceScope scope = provider.CreateScope();
            scope.ServiceProvider.GetRequiredService<IGreeter>().Greet().Should().Be("HELLO");
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void Decorate_Should_ReportCaptiveDependency_When_ValidateScopesEnabled()
    {
        ServiceCollection services = new();
        services.AddKeyedScoped<IPrefix>("prefix", (_, _) => new Prefix(">"));
        services.AddSingleton<IGreeter, Greeter>();
        services.Decorate<IGreeter>((inner, sp) => new NamedGreeter(sp.GetRequiredKeyedService<IPrefix>("prefix").Value + inner.Greet()));
        using ServiceProvider provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

        Action act = () => provider.GetRequiredService<IGreeter>();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Decorate_Should_Throw_When_NothingMatches()
    {
        ServiceCollection services = new();
        services.AddKeyedScoped<IGreeter, Greeter>("a");

        Action act = () => services.Decorate<IGreeter, LoudGreeter>();

        act.Should().Throw<InvalidOperationException>().WithMessage($"*{typeof(IGreeter).FullName}*{typeof(LoudGreeter).FullName}*");
    }

    [Fact]
    public void Decorate_Should_ThrowWithoutDecoratorName_When_FuncOverloadMatchesNothing()
    {
        ServiceCollection services = new();

        Action act = () => services.Decorate<IGreeter>((inner, _) => inner, "x");

        act.Should().Throw<InvalidOperationException>().WithMessage("*'x'*");
    }

    [Fact]
    public void TryDecorate_Should_ReturnFalseAndLeaveServicesUnchanged_When_NothingMatches()
    {
        ServiceCollection services = new();
        services.AddScoped<IGreeter, Greeter>();

        bool decorated = services.TryDecorate<IGreeter, LoudGreeter>("missing");

        decorated.Should().BeFalse();
        services.Should().ContainSingle().Which.ImplementationType.Should().Be<Greeter>();
    }

    [Fact]
    public void TryDecorate_Should_ReturnTrue_When_RegistrationMatches()
    {
        ServiceCollection services = new();
        services.AddScoped<IGreeter, Greeter>();

        bool decorated = services.TryDecorate<IGreeter, LoudGreeter>();

        decorated.Should().BeTrue();
    }

    [Fact]
    public void Decorate_Should_ThrowAndLeaveServicesUnchanged_When_OriginalInjectsServiceKey()
    {
        ServiceCollection services = new();
        services.AddKeyedScoped<IGreeter, Greeter>("a");
        services.AddKeyedScoped<IGreeter, KeyAwareGreeter>("a");
        ServiceDescriptor[] before = [.. services];

        Action act = () => services.Decorate<IGreeter, LoudGreeter>("a");

        act.Should().Throw<InvalidOperationException>().WithMessage($"*{typeof(KeyAwareGreeter).FullName}*[ServiceKey]*");
        services.Should().Equal(before);
    }

    [Fact]
    public void Decorate_Should_Throw_When_DecoratorHasSeveralPublicConstructors()
    {
        ServiceCollection services = new();
        services.AddScoped<IGreeter, Greeter>();

        Action act = () => services.Decorate<IGreeter, TwoConstructorGreeter>();

        act.Should().Throw<InvalidOperationException>().WithMessage("*exactly one public constructor*");
    }

    [Fact]
    public void Decorate_Should_Throw_When_DecoratorDoesNotTakeInnerService()
    {
        ServiceCollection services = new();
        services.AddScoped<IGreeter, Greeter>();

        Action act = () => services.Decorate<IGreeter, NoInnerGreeter>();

        act.Should().Throw<InvalidOperationException>().WithMessage("*exactly one parameter*");
        services.Should().ContainSingle();
    }

    [Fact]
    public void DecorateOpenGeneric_Should_DecorateEveryClosedRegistrationKeepingKeys()
    {
        ServiceCollection services = new();
        services.AddScoped<IRepository<int>, Repository<int>>();
        services.AddKeyedScoped<IRepository<string>, Repository<string>>("k");
        services.AddScoped<IGreeter, Greeter>();

        services.Decorate(typeof(IRepository<>), typeof(LoggingRepository<>));

        using ServiceProvider provider = services.BuildServiceProvider();
        provider.GetRequiredService<IRepository<int>>().Describe(1).Should().Be("logged(Int32:1)");
        provider.GetRequiredKeyedService<IRepository<string>>("k").Describe("s").Should().Be("logged(String:s)");
        provider.GetRequiredService<IGreeter>().Greet().Should().Be("hello");
    }

    [Fact]
    public void DecorateOpenGeneric_Should_ThrowAndLeaveServicesUnchanged_When_OpenRegistrationExists()
    {
        ServiceCollection services = new();
        services.AddScoped<IRepository<int>, Repository<int>>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        ServiceDescriptor[] before = [.. services];

        Action act = () => services.Decorate(typeof(IRepository<>), typeof(LoggingRepository<>));

        act.Should().Throw<InvalidOperationException>().WithMessage($"*{typeof(Repository<>).FullName}*");
        services.Should().Equal(before);
    }

    [Fact]
    public void DecorateOpenGeneric_Should_Throw_When_NothingMatches()
    {
        ServiceCollection services = new();

        Action act = () => services.Decorate(typeof(IRepository<>), typeof(LoggingRepository<>));

        act.Should().Throw<InvalidOperationException>().WithMessage("*No closed registration*");
    }

    [Fact]
    public void DecorateOpenGeneric_Should_Throw_When_TypesAreNotOpenGenerics()
    {
        ServiceCollection services = new();

        Action act = () => services.Decorate(typeof(IGreeter), typeof(LoudGreeter));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Decorate_Should_Throw_When_ArgumentsAreNull()
    {
        ServiceCollection services = new();

        Action nullServices = () => ((IServiceCollection)null!).Decorate<IGreeter, LoudGreeter>();
        Action nullFactory = () => services.Decorate<IGreeter>(null!);
        Action nullType = () => services.Decorate(null!, typeof(LoggingRepository<>));

        nullServices.Should().Throw<ArgumentNullException>().WithParameterName("services");
        nullFactory.Should().Throw<ArgumentNullException>().WithParameterName("decorator");
        nullType.Should().Throw<ArgumentNullException>().WithParameterName("serviceType");
    }
}
