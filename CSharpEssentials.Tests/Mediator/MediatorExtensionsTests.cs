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
        services.Should().Contain(sd => sd.ImplementationType == typeof(CachingBehavior<,>));
        services.Should().Contain(sd => sd.ImplementationType == typeof(TransactionScopeBehavior<,>));
    }
}
