using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed;

/// <summary>
/// The Task-source <c>BindAsync</c> and <c>BindWidenAsync</c> extensions with a Task step: a failing, missing or pending step
/// ends in the returned task, never as a synchronous throw.
/// </summary>
public sealed class TypedAsyncBindEdgeTests
{
    private static readonly Task<Result<int, NotFoundError>> Source = Task.FromResult<Result<int, NotFoundError>>(1);

    [Fact]
    public async Task BindAsync_Should_FaultWithNullReferenceException_When_TheStepReturnsNull()
    {
        Task<Result<int, NotFoundError>> task = Source.BindAsync(_ => (Task<Result<int, NotFoundError>>)null!);

        task.IsFaulted.Should().BeTrue();
        await FluentActions.Awaiting(() => task).Should().ThrowAsync<NullReferenceException>();
    }

    [Fact]
    public async Task BindWidenAsync_Should_FaultWithNullReferenceException_When_TheStepReturnsNull()
    {
        Task<Result<int, NotFoundError, ConflictError>> task = Source.BindWidenAsync(_ => (Task<Result<int, ConflictError>>)null!);

        task.IsFaulted.Should().BeTrue();
        await FluentActions.Awaiting(() => task).Should().ThrowAsync<NullReferenceException>();
    }

    [Fact]
    public async Task BindAsync_Should_Fault_When_TheStepThrows()
    {
        Task<Result<int, NotFoundError>> task = Source.BindAsync((Func<int, Task<Result<int, NotFoundError>>>)(_ => throw new FormatException("step")));

        task.IsFaulted.Should().BeTrue();
        await FluentActions.Awaiting(() => task).Should().ThrowAsync<FormatException>().WithMessage("step");
    }

    [Fact]
    public async Task BindAsync_Should_BeCanceled_When_TheStepThrowsOperationCanceledException()
    {
        Task<Result<int, NotFoundError>> task = Source.BindAsync((Func<int, Task<Result<int, NotFoundError>>>)(_ => throw new OperationCanceledException()));

        task.IsCanceled.Should().BeTrue();
        await FluentActions.Awaiting(() => task).Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task BindAsync_Should_Fault_When_TheSourceIsDefault()
    {
        Task<Result<int, NotFoundError>> task = Task.FromResult(default(Result<int, NotFoundError>)).BindAsync(v => Task.FromResult<Result<int, NotFoundError>>(v));

        task.IsFaulted.Should().BeTrue();
        await FluentActions.Awaiting(() => task).Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task BindAsync_Should_Throw_When_TheTokenIsCanceledWhileTheStepIsPending()
    {
        using var cts = new CancellationTokenSource();
        var step = new TaskCompletionSource<Result<int, NotFoundError>>(TaskCreationOptions.RunContinuationsAsynchronously);

        Task<Result<int, NotFoundError>> task = Source.BindAsync(_ => step.Task, cts.Token);
        await cts.CancelAsync();

        await FluentActions.Awaiting(() => task).Should().ThrowAsync<OperationCanceledException>();
        step.SetResult(2);
    }

    [Fact]
    public async Task BindAsync_Should_ReturnTheStepResult_When_TheStepCompletesLater()
    {
        using var cts = new CancellationTokenSource();
        var step = new TaskCompletionSource<Result<int, NotFoundError>>(TaskCreationOptions.RunContinuationsAsynchronously);

        Task<Result<int, NotFoundError>> task = Source.BindAsync(_ => step.Task, cts.Token);
        step.SetResult(2);

        (await task).Should().Be((Result<int, NotFoundError>)2);
    }
}
