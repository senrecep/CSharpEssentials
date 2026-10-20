using CSharpEssentials.Idempotency;
using CSharpEssentials.Locking;
using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;

using FluentAssertions;

using Mediator;

using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.Mediator;

public class IdempotencyBehaviorRegistrationTests
{
    private static List<Type?> Behaviors(IServiceCollection services) =>
        [.. services.Where(sd => sd.ServiceType == typeof(IPipelineBehavior<,>)).Select(sd => sd.ImplementationType)];

    private static List<Type?> Tail(IServiceCollection services, int count)
    {
        List<Type?> behaviors = Behaviors(services);
        return behaviors.GetRange(behaviors.Count - count, count);
    }

    [Fact]
    public void AddMediatorIdempotencyBehavior_Should_Place_It_Right_Outside_The_Transaction_By_Default()
    {
        var services = new ServiceCollection();
        services.AddMediatorBehaviors();

        services.AddMediatorIdempotencyBehavior();

        Tail(services, 2).Should().Equal(typeof(IdempotencyBehavior<,>), typeof(TransactionScopeBehavior<,>));
        services.Single(sd => sd.ImplementationType == typeof(IdempotencyBehavior<,>)).Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddMediatorIdempotencyBehavior_Should_Run_Before_An_Outside_Lock_Registered_First()
    {
        var services = new ServiceCollection();
        services.AddMediatorBehaviors();
        services.AddMediatorLockBehavior();

        services.AddMediatorIdempotencyBehavior();

        Tail(services, 3).Should().Equal(typeof(IdempotencyBehavior<,>), typeof(LockBehavior<,>), typeof(TransactionScopeBehavior<,>));
    }

    [Fact]
    public void AddMediatorLockBehavior_Should_Run_After_An_Outside_Idempotency_Behavior_Registered_First()
    {
        var services = new ServiceCollection();
        services.AddMediatorBehaviors();
        services.AddMediatorIdempotencyBehavior();

        services.AddMediatorLockBehavior();

        Tail(services, 3).Should().Equal(typeof(IdempotencyBehavior<,>), typeof(LockBehavior<,>), typeof(TransactionScopeBehavior<,>));
    }

    [Fact]
    public void AddMediatorIdempotencyBehavior_Should_Run_Before_An_Inside_Lock_When_Outside()
    {
        var services = new ServiceCollection();
        services.AddMediatorBehaviors();
        services.AddMediatorLockBehavior(LockPlacement.InsideTransaction);

        services.AddMediatorIdempotencyBehavior();

        Tail(services, 3).Should().Equal(typeof(IdempotencyBehavior<,>), typeof(TransactionScopeBehavior<,>), typeof(LockBehavior<,>));
    }

    [Fact]
    public void AddMediatorIdempotencyBehavior_Should_Place_It_Right_Inside_The_Transaction_When_Requested()
    {
        var services = new ServiceCollection();
        services.AddMediatorBehaviors();

        services.AddMediatorIdempotencyBehavior(options => options.Placement = IdempotencyPlacement.InsideTransaction);

        Tail(services, 2).Should().Equal(typeof(TransactionScopeBehavior<,>), typeof(IdempotencyBehavior<,>));
    }

    [Fact]
    public void Inside_Idempotency_Should_Run_Before_An_Inside_Lock_Registered_Later()
    {
        var services = new ServiceCollection();
        services.AddMediatorBehaviors();
        services.AddMediatorIdempotencyBehavior(options => options.Placement = IdempotencyPlacement.InsideTransaction);

        services.AddMediatorLockBehavior(LockPlacement.InsideTransaction);

        Tail(services, 3).Should().Equal(typeof(TransactionScopeBehavior<,>), typeof(IdempotencyBehavior<,>), typeof(LockBehavior<,>));
    }

    [Fact]
    public void Inside_Idempotency_Should_Run_Before_An_Inside_Lock_Registered_Earlier()
    {
        var services = new ServiceCollection();
        services.AddMediatorBehaviors();
        services.AddMediatorLockBehavior(LockPlacement.InsideTransaction);

        services.AddMediatorIdempotencyBehavior(options => options.Placement = IdempotencyPlacement.InsideTransaction);

        Tail(services, 3).Should().Equal(typeof(TransactionScopeBehavior<,>), typeof(IdempotencyBehavior<,>), typeof(LockBehavior<,>));
    }

    [Fact]
    public void AddMediatorIdempotencyBehavior_Should_Throw_Inside_Without_A_Transaction_Behavior()
    {
        var services = new ServiceCollection();
        services.AddMediatorLoggingBehavior();

        Action act = () => services.AddMediatorIdempotencyBehavior(options => options.Placement = IdempotencyPlacement.InsideTransaction);

        act.Should().Throw<InvalidOperationException>().WithMessage("*InsideTransaction*");
    }

    [Fact]
    public void AddMediatorIdempotencyBehavior_Should_Append_When_No_Transaction_Behavior()
    {
        var services = new ServiceCollection();
        services.AddMediatorLoggingBehavior();

        services.AddMediatorIdempotencyBehavior();

        Behaviors(services).Should().Equal(typeof(LoggingBehavior<,>), typeof(IdempotencyBehavior<,>));
    }

    [Fact]
    public void Switching_Transaction_Behavior_Should_Keep_Idempotency_Inside()
    {
        var services = new ServiceCollection();
        services.AddMediatorBehaviors();
        services.AddMediatorIdempotencyBehavior(options => options.Placement = IdempotencyPlacement.InsideTransaction);

        services.AddMediatorTransactionRunnerBehavior();

        Tail(services, 2).Should().Equal(typeof(TransactionBehavior<,>), typeof(IdempotencyBehavior<,>));
    }

    [Fact]
    public void AddMediatorIdempotencyBehavior_Should_Move_Instead_Of_Duplicating_And_Configure_The_Same_Options()
    {
        var services = new ServiceCollection();
        services.AddMediatorBehaviors();
        services.AddMediatorIdempotencyBehavior(options =>
        {
            options.Placement = IdempotencyPlacement.InsideTransaction;
            options.StoreFailures = false;
        });

        services.AddMediatorIdempotencyBehavior(options => options.Placement = IdempotencyPlacement.OutsideTransaction);

        Behaviors(services).Count(type => type == typeof(IdempotencyBehavior<,>)).Should().Be(1);
        Tail(services, 2).Should().Equal(typeof(IdempotencyBehavior<,>), typeof(TransactionScopeBehavior<,>));
        IdempotencyBehaviorOptions options = services.Where(sd => sd.ServiceType == typeof(IdempotencyBehaviorOptions))
            .Should().ContainSingle().Which.ImplementationInstance.Should().BeOfType<IdempotencyBehaviorOptions>().Subject;
        options.StoreFailures.Should().BeFalse();
    }

    [Fact]
    public void AddMediatorIdempotencyBehavior_Should_Reject_Options_Registered_With_A_Factory()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_ => new IdempotencyBehaviorOptions());

