using CSharpEssentials.Locking;
using CSharpEssentials.Mediator;

using FluentAssertions;

using Mediator;

using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.Mediator;

public class LockBehaviorRegistrationTests
{
    private static List<Type?> Behaviors(IServiceCollection services) =>
        [.. services.Where(sd => sd.ServiceType == typeof(IPipelineBehavior<,>)).Select(sd => sd.ImplementationType)];

    [Fact]
    public void AddMediatorLockBehavior_Should_Place_Lock_Outside_Transaction_By_Default()
    {
        var services = new ServiceCollection();
        services.AddMediatorBehaviors();

        services.AddMediatorLockBehavior();

        List<Type?> behaviors = Behaviors(services);
        behaviors.IndexOf(typeof(LockBehavior<,>)).Should().Be(behaviors.IndexOf(typeof(TransactionScopeBehavior<,>)) - 1);
        services.Single(sd => sd.ImplementationType == typeof(LockBehavior<,>)).Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddMediatorLockBehavior_Should_Place_Lock_Inside_Transaction_When_Requested()
    {
        var services = new ServiceCollection();
        services.AddMediatorBehaviors();
        services.AddMediatorTransactionRunnerBehavior();

        services.AddMediatorLockBehavior(LockPlacement.InsideTransaction);

        List<Type?> behaviors = Behaviors(services);
        behaviors.IndexOf(typeof(LockBehavior<,>)).Should().Be(behaviors.IndexOf(typeof(TransactionBehavior<,>)) + 1);
    }

    [Fact]
    public void Switching_Transaction_Behavior_Should_Keep_Lock_Inside()
    {
        var services = new ServiceCollection();
        services.AddMediatorBehaviors();
        services.AddMediatorLockBehavior(LockPlacement.InsideTransaction);

        services.AddMediatorTransactionRunnerBehavior();

        List<Type?> behaviors = Behaviors(services);
        behaviors.IndexOf(typeof(LockBehavior<,>)).Should().Be(behaviors.IndexOf(typeof(TransactionBehavior<,>)) + 1);
    }

    [Fact]
    public void AddMediatorLockBehavior_Should_Move_Instead_Of_Duplicating()
    {
        var services = new ServiceCollection();
        services.AddMediatorBehaviors();
        services.AddMediatorLockBehavior(LockPlacement.InsideTransaction);

        services.AddMediatorLockBehavior();

        List<Type?> behaviors = Behaviors(services);
        behaviors.Count(t => t == typeof(LockBehavior<,>)).Should().Be(1);
        behaviors.IndexOf(typeof(LockBehavior<,>)).Should().Be(behaviors.IndexOf(typeof(TransactionScopeBehavior<,>)) - 1);
    }

    [Fact]
    public void AddMediatorLockBehavior_Should_Append_When_No_Transaction_Behavior()
    {
        var services = new ServiceCollection();
        services.AddMediatorLoggingBehavior();

        services.AddMediatorLockBehavior();

        Behaviors(services).Should().Equal(typeof(LoggingBehavior<,>), typeof(LockBehavior<,>));
    }

    [Fact]
    public void AddMediatorLockBehavior_Should_Throw_Inside_Without_Transaction_Behavior()
    {
        var services = new ServiceCollection();

        Action act = () => services.AddMediatorLockBehavior(LockPlacement.InsideTransaction);

        act.Should().Throw<InvalidOperationException>().WithMessage("*transaction behavior*");
    }

    [Fact]
    public void AddMediatorLockBehavior_Should_Register_InProcess_Lock_Unless_One_Exists()
    {
        var defaults = new ServiceCollection();
        defaults.AddMediatorLockBehavior();
        var custom = new ServiceCollection();
        custom.AddScoped<IResourceLock, FakeResourceLock>();
        custom.AddMediatorLockBehavior();

        defaults.Should().ContainSingle(sd => sd.ServiceType == typeof(IResourceLock)
            && sd.ImplementationType == typeof(InProcessResourceLock) && sd.Lifetime == ServiceLifetime.Singleton);
        custom.Should().ContainSingle(sd => sd.ServiceType == typeof(IResourceLock))
            .Which.ImplementationType.Should().Be<FakeResourceLock>();
    }
}
