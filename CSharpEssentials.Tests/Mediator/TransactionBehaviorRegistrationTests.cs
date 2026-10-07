using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Transactions;

using FluentAssertions;

using Mediator;

using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.Mediator;

public class TransactionBehaviorRegistrationTests
{
    private static List<Type?> TransactionBehaviors(IServiceCollection services) =>
        [.. services
            .Where(sd => sd.ServiceType == typeof(IPipelineBehavior<,>)
                && (sd.ImplementationType == typeof(TransactionBehavior<,>)
                    || sd.ImplementationType == typeof(TransactionScopeBehavior<,>)))
            .Select(sd => sd.ImplementationType)];

    [Fact]
    public void AddMediatorTransactionRunnerBehavior_Should_Replace_Scope_Behavior_In_Place()
    {
        var services = new ServiceCollection();
        services.AddMediatorBehaviors();
        int position = services.ToList().FindIndex(sd => sd.ImplementationType == typeof(TransactionScopeBehavior<,>));

        services.AddMediatorTransactionRunnerBehavior();

        TransactionBehaviors(services).Should().Equal(typeof(TransactionBehavior<,>));
        services[position].ImplementationType.Should().Be(typeof(TransactionBehavior<,>));
        services[position].Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddMediatorTransactionBehavior_Should_Replace_Runner_Behavior()
    {
        var services = new ServiceCollection();
        services.AddMediatorTransactionRunnerBehavior();

        services.AddMediatorTransactionBehavior();

        TransactionBehaviors(services).Should().Equal(typeof(TransactionScopeBehavior<,>));
    }

    [Fact]
    public void AddMediatorTransactionRunnerBehavior_Should_Register_Once_When_Called_Twice()
    {
        var services = new ServiceCollection();

        services.AddMediatorTransactionRunnerBehavior();
        services.AddMediatorTransactionRunnerBehavior();

        TransactionBehaviors(services).Should().Equal(typeof(TransactionBehavior<,>));
    }

    [Fact]
    public async Task Resolved_Runner_Behavior_Should_Use_Registered_Runner()
    {
        FakeTransactionRunner runner = new();
        var services = new ServiceCollection();
        services.AddSingleton<ITransactionRunner>(runner);
        services.AddMediatorTransactionRunnerBehavior();
        await using ServiceProvider provider = services.BuildServiceProvider(validateScopes: true);
        using IServiceScope scope = provider.CreateScope();

        IPipelineBehavior<TestTransactionalCommand, Result> behavior =
            scope.ServiceProvider.GetRequiredService<IPipelineBehavior<TestTransactionalCommand, Result>>();
        await behavior.Handle(new TestTransactionalCommand("x"), (_, _) => new ValueTask<Result>(Result.Success()), default);

        runner.Runs.Should().Be(1);
    }
}
