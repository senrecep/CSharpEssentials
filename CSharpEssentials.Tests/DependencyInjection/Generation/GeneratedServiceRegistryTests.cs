using System.Reflection;
using CSharpEssentials.Tests.DependencyInjection.Scanning;
using CSharpEssentials.Tests.Fixtures.DependencyInjectionA;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Tests.DependencyInjection.Generation;

public class GeneratedServiceRegistryTests
{
    private static readonly ServiceProviderOptions Validated = new() { ValidateOnBuild = true, ValidateScopes = true };

    [Fact]
    public void AddCSharpEssentialsTestsServices_Should_Register_Same_Descriptors_As_Assembly_Scanning()
    {
        ServiceCollection scanned = new();
        scanned.AddServicesFromAssemblies(typeof(ScanGreeter).Assembly);

        ServiceCollection generated = new();
        generated.AddCSharpEssentialsTestsServices();

        generated.Select(Describe).Should().BeEquivalentTo(scanned.Select(Describe));
    }

    [Fact]
    public void AddCSharpEssentialsTestsServices_Should_Register_Record_Services()
    {
        ServiceCollection services = new();

        services.AddCSharpEssentialsTestsServices();

        services.Should().ContainSingle(static descriptor => descriptor.ServiceType == typeof(IScanRecordService))
            .Which.ImplementationType.Should().Be<ScanRecordService>();
    }

    [Fact]
    public void AddCSharpEssentialsTestsServices_Should_Apply_Decorators_In_Order()
    {
        ServiceCollection services = new();
        services.AddCSharpEssentialsTestsServices();
        using ServiceProvider provider = services.BuildServiceProvider(Validated);
        using IServiceScope scope = provider.CreateScope();

        string greeting = scope.ServiceProvider.GetRequiredService<IScanGreeter>().Greet();
        string keyed = scope.ServiceProvider.GetRequiredKeyedService<IScanGreeter>("k").Greet();

        greeting.Should().Be("outer(inner(hi))");
        keyed.Should().Be("keyed-decorator(keyed)");
    }

    [Fact]
    public void AddCSharpEssentialsTestsServices_Should_Forward_Interfaces_To_Single_Instance()
    {
        ServiceCollection services = new();
        services.AddCSharpEssentialsTestsServices();
        using ServiceProvider provider = services.BuildServiceProvider(Validated);

        object shared = provider.GetRequiredService<SharedService>();

        provider.GetRequiredService<ISharedService>().Should().BeSameAs(shared);
        provider.GetRequiredService<ISecondShared>().Should().BeSameAs(shared);
    }

    [Fact]
    public void AddCSharpEssentialsTestsServices_Should_Log_Duplicate_At_Debug_When_Logger_Is_Given()
    {
        ServiceCollection services = new();
        services.AddScoped<IConventionalService, ConventionalService>();
        CapturingLogger logger = new();

        services.AddCSharpEssentialsTestsServices(logger);

        logger.Entries.Should().Contain(entry => entry.Level == LogLevel.Debug && entry.Message.Contains(nameof(IConventionalService), StringComparison.Ordinal));
    }

    [Fact]
    public void AddCSharpEssentialsTestsServices_Should_Not_Throw_When_Logger_Is_Null()
    {
        ServiceCollection services = new();
        services.AddScoped<IConventionalService, ConventionalService>();

        Action act = () => services.AddCSharpEssentialsTestsServices(logger: null);

        act.Should().NotThrow();
    }

    [Fact]
    public void AddAllServices_Should_Register_All_Assemblies_Before_Applying_Decorators()
    {
        ServiceCollection services = new();
        services.AddAllServices();
        using ServiceProvider provider = services.BuildServiceProvider(Validated);
        using IServiceScope scope = provider.CreateScope();

        string greeting = scope.ServiceProvider.GetRequiredService<IFixtureGreeter>().Greet();
        string message = scope.ServiceProvider.GetRequiredService<IFixtureMessage>().Text();
        string own = scope.ServiceProvider.GetRequiredService<IScanGreeter>().Greet();

        greeting.Should().Be("b(a)");
        message.Should().Be("a(b)");
        own.Should().Be("outer(inner(hi))");
    }

