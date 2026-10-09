using CSharpEssentials.Errors;
using CSharpEssentials.Maybe;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

/// <summary>
/// Library awaits must not hop back to the caller's <see cref="SynchronizationContext"/>: a sync-over-async caller on a
/// single-threaded context would deadlock. Each chain runs to completion on a thread whose context counts every post.
/// </summary>
public sealed class SynchronizationContextCaptureTests
{
    private static readonly Error TestError = Error.Failure("TEST", "Test error");

    public static TheoryData<string> ChainNames() => new(Chains.Keys);

    private static readonly Dictionary<string, Func<Task>> Chains = new()
    {
        ["Result.MapAsync(Task handler)"] = () => ResultSource(1).MapAsync(v => DelayedValue(v + 1)),
        ["Result.MapAsync(sync handler)"] = () => ResultSource(1).MapAsync(v => v + 1),
        ["Result.BindAsync"] = () => ResultSource(1).BindAsync(v => ResultSource(v + 1)),
        ["Result.TapAsync"] = () => ResultSource(1).TapAsync(_ => Task.Delay(5)),
        ["Result.ThenAsync"] = () => ResultSource(1).ThenAsync(v => ResultSource(v + 1)),
        ["Result.EnsureAsync"] = () => ResultSource(1).EnsureAsync(v => v > 0, TestError),
        ["Result.ElseAsync"] = () => FailureSource().ElseAsync(_ => DelayedValue(7)),
        ["Result.MatchAsync"] = () => ResultSource(1).MatchAsync(v => DelayedValue(v), _ => DelayedValue(0)),
        ["Result.SwitchAsync"] = () => ResultSource(1).SwitchAsync(_ => Task.Delay(5), _ => Task.Delay(5)),
        ["Result.FinallyAsync"] = () => ResultSource(1).FinallyAsync(r => DelayedValue(r.IsSuccess)),
        ["Result.TapErrorAsync"] = () => FailureSource().TapErrorAsync(_ => Task.Delay(5)),
        ["Result.CompensateAsync"] = () => FailureSource().CompensateAsync(_ => ResultSource(3)),
        ["Result.TryAsync"] = () => Result.TryAsync(TryBody, _ => TestError),
        ["Result.FailIfAsync"] = () => ResultSource(1).FailIfAsync(v => DelayedValue(v > 5), TestError),
        ["Maybe.ExecuteAsync"] = () => Maybe<int>.From(1).ExecuteAsync(_ => Task.Delay(5)),
        ["Maybe.ExecuteAsync(Task source)"] = () => MaybeSource(1).ExecuteAsync(_ => Task.Delay(5)),
        ["Maybe.OrAsync"] = () => Maybe<int>.None.OrAsync(() => DelayedValue(3)),
        ["Maybe.OrAsync(Task source)"] = () => MaybeSource(null).OrAsync(() => DelayedValue(3)),
        ["Maybe.MatchAsync"] = () => Maybe<int>.From(1).MatchAsync((v, _) => DelayedValue(v), _ => DelayedValue(0)),
        ["Maybe.MapAsync"] = () => MaybeSource(1).MapAsync(v => DelayedValue(v + 1)),
        ["Maybe.BindAsync"] = () => Maybe<int>.From(1).BindAsync(v => MaybeSource(v + 1)),
        ["Maybe.WhereAsync"] = () => MaybeSource(1).WhereAsync(v => DelayedValue(v > 0)),
        ["Maybe.ToMaybeResultAsync"] = () => MaybeSource(1).ToMaybeResultAsync(),
    };

    [Theory]
    [MemberData(nameof(ChainNames))]
    public void Chain_Should_NotPostToSynchronizationContext_When_AwaitedByLibraryCode(string chainName)
    {
        var context = new CountingSynchronizationContext();
        Func<Task> chain = Chains[chainName];

        Exception? failure = null;
        var thread = new Thread(() =>
        {
            SynchronizationContext.SetSynchronizationContext(context);
            try
            {
                chain().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        });
        thread.Start();
        thread.Join();

        (failure, context.PostCount).Should().Be(((Exception?)null, 0));
    }

    private static async Task<Result<int>> ResultSource(int value)
    {
        await Task.Delay(5).ConfigureAwait(false);
        return value;
    }

    private static async Task<Result<int>> FailureSource()
    {
        await Task.Delay(5).ConfigureAwait(false);
        return TestError;
    }

    private static async Task<Maybe<int>> MaybeSource(int? value)
    {
        await Task.Delay(5).ConfigureAwait(false);
        return value is { } v ? Maybe<int>.From(v) : Maybe<int>.None;
    }

    private static Task<int> TryBody() => DelayedValue(4);

    private static async Task<T> DelayedValue<T>(T value)
    {
        await Task.Delay(5).ConfigureAwait(false);
        return value;
    }

    private sealed class CountingSynchronizationContext : SynchronizationContext
    {
        private int _postCount;

        public int PostCount => Volatile.Read(ref _postCount);

        public override void Post(SendOrPostCallback d, object? state)
        {
            Interlocked.Increment(ref _postCount);
            ThreadPool.QueueUserWorkItem(_ => d(state));
        }
    }
}
