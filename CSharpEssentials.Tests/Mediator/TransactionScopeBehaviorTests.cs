using Mediator;
using System.Transactions;

using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;

using FluentAssertions;

namespace CSharpEssentials.Tests.Mediator;

internal sealed record TestTransactionalCommand(string Name) : ICommand<Result>, ITransactionalRequest;

public class TransactionScopeBehaviorTests
{
    private static readonly MessageHandlerDelegate<TestTransactionalCommand, Result> SuccessNext =
        (message, ct) => new ValueTask<Result>(Result.Success());

    [Fact]
    public async Task Handle_Should_Complete_TransactionScope_On_Success()
    {
        var behavior = new TransactionScopeBehavior<TestTransactionalCommand, Result>();
        var command = new TestTransactionalCommand("test");

        Result result = await behavior.Handle(command, SuccessNext, default);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_Propagate_Exception_When_Handler_Throws()
    {
        var behavior = new TransactionScopeBehavior<TestTransactionalCommand, Result>();
        var command = new TestTransactionalCommand("test");

        Func<Task> act = () => behavior.Handle(
            command,
            (_, _) => throw new InvalidOperationException("Handler failed"),
            default).AsTask();

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Handler failed");
    }

    [Fact]
    public async Task Handle_Should_Create_TransactionScope_With_AsyncFlow()
    {
        var behavior = new TransactionScopeBehavior<TestTransactionalCommand, Result>();
        var command = new TestTransactionalCommand("test");

        bool handlerCalled = false;
        Result result = await behavior.Handle(command, (msg, ct) =>
        {
            handlerCalled = true;
            Transaction.Current.Should().NotBeNull();
            return new ValueTask<Result>(Result.Success());
        }, default);

        handlerCalled.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
    }
}
