using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultTaskPipelineTests
{
    private static readonly Error TestError = Error.Failure("TEST", "Test error");

    public static TheoryData<string> Steps => ["Bind", "Map", "Ensure", "Tap"];

    private static Task<Result<int>> Run(string step, Task<Result<int>> source, Action onCall, CancellationToken cancellationToken = default) => step switch
    {
        "Bind" => source.BindAsync(x => { onCall(); return Task.FromResult(Result<int>.Success(x + 1)); }, cancellationToken),
        "Map" => source.MapAsync(x => { onCall(); return x + 1; }, cancellationToken),
        "Ensure" => source.EnsureAsync(x => { onCall(); return x > 0; }, TestError, cancellationToken),
        _ => source.TapAsync(_ => onCall(), cancellationToken),
    };

    [Theory]
    [MemberData(nameof(Steps))]
    public async Task Step_Should_ReturnFaultedTask_When_SourceTaskIsNull(string step)
    {
        Task<Result<int>> task = Run(step, null!, () => { });

        await task.Invoking(t => t).Should().ThrowAsync<ArgumentNullException>();
    }

    [Theory]
    [MemberData(nameof(Steps))]
    public async Task Step_Should_ReturnFaultedTask_When_DelegateThrows(string step)
    {
        Task<Result<int>> task = Run(step, Task.FromResult(Result<int>.Success(1)), () => throw new InvalidOperationException("boom"));

        task.IsFaulted.Should().BeTrue();
        await task.Invoking(t => t).Should().ThrowAsync<InvalidOperationException>();
    }

    [Theory]
    [MemberData(nameof(Steps))]
    public async Task Step_Should_IgnoreCancelledToken_When_SourceIsAlreadyCompleted(string step)
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        int calls = 0;

        Result<int> result = await Run(step, Task.FromResult(Result<int>.Success(1)), () => calls++, cts.Token);

        result.IsSuccess.Should().BeTrue();
        calls.Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(Steps))]
    public async Task Step_Should_Cancel_When_TokenIsCancelledWhileSourceIsPending(string step)
    {
        using var cts = new CancellationTokenSource();
        var source = new TaskCompletionSource<Result<int>>(TaskCreationOptions.RunContinuationsAsynchronously);
        int calls = 0;

        Task<Result<int>> task = Run(step, source.Task, () => calls++, cts.Token);
        await cts.CancelAsync();

        await task.Invoking(t => t).Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    [Theory]
    [MemberData(nameof(Steps))]
    public async Task Step_Should_PassFailureThrough_WithoutCallingDelegate(string step)
    {
        Error[] errors = [TestError];
        int calls = 0;

        Result<int> result = await Run(step, Task.FromResult(Result<int>.Failure(errors)), () => calls++);

        calls.Should().Be(0);
        result.Errors.Should().BeSameAs(errors);
    }

    [Fact]
    public async Task BindAsync_Should_ReturnFaultedTask_When_StepReturnsNullTask()
    {
        Task<Result<int>> task = Task.FromResult(Result<int>.Success(1)).BindAsync(_ => (Task<Result<int>>)null!);

        await task.Invoking(t => t).Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task BindAsync_Should_Cancel_When_TokenIsCancelledWhileStepIsPending()
    {
        using var cts = new CancellationTokenSource();
        var step = new TaskCompletionSource<Result<int>>(TaskCreationOptions.RunContinuationsAsynchronously);

        Task<Result<int>> task = Task.FromResult(Result<int>.Success(1)).BindAsync(_ => step.Task, cts.Token);
        await cts.CancelAsync();

        await task.Invoking(t => t).Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Pipeline_Should_ProduceSameResult_When_SourceYields()
    {
        int sink = 0;

        Result<int> result = await Source()
            .BindAsync(async x => { await Task.Yield(); return Result<int>.Success(x + 1); })
            .MapAsync(x => x * 2)
            .EnsureAsync(x => x > 0, TestError)
            .TapAsync(x => sink = x)
            .MapAsync(x => x + 1);

        result.Value.Should().Be(87);
        sink.Should().Be(86);

        static async Task<Result<int>> Source()
        {
            await Task.Yield();
            return 42;
        }
    }
}
