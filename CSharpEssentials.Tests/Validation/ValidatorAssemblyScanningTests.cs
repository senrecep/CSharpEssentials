using System.Reflection;
using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Extensions;
using CSharpEssentials.Validation.Validators;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.Validation;

public class ValidatorAssemblyScanningTests
{
    private sealed record ScanModel(string? Name);

    private sealed class ScanModelValidator : Validator<ScanModel>
    {
        protected override ValueTask Configure(ScanModel model, RuleContext<ScanModel> rules, CancellationToken ct = default)
        {
            rules.For(() => model.Name).NotEmpty();
            return ValueTask.CompletedTask;
        }
    }

    private abstract class AbstractScanModelValidator : Validator<ScanModel>;

    private sealed class OpenGenericValidator<T> : Validator<T>
    {
        protected override ValueTask Configure(T model, RuleContext<T> rules, CancellationToken ct = default) =>
            ValueTask.CompletedTask;
    }

    private static readonly Assembly TestAssembly = typeof(ScanModelValidator).Assembly;

    [Fact]
    public void AddValidatorsFromAssemblies_ShouldRegisterValidatorsOnce_WhenAssemblyListedTwice()
    {
        ServiceCollection services = new();

        services.AddValidatorsFromAssemblies([TestAssembly, TestAssembly], ServiceLifetime.Singleton);

        services.Should().ContainSingle(d => d.ServiceType == typeof(IValidator<ScanModel>))
            .Which.Should().Match<ServiceDescriptor>(d =>
                d.ImplementationType == typeof(ScanModelValidator) && d.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddValidatorsFromAssemblies_ShouldRegisterNothing_WhenListIsEmpty()
    {
        ServiceCollection services = new();

        services.AddValidatorsFromAssemblies([]);

        services.Should().BeEmpty();
    }

    [Fact]
    public void AddValidatorsFromAssembly_ShouldSkipAbstractValidators()
    {
        ServiceCollection services = new();

        services.AddValidatorsFromAssembly(TestAssembly);

        services.Select(d => d.ImplementationType).Should().NotContain(typeof(AbstractScanModelValidator));
    }

    [Fact]
    public void AddValidatorsFromAssembly_ShouldNotThrow_WhenAssemblyContainsOpenGenericValidator()
    {
        ServiceCollection services = new();

        Action act = () => services.AddValidatorsFromAssembly(TestAssembly);

        act.Should().NotThrow();
        services.Select(d => d.ImplementationType).Should().NotContain(typeof(OpenGenericValidator<>));
    }

    [Fact]
    public void AddValidatorsFromAssembly_ShouldResolveValidator_WithDefaultScopedLifetime()
    {
        ServiceCollection services = new();

        services.AddValidatorsFromAssembly(TestAssembly);

        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();
        scope.ServiceProvider.GetServices<IValidator<ScanModel>>().Should().ContainSingle()
            .Which.Should().BeOfType<ScanModelValidator>();
        services.Single(d => d.ServiceType == typeof(IValidator<ScanModel>)).Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddValidator_ShouldThrow_WhenServicesIsNull()
    {
        Action act = () => ValidatorServiceCollectionExtensions.AddValidator<ScanModel, ScanModelValidator>(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("services");
    }

    [Fact]
    public void AddValidatorsFromAssembly_ShouldThrow_WhenServicesIsNull()
    {
        Action act = () => ValidatorServiceCollectionExtensions.AddValidatorsFromAssembly(null!, TestAssembly);

        act.Should().Throw<ArgumentNullException>().WithParameterName("services");
    }

    [Fact]
    public void AddValidatorsFromAssembly_ShouldThrow_WhenAssemblyIsNull()
    {
        Action act = () => new ServiceCollection().AddValidatorsFromAssembly(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("assembly");
    }

    [Fact]
    public void AddValidatorsFromAssemblies_ShouldThrow_WhenServicesIsNull()
    {
        Action act = () => ValidatorServiceCollectionExtensions.AddValidatorsFromAssemblies(null!, [TestAssembly]);

        act.Should().Throw<ArgumentNullException>().WithParameterName("services");
    }

    [Fact]
    public void AddValidatorsFromAssemblies_ShouldThrow_WhenAssembliesIsNull()
    {
        Action act = () => new ServiceCollection().AddValidatorsFromAssemblies(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("assemblies");
    }
}
