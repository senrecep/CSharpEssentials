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
}
