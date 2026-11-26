using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed;

/// <summary>
/// The Task-source <c>MapAsync</c>, <c>MapErrorAsync</c>, <c>EnsureAsync</c>, <c>CompensateAsync</c> and <c>TapAsync</c>
/// extensions with a Task handler: a missing handler task, a canceled token and a pending mapper behave as the instance
/// members do.
/// </summary>
public sealed class TypedAsyncInlineEdgeTests
{
    private static readonly NotFoundError Missing = new("EDGE.NotFound", "missing");
    private static readonly Task<Result<int, NotFoundError>> Success = Task.FromResult<Result<int, NotFoundError>>(1);
    private static readonly Task<Result<int, NotFoundError>> Failure = Task.FromResult<Result<int, NotFoundError>>(Missing);

    [Fact]
    public async Task MapAsync_Should_FaultWithArgumentNullException_When_TheHandlerReturnsNull()
    {
        Task<Result<int, NotFoundError>> task = Success.MapAsync(_ => (Task<int>)null!);

        task.IsFaulted.Should().BeTrue();
        await FluentActions.Awaiting(() => task).Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task MapAsync_Should_NotCallTheHandler_When_TheTokenIsCanceled()
    {
        bool called = false;
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        Task<Result<int, NotFoundError>> task = Task.FromResult<Result<int, NotFoundError>>(1).MapAsync(v =>
        {
            called = true;
            return Task.FromResult(v);
        }, cts.Token);

        await FluentActions.Awaiting(() => task).Should().ThrowAsync<OperationCanceledException>();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task MapErrorAsync_Should_FaultWithNullReferenceException_When_TheMapperReturnsNull()
    {
        Task<Result<int, ConflictError>> task = Failure.MapErrorAsync(_ => (Task<ConflictError>)null!);

        task.IsFaulted.Should().BeTrue();
        await FluentActions.Awaiting(() => task).Should().ThrowAsync<NullReferenceException>();
    }

    [Fact]
    public async Task MapErrorAsync_Should_NotAbandonTheMapper_When_TheTokenIsCanceledWhileItIsPending()
    {
        using var cts = new CancellationTokenSource();
        var mapper = new TaskCompletionSource<ConflictError>(TaskCreationOptions.RunContinuationsAsynchronously);
        var conflict = new ConflictError("EDGE.Conflict", "conflict");

        Task<Result<int, ConflictError>> task = Failure.MapErrorAsync(_ => mapper.Task, cts.Token);
        await cts.CancelAsync();
        mapper.SetResult(conflict);

        (await task).Should().Be((Result<int, ConflictError>)conflict);
    }

    [Fact]
    public async Task EnsureAsync_Should_Throw_When_TheTokenIsCanceledWhileThePredicateIsPending()
    {
        using var cts = new CancellationTokenSource();
        var predicate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        Task<Result<int, NotFoundError>> task = Success.EnsureAsync(_ => predicate.Task, Missing, cts.Token);
        await cts.CancelAsync();

        await FluentActions.Awaiting(() => task).Should().ThrowAsync<OperationCanceledException>();
        predicate.SetResult(true);
    }

    [Fact]
    public async Task EnsureAsync_Should_Fault_When_TheSourceIsDefault()
    {
        Task<Result<int, NotFoundError>> task = Task.FromResult(default(Result<int, NotFoundError>)).EnsureAsync(_ => Task.FromResult(true), Missing);

        task.IsFaulted.Should().BeTrue();
        await FluentActions.Awaiting(() => task).Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CompensateAsync_Should_ReturnTheSuccess_When_TheSourceSucceeds()
    {
        Result<int, NotFoundError> result = await Success.CompensateAsync(_ => Task.FromResult<Result<int, NotFoundError>>(2));

        result.Should().Be((Result<int, NotFoundError>)1);
    }

    [Fact]
    public async Task TapAsync_Should_Fault_When_TheActionThrows()
    {
        Task<Result<int, NotFoundError>> task = Success.TapAsync((Func<int, Task>)(_ => throw new FormatException("tap")));

        task.IsFaulted.Should().BeTrue();
        await FluentActions.Awaiting(() => task).Should().ThrowAsync<FormatException>().WithMessage("tap");
    }
}
