using Mediator;
using CSharpEssentials.Mediator;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.Mediator;

public class MediatorExtensionsTests
{
    [Fact]
    public void AddMediatorValidationBehavior_Should_Register_ValidationBehavior()
    {
        var services = new ServiceCollection();

        services.AddMediatorValidationBehavior();

        services.Should().ContainSingle(sd => sd.ServiceType == typeof(IPipelineBehavior<,>)
            && sd.ImplementationType == typeof(ValidationBehavior<,>));
    }

    [Fact]
    public void AddMediatorLoggingBehavior_Should_Register_LoggingBehavior()
    {
        var services = new ServiceCollection();

        services.AddMediatorLoggingBehavior();

        services.Should().ContainSingle(sd => sd.ServiceType == typeof(IPipelineBehavior<,>)
            && sd.ImplementationType == typeof(LoggingBehavior<,>));
    }

    [Fact]
    public void AddMediatorExceptionHandlingBehavior_Should_Register_ExceptionHandlingBehavior()
    {
        var services = new ServiceCollection();

        services.AddMediatorExceptionHandlingBehavior();

        services.Should().ContainSingle(sd => sd.ServiceType == typeof(IPipelineBehavior<,>)
            && sd.ImplementationType == typeof(ExceptionHandlingBehavior<,>)
            && sd.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddMediatorCachingBehavior_Should_Register_CachingBehavior()
    {
        var services = new ServiceCollection();

        services.AddMediatorCachingBehavior();

        services.Should().ContainSingle(sd => sd.ServiceType == typeof(IPipelineBehavior<,>)
            && sd.ImplementationType == typeof(CachingBehavior<,>));
    }

    [Fact]
    public void AddMediatorTransactionBehavior_Should_Register_TransactionScopeBehavior()
    {
        var services = new ServiceCollection();

        services.AddMediatorTransactionBehavior();

        services.Should().ContainSingle(sd => sd.ServiceType == typeof(IPipelineBehavior<,>)
            && sd.ImplementationType == typeof(TransactionScopeBehavior<,>));
    }

    [Fact]
    public void AddMediatorBehaviors_Should_Register_All_Behaviors()
    {
        var services = new ServiceCollection();

        services.AddMediatorBehaviors();

        services.Should().Contain(sd => sd.ImplementationType == typeof(ValidationBehavior<,>));
        services.Should().Contain(sd => sd.ImplementationType == typeof(LoggingBehavior<,>));
        services.Should().Contain(sd => sd.ImplementationType == typeof(ExceptionHandlingBehavior<,>));
        services.Should().Contain(sd => sd.ImplementationType == typeof(CachingBehavior<,>));
        services.Should().Contain(sd => sd.ImplementationType == typeof(TransactionScopeBehavior<,>));
    }

    [Fact]
    public void AddMediatorValidationOptions_Should_Reuse_Options_Instance_When_Called_Twice()
    {
        var services = new ServiceCollection();

        services.AddMediatorValidationOptions(options => options.DefaultMode = ValidationMode.LogOnly);
        services.AddMediatorValidationOptions();

        ServiceDescriptor descriptor = services.Should().ContainSingle(sd => sd.ServiceType == typeof(ValidationBehaviorOptions)).Subject;
        descriptor.ImplementationInstance.Should().BeOfType<ValidationBehaviorOptions>()
            .Which.DefaultMode.Should().Be(ValidationMode.LogOnly);
    }

    [Fact]
    public void AddMediatorValidationOptions_Should_Register_Logging_Observer_Once()
    {
        var services = new ServiceCollection();

        services.AddMediatorValidationBehavior();
        services.AddMediatorValidationOptions();

        services.Should().ContainSingle(sd => sd.ServiceType == typeof(IValidationFailureObserver)
            && sd.ImplementationType == typeof(LoggingValidationFailureObserver));
    }

    [Fact]
    public void AddMediatorValidationBehavior_Should_Apply_Configure_Delegate()
    {
        var services = new ServiceCollection();

        services.AddMediatorValidationBehavior(options => options.DefaultMode = ValidationMode.Off);

        using ServiceProvider provider = services.BuildServiceProvider();
        provider.GetRequiredService<ValidationBehaviorOptions>().DefaultMode.Should().Be(ValidationMode.Off);
        services.Should().ContainSingle(sd => sd.ImplementationType == typeof(ValidationBehavior<,>));
    }

    [Fact]
    public void AddMediatorValidationOptions_Should_Keep_Factory_Registration_When_No_Configure_Delegate()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_ => new ValidationBehaviorOptions { DefaultMode = ValidationMode.Off });

        services.AddMediatorValidationBehavior();

        using ServiceProvider provider = services.BuildServiceProvider();
        provider.GetRequiredService<ValidationBehaviorOptions>().DefaultMode.Should().Be(ValidationMode.Off);
        services.Should().ContainSingle(sd => sd.ServiceType == typeof(ValidationBehaviorOptions));
    }

    [Fact]
    public void AddMediatorValidationOptions_Should_Throw_When_Configuring_Factory_Registration()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_ => new ValidationBehaviorOptions());

        Action act = () => services.AddMediatorValidationOptions(options => options.DefaultMode = ValidationMode.LogOnly);

        act.Should().Throw<InvalidOperationException>();
    }
}
