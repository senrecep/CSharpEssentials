using System.Reflection;
using System.Reflection.Emit;
using CSharpEssentials.DependencyInjection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Tests.DependencyInjection.Scanning;

public class ServiceCollectionRegistrationExtensionsTests
{
    private static readonly Assembly TestAssembly = typeof(ScanGreeter).Assembly;

    private static ServiceProvider BuildScanned(Action<IServiceCollection>? configure = null)
    {
        ServiceCollection services = new();
        configure?.Invoke(services);
        services.AddServicesFromAssemblies(TestAssembly);
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
    }

    private static AssemblyBuilder CreateAssembly(TypeAttributes attributes, ConstructorInfo attributeConstructor, params object[] arguments)
    {
        string name = "DynamicRegistrations" + Guid.NewGuid().ToString("N");
        AssemblyBuilder assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(name), AssemblyBuilderAccess.RunAndCollect);
        ModuleBuilder module = assembly.DefineDynamicModule(name);
        TypeBuilder type = module.DefineType("Invalid", TypeAttributes.Public | TypeAttributes.Class | attributes);
        if (!attributes.HasFlag(TypeAttributes.Abstract))
        {
            type.DefineDefaultConstructor(MethodAttributes.Public);
        }

        type.SetCustomAttribute(new CustomAttributeBuilder(attributeConstructor, arguments));
        type.CreateType();
        return assembly;
    }

    [Fact]
    public void AddServicesFromAssemblies_Should_RegisterConventionalInterface_When_NoServiceTypeGiven()
    {
        using ServiceProvider provider = BuildScanned();
        using IServiceScope scope = provider.CreateScope();

        scope.ServiceProvider.GetService<IConventionalService>().Should().BeOfType<ConventionalService>();
        scope.ServiceProvider.GetService<IOtherMarker>().Should().BeNull();
    }

    [Fact]
    public void AddServicesFromAssemblies_Should_RegisterSelf_When_NoConventionalInterfaceExists()
    {
        using ServiceProvider provider = BuildScanned();

        provider.GetService<SelfOnlyService>().Should().NotBeNull();
    }

    [Fact]
    public void AddServicesFromAssemblies_Should_UseExplicitServiceTypeAndKey()
    {
        using ServiceProvider provider = BuildScanned();
        using IServiceScope scope = provider.CreateScope();

        scope.ServiceProvider.GetRequiredKeyedService<IOtherMarker>("explicit").Should().BeOfType<ExplicitService>();
    }

    [Fact]
    public void AddServicesFromAssemblies_Should_ReadGenericAttributeWithEnumKey()
    {
        using ServiceProvider provider = BuildScanned();

        provider.GetRequiredKeyedService<IGenericAttributeService>(ScanRegion.Eu).Should().BeOfType<GenericAttributeService>();
        provider.GetKeyedService<IGenericAttributeService>(1).Should().BeNull();
    }

    [Fact]
    public void AddServicesFromAssemblies_Should_ShareInstance_When_SelfWithInterfaces()
    {
        using ServiceProvider provider = BuildScanned();

        SharedService self = provider.GetRequiredService<SharedService>();

        provider.GetRequiredService<ISharedService>().Should().BeSameAs(self);
        provider.GetRequiredService<ISecondShared>().Should().BeSameAs(self);
    }

    [Fact]
    public void AddServicesFromAssemblies_Should_ForwardKeyed_When_SelfWithInterfacesHasKey()
    {
        using ServiceProvider provider = BuildScanned();
        using IServiceScope scope = provider.CreateScope();

        KeyedSharedService self = scope.ServiceProvider.GetRequiredKeyedService<KeyedSharedService>("shared");

        scope.ServiceProvider.GetRequiredKeyedService<IKeyedSharedService>("shared").Should().BeSameAs(self);
    }

    [Fact]
    public void AddServicesFromAssemblies_Should_RegisterEachInterfaceExceptExcluded_When_ImplementedInterfaces()
    {
        ServiceCollection services = new();

        services.AddServicesFromAssemblies(TestAssembly);

        services.Where(d => d.ImplementationType == typeof(SplitService)).Select(d => d.ServiceType)
            .Should().BeEquivalentTo([typeof(ISecondSplit), typeof(ISplitService)]);
    }

    [Fact]
    public void AddServicesFromAssemblies_Should_RegisterOnlySelf_When_AsSelfIsExplicit()
    {
        ServiceCollection services = new();

        services.AddServicesFromAssemblies(TestAssembly);

        services.Where(d => d.ImplementationType == typeof(SelfExplicitService)).Select(d => d.ServiceType)
            .Should().Equal(typeof(SelfExplicitService));
    }

    [Fact]
    public void AddServicesFromAssemblies_Should_RegisterOpenGenerics()
    {
        using ServiceProvider provider = BuildScanned();
        using IServiceScope scope = provider.CreateScope();

        scope.ServiceProvider.GetRequiredService<IScanRepository<int>>().Should().BeOfType<ScanRepository<int>>();
        scope.ServiceProvider.GetRequiredKeyedService<IKeyedRepository<int, string>>(42).Should().BeOfType<KeyedRepository<int, string>>();
    }

    [Fact]
    public void AddServicesFromAssemblies_Should_ApplyStrategies()
    {
        using ServiceProvider provider = BuildScanned();
        using IServiceScope scope = provider.CreateScope();

        provider.GetServices<IScanFormatter>().Should().ContainSingle().Which.Should().BeOfType<DefaultFormatter>();
        provider.GetRequiredKeyedService<IScanFormatter>(typeof(int)).Should().BeOfType<IntFormatter>();
        scope.ServiceProvider.GetServices<IScanReplaceable>().Should().ContainSingle().Which.Should().BeOfType<ReplacingReplaceable>();
    }

    [Fact]
    public void AddServicesFromAssemblies_Should_Throw_When_ThrowStrategyMeetsExistingRegistration()
    {
        ServiceCollection services = new();
        services.AddScoped<IThrowingService, ThrowingService>();

        Action act = () => services.AddServicesFromAssemblies(TestAssembly);

        act.Should().Throw<InvalidOperationException>().WithMessage($"*{typeof(IThrowingService).FullName}*");
    }

    [Fact]
    public void AddServicesFromAssemblies_Should_RegisterEveryAttribute_When_AttributeRepeated()
    {
        using ServiceProvider provider = BuildScanned();
        using IServiceScope scope = provider.CreateScope();

        scope.ServiceProvider.GetRequiredKeyedService<IMultiService>("a").Should().BeOfType<MultiService>();
        provider.GetRequiredKeyedService<IMultiService>("b").Should().BeOfType<MultiService>();
    }

    [Fact]
    public void AddServicesFromAssemblies_Should_SkipExcludedTypes()
    {
        ServiceCollection services = new();

        services.AddServicesFromAssemblies(TestAssembly);

        services.Should().NotContain(d => d.ServiceType == typeof(IExcludedService));
    }

    [Fact]
    public void AddServicesFromAssemblies_Should_ApplyDecoratorsLastByOrder()
    {
        using ServiceProvider provider = BuildScanned();
        using IServiceScope scope = provider.CreateScope();

        scope.ServiceProvider.GetRequiredService<IScanGreeter>().Greet().Should().Be("outer(inner(hi))");
        scope.ServiceProvider.GetRequiredKeyedService<IScanGreeter>("k").Greet().Should().Be("keyed-decorator(keyed)");
    }

    [Fact]
    public void AddServicesFromAssemblies_Should_DecorateManualRegistrationsToo()
    {
        using ServiceProvider provider = BuildScanned(services => services.AddScoped<IScanGreeter, KeyedScanGreeter>());
        using IServiceScope scope = provider.CreateScope();

        scope.ServiceProvider.GetServices<IScanGreeter>().Select(g => g.Greet())
            .Should().Equal("outer(inner(keyed))", "outer(inner(hi))");
    }

    [Fact]
    public void AddServicesFromAssemblies_Should_KeepBothAndResolveLast_When_ManualRegistrationCoexists()
    {
        ServiceCollection services = new();
        services.AddScoped<IConventionalService, ConventionalService>();
        services.AddServicesFromAssemblies(TestAssembly);
        services.AddScoped<IConventionalService>(_ => new ConventionalService());
        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();

        IConventionalService[] all = [.. scope.ServiceProvider.GetServices<IConventionalService>()];

        all.Should().HaveCount(3);
        scope.ServiceProvider.GetRequiredService<IConventionalService>().Should().BeSameAs(all[^1]);
    }

    [Fact]
    public void AddServicesFromAssemblies_Should_LogDuplicatesAtDebug_When_LoggerGiven()
    {
        ServiceCollection services = new();
        CapturingLogger logger = new();

        services.AddServicesFromAssemblies(logger, TestAssembly);

        logger.Entries.Should().Contain(entry =>
            entry.EventId.Id == 2001 && entry.Message.Contains(nameof(DefaultFormatter)) && entry.Message.Contains(nameof(TryFormatter)));
    }

    [Fact]
    public void AddServicesFromAssemblies_Should_ThrowListingLoaderErrors_When_TypesFailToLoadWithoutLogger()
    {
        ServiceCollection services = new();
        AssemblyBuilder assembly = PartiallyLoadableAssembly.Create("DependencyInjectionPartial");

        Action act = () => services.AddServicesFromAssemblies(assembly);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*DependencyInjectionPartial*could not be loaded*")
            .WithInnerException<ReflectionTypeLoadException>();
        services.Should().BeEmpty();
    }

    [Fact]
    public void AddServicesFromAssemblies_Should_LogWarningAndContinue_When_TypesFailToLoadWithLogger()
    {
        ServiceCollection services = new();
        CapturingLogger logger = new();
        AssemblyBuilder assembly = PartiallyLoadableAssembly.Create("DependencyInjectionPartialLogged");

        services.AddServicesFromAssemblies(logger, assembly, TestAssembly);

        logger.Entries.Should().Contain(entry =>
            entry.Level == LogLevel.Warning && entry.EventId.Id == 2002 && entry.Message.Contains("DependencyInjectionPartialLogged"));
        services.Should().Contain(d => d.ServiceType == typeof(IScanGreeter));
    }

    [Fact]
    public void AddServicesFromAssemblies_Should_ThrowWithoutMutation_When_ServiceTypeNotImplemented()
    {
        ServiceCollection services = new();
        Assembly assembly = CreateAssembly(
            TypeAttributes.Sealed,
            typeof(RegisterScopedAttribute).GetConstructor([typeof(Type)])!,
            typeof(IScanGreeter));

        Action act = () => services.AddServicesFromAssemblies(TestAssembly, assembly);

        act.Should().Throw<InvalidOperationException>().WithMessage($"*Invalid*{typeof(IScanGreeter).FullName}*does not implement*");
        services.Should().BeEmpty();
    }

    [Fact]
    public void AddServicesFromAssemblies_Should_ThrowWithoutMutation_When_TypeIsAbstract()
    {
        ServiceCollection services = new();
        Assembly assembly = CreateAssembly(TypeAttributes.Abstract, typeof(RegisterSingletonAttribute).GetConstructor(Type.EmptyTypes)!);

        Action act = () => services.AddServicesFromAssemblies(TestAssembly, assembly);

        act.Should().Throw<InvalidOperationException>().WithMessage("*Invalid*abstract*");
        services.Should().BeEmpty();
    }

    [Fact]
    public void AddServicesFromAssemblies_Should_ThrowWithoutMutation_When_DecoratorDoesNotImplementService()
    {
        ServiceCollection services = new();
        Assembly assembly = CreateAssembly(
            TypeAttributes.Sealed,
            typeof(DecoratesAttribute).GetConstructor([typeof(Type)])!,
            typeof(IScanGreeter));

        Action act = () => services.AddServicesFromAssemblies(TestAssembly, assembly);

        act.Should().Throw<InvalidOperationException>().WithMessage("*Invalid*does not implement*");
        services.Should().BeEmpty();
    }

    [Fact]
    public void AddServicesFromAssemblies_Should_SkipAssembly_When_AssemblyIsExcluded()
    {
        string name = "ExcludedRegistrations" + Guid.NewGuid().ToString("N");
        AssemblyBuilder assembly = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName(name),
            AssemblyBuilderAccess.RunAndCollect,
            [new CustomAttributeBuilder(typeof(ExcludeFromRegistrationAttribute).GetConstructor(Type.EmptyTypes)!, [])]);
        TypeBuilder type = assembly.DefineDynamicModule(name).DefineType("Invalid", TypeAttributes.Public | TypeAttributes.Abstract);
        type.SetCustomAttribute(new CustomAttributeBuilder(typeof(RegisterScopedAttribute).GetConstructor(Type.EmptyTypes)!, []));
        type.CreateType();
        ServiceCollection services = new();

        services.AddServicesFromAssemblies(assembly);

        services.Should().BeEmpty();
    }

    [Fact]
    public void AddServicesFromAssemblies_Should_Throw_When_ArgumentsAreNull()
    {
        ServiceCollection services = new();

        Action nullServices = () => ((IServiceCollection)null!).AddServicesFromAssemblies(TestAssembly);
        Action nullAssemblies = () => services.AddServicesFromAssemblies((Assembly[])null!);
        Action nullAssembly = () => services.AddServicesFromAssemblies(TestAssembly, null!);

        nullServices.Should().Throw<ArgumentNullException>().WithParameterName("services");
        nullAssemblies.Should().Throw<ArgumentNullException>();
        nullAssembly.Should().Throw<ArgumentNullException>().WithParameterName("assembly");
    }
}
