using CSharpEssentials.Errors;
using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Transactions;

using FluentAssertions;

using Mediator;

namespace CSharpEssentials.Tests.Mediator;

internal sealed record TransactionalQuery(int Value) : IQuery<Result<int>>, ITransactionalRequest;

internal sealed record TransactionalTextCommand(string Text) : ICommand<string>, ITransactionalRequest;

internal sealed class FakeTransactionRunner : ITransactionRunner
{
    public int Runs { get; private set; }
    public bool? Committed { get; private set; }

    public async ValueTask<T> ExecuteAsync<T>(
        Func<CancellationToken, ValueTask<T>> work,
        Func<T, bool> shouldCommit,
        CancellationToken cancellationToken = default)
    {
        Runs++;
        try
        {
            T result = await work(cancellationToken);
            Committed = shouldCommit(result);
            return result;
        }
        catch
        {
            Committed = false;
            throw;
        }
    }
}

public class TransactionBehaviorTests
{
    [Fact]
    public async Task Handle_Should_Commit_When_Result_Succeeds()
    {
        FakeTransactionRunner runner = new();
        TransactionBehavior<TestTransactionalCommand, Result> behavior = new(runner);

        Result result = await behavior.Handle(new TestTransactionalCommand("x"), (_, _) => new ValueTask<Result>(Result.Success()), default);

        result.IsSuccess.Should().BeTrue();
        runner.Committed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_Roll_Back_When_Result_Fails()
    {
        FakeTransactionRunner runner = new();
        TransactionBehavior<TestTransactionalCommand, Result> behavior = new(runner);

        Result result = await behavior.Handle(
            new TestTransactionalCommand("x"),
            (_, _) => new ValueTask<Result>(Error.Conflict("Order.Conflict", "conflict")),
            default);

        result.IsFailure.Should().BeTrue();
        runner.Committed.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Roll_Back_When_Generic_Result_Fails()
    {
        FakeTransactionRunner runner = new();
        TransactionBehavior<TransactionalQuery, Result<int>> behavior = new(runner);

        Result<int> result = await behavior.Handle(
            new TransactionalQuery(1),
            (_, _) => new ValueTask<Result<int>>(Error.NotFound("Order.NotFound", "missing")),
            default);

        result.IsFailure.Should().BeTrue();
        runner.Committed.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Commit_When_Generic_Result_Succeeds()
    {
        FakeTransactionRunner runner = new();
        TransactionBehavior<TransactionalQuery, Result<int>> behavior = new(runner);

        Result<int> result = await behavior.Handle(new TransactionalQuery(1), (_, _) => new ValueTask<Result<int>>(42), default);

        result.Value.Should().Be(42);
        runner.Committed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_Commit_Non_Result_Response()
    {
        FakeTransactionRunner runner = new();
        TransactionBehavior<TransactionalTextCommand, string> behavior = new(runner);

        string response = await behavior.Handle(new TransactionalTextCommand("x"), (_, _) => new ValueTask<string>("ok"), default);

        response.Should().Be("ok");
        runner.Committed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_Roll_Back_And_Rethrow_When_Handler_Throws()
    {
        FakeTransactionRunner runner = new();
        TransactionBehavior<TestTransactionalCommand, Result> behavior = new(runner);

        Func<Task> act = () => behavior.Handle(
            new TestTransactionalCommand("x"),
            (_, _) => throw new InvalidOperationException("boom"),
            default).AsTask();

        await act.Should().ThrowAsync<InvalidOperationException>();
        runner.Committed.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Pass_Cancellation_Token_To_Handler()
    {
        using CancellationTokenSource cts = new();
        CancellationToken seen = default;
        TransactionBehavior<TestTransactionalCommand, Result> behavior = new(new FakeTransactionRunner());

        await behavior.Handle(new TestTransactionalCommand("x"), (_, token) =>
        {
            seen = token;
            return new ValueTask<Result>(Result.Success());
        }, cts.Token);

        seen.Should().Be(cts.Token);
    }
}
