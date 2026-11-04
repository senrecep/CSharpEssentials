using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Extensions;
using CSharpEssentials.Validation.Validators;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.Validation;

public class ValidatorRegistrationTests
{
    private sealed record Model(string? Name);

    private sealed class ModelValidator : Validator<Model>
    {
        protected override ValueTask Configure(Model model, RuleContext<Model> rules, CancellationToken ct = default)
        {
            rules.For(() => model.Name).NotEmpty();
            return ValueTask.CompletedTask;
        }
    }

    private sealed class OtherModelValidator : Validator<Model>
    {
        protected override ValueTask Configure(Model model, RuleContext<Model> rules, CancellationToken ct = default)
        {
            rules.For(() => model.Name).MaxLength(10);
            return ValueTask.CompletedTask;
        }
    }

    [Fact]
    public void AddValidator_ShouldRegisterValidator()
    {
        ServiceCollection services = new();

        services.AddValidator<Model, ModelValidator>();

        using ServiceProvider provider = services.BuildServiceProvider();
        provider.GetService<IValidator<Model>>().Should().BeOfType<ModelValidator>();
    }

    [Fact]
    public void AddValidator_ShouldNotDuplicate_WhenSameImplementationRegisteredTwice()
    {
        ServiceCollection services = new();

        services.AddValidator<Model, ModelValidator>();
        services.AddValidator<Model, ModelValidator>();

        services.Count(d => d.ServiceType == typeof(IValidator<Model>)).Should().Be(1);
    }

    [Fact]
    public void AddValidator_ShouldKeepBoth_WhenDifferentImplementationsRegistered()
    {
        ServiceCollection services = new();

        services.AddValidator<Model, ModelValidator>();
        services.AddValidator<Model, OtherModelValidator>();

        services.Count(d => d.ServiceType == typeof(IValidator<Model>)).Should().Be(2);
    }

    [Fact]
    public void AddValidatorsFromAssembly_ShouldNotDuplicate_WhenAssemblyScannedTwice()
    {
        ServiceCollection services = new();

        services.AddValidatorsFromAssembly(typeof(ModelValidator).Assembly);
        services.AddValidatorsFromAssembly(typeof(ModelValidator).Assembly);

        services.Count(d =>
            d.ServiceType == typeof(IValidator<Model>)
            && d.ImplementationType == typeof(ModelValidator)).Should().Be(1);
    }

    [Fact]
    public void AddValidatorsFromAssemblies_ShouldRegisterValidators_WithRequestedLifetime()
    {
        ServiceCollection services = new();

        services.AddValidatorsFromAssemblies([typeof(ModelValidator).Assembly], ServiceLifetime.Singleton);

        services.Should().Contain(d =>
            d.ServiceType == typeof(IValidator<Model>)
            && d.ImplementationType == typeof(ModelValidator)
            && d.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddValidator_TValidator_ShouldKeepPublicConstructorsForTrimming()
    {
        MethodInfo method = typeof(ValidatorServiceCollectionExtensions).GetMethod(nameof(ValidatorServiceCollectionExtensions.AddValidator))!;

        DynamicallyAccessedMembersAttribute? attribute = method.GetGenericArguments()[1].GetCustomAttribute<DynamicallyAccessedMembersAttribute>();

        attribute.Should().NotBeNull();
        attribute.MemberTypes.Should().HaveFlag(DynamicallyAccessedMemberTypes.PublicConstructors);
    }

    [Theory]
    [InlineData(nameof(ValidatorServiceCollectionExtensions.AddValidatorsFromAssembly))]
    [InlineData(nameof(ValidatorServiceCollectionExtensions.AddValidatorsFromAssemblies))]
    public void ScanningApis_ShouldRequireUnreferencedCode(string methodName)
    {
        MethodInfo method = typeof(ValidatorServiceCollectionExtensions).GetMethod(methodName)!;

        RequiresUnreferencedCodeAttribute? attribute = method.GetCustomAttribute<RequiresUnreferencedCodeAttribute>();

        attribute.Should().NotBeNull();
        attribute.Message.Should().Contain("AddValidator<T, TValidator>()");
    }
}