    [Fact]
    public void AddAllServices_Should_Apply_Decorator_Order_Across_Assemblies()
    {
        ServiceCollection services = new();
        services.AddAllServices();
        using ServiceProvider provider = services.BuildServiceProvider(Validated);
        using IServiceScope scope = provider.CreateScope();

        string chain = scope.ServiceProvider.GetRequiredService<IFixtureOrdered>().Describe();

        chain.Should().Be("a1(b0(a))");
    }

    [Fact]
    public void AddServicesFromAssemblies_Should_Apply_Decorator_Order_Across_Assemblies()
    {
        ServiceCollection services = new();
        services.AddServicesFromAssemblies(
            typeof(IFixtureOrdered).Assembly,
            Assembly.Load(new AssemblyName("CSharpEssentials.Tests.Fixtures.DependencyInjectionB")));
        using ServiceProvider provider = services.BuildServiceProvider(Validated);
        using IServiceScope scope = provider.CreateScope();

        string chain = scope.ServiceProvider.GetRequiredService<IFixtureOrdered>().Describe();

        chain.Should().Be("a1(b0(a))");
    }

    [Fact]
    public void AddAllServices_Should_Register_Each_Module_Once()
    {
        ServiceCollection services = new();

        services.AddAllServices();

        services.Count(static descriptor => descriptor.ServiceType == typeof(IFixtureGreeter)).Should().Be(1);
        services.Count(static descriptor => descriptor.ServiceType == typeof(IFixtureMessage)).Should().Be(1);
    }

    [Fact]
    public void AddCSharpEssentialsTestsServices_Should_Register_Once_When_Called_Twice()
    {
        ServiceCollection once = new();
        once.AddCSharpEssentialsTestsServices();
        ServiceCollection twice = new();

        twice.AddCSharpEssentialsTestsServices().AddCSharpEssentialsTestsServices();

        twice.Should().HaveCount(once.Count);
        GreetingOf(twice).Should().Be("outer(inner(hi))");
    }

    [Fact]
    public void AddAllServices_Should_Skip_Registry_Already_Added_By_Assembly_Method()
    {
        ServiceCollection services = new();

        services.AddCSharpEssentialsTestsServices();
        services.AddAllServices();

        GreetingOf(services).Should().Be("outer(inner(hi))");
        services.Count(static descriptor => descriptor.ServiceType == typeof(IFixtureGreeter)).Should().Be(1);
    }

    [Fact]
    public void AddCSharpEssentialsTestsServices_Should_Do_Nothing_After_AddAllServices()
    {
        ServiceCollection services = new();
        services.AddAllServices();
        int count = services.Count;

        services.AddCSharpEssentialsTestsServices();

        services.Should().HaveCount(count);
        GreetingOf(services).Should().Be("outer(inner(hi))");
    }

    [Fact]
    public void AddServicesFromAssemblies_Should_Register_Once_When_Called_Twice()
    {
        ServiceCollection once = new();
        once.AddServicesFromAssemblies(typeof(ScanGreeter).Assembly);
        ServiceCollection twice = new();

        twice.AddServicesFromAssemblies(typeof(ScanGreeter).Assembly).AddServicesFromAssemblies(typeof(ScanGreeter).Assembly);

        twice.Should().HaveCount(once.Count);
        GreetingOf(twice).Should().Be("outer(inner(hi))");
    }

    [Fact]
    public void AddServicesFromAssemblies_Should_Skip_Assembly_Registered_By_Generated_Registry()
    {
        ServiceCollection services = new();
        services.AddCSharpEssentialsTestsServices();
        int count = services.Count;

        services.AddServicesFromAssemblies(typeof(ScanGreeter).Assembly);

        services.Should().HaveCount(count);
        GreetingOf(services).Should().Be("outer(inner(hi))");
    }

    private static string GreetingOf(ServiceCollection services)
    {
        using ServiceProvider provider = services.BuildServiceProvider(Validated);
        using IServiceScope scope = provider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<IScanGreeter>().Greet();
    }

    private static (Type ServiceType, object? Key, ServiceLifetime Lifetime, Type? Implementation) Describe(ServiceDescriptor descriptor) =>
        (descriptor.ServiceType,
            descriptor.ServiceKey,
            descriptor.Lifetime,
            descriptor.IsKeyedService ? descriptor.KeyedImplementationType : descriptor.ImplementationType);
}
