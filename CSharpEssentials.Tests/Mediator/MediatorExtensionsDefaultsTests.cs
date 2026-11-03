using CSharpEssentials.Mediator;

using FluentAssertions;

using Mediator;

using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.Mediator;

public sealed class MediatorExtensionsDefaultsTests
{
    [Fact]
    public void DefaultPipelineBehaviors_Should_List_Behaviors_In_Pipeline_Order()
    {
        MediatorExtensions.DefaultPipelineBehaviors.Should().Equal(
            typeof(ValidationBehavior<,>),
            typeof(LoggingBehavior<,>),
            typeof(ExceptionHandlingBehavior<,>),
            typeof(CachingBehavior<,>),
            typeof(TransactionScopeBehavior<,>));
    }

    [Fact]
    public void DefaultPipelineBehaviors_Should_Match_AddMediatorBehaviors_Registration_Order()
    {
        ServiceCollection services = new();

        services.AddMediatorBehaviors();

        services.Where(descriptor => descriptor.ServiceType == typeof(IPipelineBehavior<,>))
            .Select(descriptor => descriptor.ImplementationType)
            .Should().Equal(MediatorExtensions.DefaultPipelineBehaviors);
    }

    [Fact]
    public void AddMediatorValidationOptions_Should_Configure_Existing_Instance()
    {
        ServiceCollection services = new();
        services.AddMediatorValidationOptions();

        services.AddMediatorValidationOptions(options => options.DefaultMode = ValidationMode.LogOnly);

        services.Should().ContainSingle(descriptor => descriptor.ServiceType == typeof(ValidationBehaviorOptions))
            .Which.ImplementationInstance.Should().BeOfType<ValidationBehaviorOptions>()
            .Which.DefaultMode.Should().Be(ValidationMode.LogOnly);
    }

    [Fact]
    public void AddMediatorValidationBehavior_With_Configure_Should_Update_Options_Registered_By_AddMediatorBehaviors()
    {
        ServiceCollection services = new();
        services.AddMediatorBehaviors();

        services.AddMediatorValidationBehavior(options => options.DefaultMode = ValidationMode.Off);

        using ServiceProvider provider = services.BuildServiceProvider();
        provider.GetRequiredService<ValidationBehaviorOptions>().DefaultMode.Should().Be(ValidationMode.Off);
        services.Should().ContainSingle(descriptor => descriptor.ServiceType == typeof(ValidationBehaviorOptions));
    }

    [Fact]
    public void AddMediatorValidationOptions_Should_Ignore_Keyed_Options_Registration()
    {
        ServiceCollection services = new();
        ValidationBehaviorOptions keyed = new() { DefaultMode = ValidationMode.Off };
        services.AddKeyedSingleton("tenant", keyed);

        services.AddMediatorValidationOptions(options => options.DefaultMode = ValidationMode.LogOnly);

        using ServiceProvider provider = services.BuildServiceProvider();
        provider.GetRequiredService<ValidationBehaviorOptions>().DefaultMode.Should().Be(ValidationMode.LogOnly);
        keyed.DefaultMode.Should().Be(ValidationMode.Off);
    }
}
