using CSharpEssentials.ResultPattern;
using CSharpEssentials.Rules;
using FluentAssertions;

namespace CSharpEssentials.Tests.Rules;

public class RuleEngineAsyncCancellationTests
{
    [Fact]
    public async Task Evaluate_TopLevelAsyncRule_CallerCancelled_ShouldThrowOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        Func<Task> act = async () => await RuleEngine.Evaluate(
            async (int _, CancellationToken ct) =>
            {
                await Task.Delay(Timeout.Infinite, ct);
                return Result.Success();
            },
            1,
            cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Evaluate_TopLevelAsyncRule_ThrowsOceOfCancelledCallerToken_ShouldThrowOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        CancellationToken token = cts.Token;

        Func<Task> act = async () => await RuleEngine.Evaluate(
            (int _, CancellationToken ct) => ValueTask.FromException<Result>(new OperationCanceledException(token)),
            1,
            token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
