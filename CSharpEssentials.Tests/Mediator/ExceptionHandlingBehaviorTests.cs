using Mediator;

using CSharpEssentials.Errors;
using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;

using FluentAssertions;

namespace CSharpEssentials.Tests.Mediator;

internal sealed record TestExceptionCommand(string Name) : ICommand<Result>;

public class ExceptionHandlingBehaviorTests
{
    private static readonly MessageHandlerDelegate<TestExceptionCommand, Result> SuccessNext =
        (message, ct) => new ValueTask<Result>(Result.Success());

    // -------------------------------------------------------------------------
    // Happy path — Result
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Handle_Should_PassThrough_When_Handler_Returns_Success()
    {
        var behavior = new ExceptionHandlingBehavior<TestExceptionCommand, Result>();
        var command = new TestExceptionCommand("test");

        Result result = await behavior.Handle(command, SuccessNext, default);

        result.IsSuccess.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // Happy path — Result<T>
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Handle_Should_PassThrough_When_Handler_Returns_GenericResultSuccess()
    {
        var behavior = new ExceptionHandlingBehavior<TestExceptionCommand, Result<int>>();
        var command = new TestExceptionCommand("test");
        MessageHandlerDelegate<TestExceptionCommand, Result<int>> next =
            (_, _) => new ValueTask<Result<int>>(42);

        Result<int> result = await behavior.Handle(command, next, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    // -------------------------------------------------------------------------
    // Exception → Result failure
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_Handler_Throws_And_ResponseIsResult()
    {
        var behavior = new ExceptionHandlingBehavior<TestExceptionCommand, Result>();
        var command = new TestExceptionCommand("test");
        MessageHandlerDelegate<TestExceptionCommand, Result> throwingNext =
            (_, _) => throw new InvalidOperationException("boom");

        Result result = await behavior.Handle(command, throwingNext, default);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Failure);
    }

    [Fact]
    public async Task Handle_Should_SetErrorCode_ToExceptionTypeName_When_Handler_Throws()
    {
        var behavior = new ExceptionHandlingBehavior<TestExceptionCommand, Result>();
        var command = new TestExceptionCommand("test");
        MessageHandlerDelegate<TestExceptionCommand, Result> throwingNext =
            (_, _) => throw new InvalidOperationException("boom");

        Result result = await behavior.Handle(command, throwingNext, default);

        result.FirstError.Code.Should().Be("InvalidOperationException");
    }

    [Fact]
    public async Task Handle_Should_SetErrorDescription_ToExceptionMessage_When_Handler_Throws()
    {
        var behavior = new ExceptionHandlingBehavior<TestExceptionCommand, Result>();
        var command = new TestExceptionCommand("test");
        MessageHandlerDelegate<TestExceptionCommand, Result> throwingNext =
            (_, _) => throw new InvalidOperationException("boom");

        Result result = await behavior.Handle(command, throwingNext, default);

        result.FirstError.Description.Should().Be("boom");
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_Handler_Throws_And_ResponseIsGenericResult()
    {
        var behavior = new ExceptionHandlingBehavior<TestExceptionCommand, Result<int>>();
        var command = new TestExceptionCommand("test");
        MessageHandlerDelegate<TestExceptionCommand, Result<int>> throwingNext =
            (_, _) => throw new InvalidOperationException("boom");

        Result<int> result = await behavior.Handle(command, throwingNext, default);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Failure);
    }

    // -------------------------------------------------------------------------
    // OperationCanceledException always propagates
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Handle_Should_Propagate_OperationCanceledException_When_Handler_Throws_OCE()
    {
        var behavior = new ExceptionHandlingBehavior<TestExceptionCommand, Result>();
        var command = new TestExceptionCommand("test");
        MessageHandlerDelegate<TestExceptionCommand, Result> oceNext =
            (_, _) => throw new OperationCanceledException();

        Func<Task> act = () => behavior.Handle(command, oceNext, default).AsTask();

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Handle_Should_Propagate_OperationCanceledException_When_Token_Is_Cancelled()
    {
        var behavior = new ExceptionHandlingBehavior<TestExceptionCommand, Result>();
        var command = new TestExceptionCommand("test");
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        MessageHandlerDelegate<TestExceptionCommand, Result> cancellingNext =
            (_, ct) =>
            {
                ct.ThrowIfCancellationRequested();
                return new ValueTask<Result>(Result.Success());
            };

        Func<Task> act = () => behavior.Handle(command, cancellingNext, cts.Token).AsTask();

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // -------------------------------------------------------------------------
    // Non-Result TResponse — exception propagates unchanged
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Handle_Should_Propagate_Exception_When_ResponseType_IsNotResult()
    {
        var behavior = new ExceptionHandlingBehavior<TestExceptionCommand, string>();
        var command = new TestExceptionCommand("test");
        MessageHandlerDelegate<TestExceptionCommand, string> throwingNext =
            (_, _) => throw new InvalidOperationException("should propagate");

        Func<Task> act = () => behavior.Handle(command, throwingNext, default).AsTask();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("should propagate");
    }

    // -------------------------------------------------------------------------
    // Truly async handler — exception caught after await
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_AsyncHandler_Throws_After_Await()
    {
        var behavior = new ExceptionHandlingBehavior<TestExceptionCommand, Result>();
        var command = new TestExceptionCommand("test");
        MessageHandlerDelegate<TestExceptionCommand, Result> asyncThrowingNext =
            async (_, _) =>
            {
                await Task.Yield();
                throw new ArgumentException("async boom");
            };

        Result result = await behavior.Handle(command, asyncThrowingNext, default);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("ArgumentException");
        result.FirstError.Description.Should().Be("async boom");
    }
}