        Action act = () => services.AddMediatorIdempotencyBehavior();

        act.Should().Throw<InvalidOperationException>().WithMessage("*IdempotencyBehaviorOptions*");
    }

    [Fact]
    public void AddMediatorIdempotencyBehavior_Should_Reject_An_Undefined_Placement()
    {
        var services = new ServiceCollection();

        Action act = () => services.AddMediatorIdempotencyBehavior(options => options.Placement = (IdempotencyPlacement)7);

        act.Should().Throw<InvalidOperationException>().WithMessage("*Placement*");
    }

    [Fact]
    public void AddMediatorIdempotencyBehavior_Should_Reject_An_Undefined_Unscoped_Key_Behavior()
    {
        var services = new ServiceCollection();

        Action act = () => services.AddMediatorIdempotencyBehavior(options => options.UnscopedKeyBehavior = (UnscopedKeyBehavior)7);

        act.Should().Throw<InvalidOperationException>().WithMessage("*UnscopedKeyBehavior*");
    }

    [Fact]
    public void AddMediatorIdempotencyBehavior_Should_Reject_A_Null_Scope_Resolver()
    {
        var services = new ServiceCollection();

        Action act = () => services.AddMediatorIdempotencyBehavior(options => options.ScopeResolver = null!);

        act.Should().Throw<InvalidOperationException>().WithMessage("*ScopeResolver*");
    }

    [Fact]
    public async Task AddMediatorIdempotencyBehavior_Should_Register_A_Behavior_That_Resolves_And_Runs()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMediatorIdempotencyBehavior(options => options.UnscopedKeyBehavior = UnscopedKeyBehavior.Global);
        await using ServiceProvider provider = services.BuildServiceProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        int runs = 0;

        IPipelineBehavior<IdemCommand, Result<int>> behavior = scope.ServiceProvider.GetServices<IPipelineBehavior<IdemCommand, Result<int>>>().Single();
        Result<int> first = await behavior.Handle(new IdemCommand("k1", 5), (_, _) => new ValueTask<Result<int>>(++runs), CancellationToken.None);
        Result<int> retry = await behavior.Handle(new IdemCommand("k1", 5), (_, _) => new ValueTask<Result<int>>(++runs), CancellationToken.None);

        behavior.Should().BeOfType<IdempotencyBehavior<IdemCommand, Result<int>>>();
        first.Value.Should().Be(1);
        retry.Value.Should().Be(1);
        runs.Should().Be(1);
    }
}
