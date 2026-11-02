using CSharpEssentials.Maybe;
using FluentAssertions;

namespace CSharpEssentials.Tests.Maybe;

public class MaybeKeyValueMatchTests
{
    private static readonly Maybe<KeyValuePair<string, int>> Some = Maybe<KeyValuePair<string, int>>.From(new KeyValuePair<string, int>("a", 1));
    private static readonly Maybe<KeyValuePair<string, int>> None = Maybe<KeyValuePair<string, int>>.None;

    #region Instance Match with context and actions

    [Fact]
    public void Match_WithContext_ShouldPassContextToChosenBranch()
    {
        Maybe<int>.From(2).Match((x, ctx) => x * ctx, ctx => -ctx, 10).Should().Be(20);
        Maybe<int>.None.Match((x, ctx) => x * ctx, ctx => -ctx, 10).Should().Be(-10);
    }

    [Fact]
    public void Match_Actions_ShouldRunOnlyChosenBranch()
    {
        var log = new List<string>();

        Maybe<int>.From(2).Match(x => log.Add($"some {x}"), () => log.Add("none"));
        Maybe<int>.None.Match(x => log.Add($"some {x}"), () => log.Add("none"));

        log.Should().Equal("some 2", "none");
    }

    [Fact]
    public void Match_ActionsWithContext_ShouldRunOnlyChosenBranch()
    {
        var log = new List<string>();

        Maybe<int>.From(2).Match((x, ctx) => ctx.Add($"some {x}"), ctx => ctx.Add("none"), log);
        Maybe<int>.None.Match((x, ctx) => ctx.Add($"some {x}"), ctx => ctx.Add("none"), log);

        log.Should().Equal("some 2", "none");
    }

    #endregion

    #region KeyValuePair sync

    [Fact]
    public void Match_KeyValue_Func_ShouldDeconstructPair()
    {
        Some.Match((k, v) => $"{k}={v}", () => "none").Should().Be("a=1");
        None.Match((k, v) => $"{k}={v}", () => "none").Should().Be("none");
    }

    [Fact]
    public void Match_KeyValue_FuncWithContext_ShouldDeconstructPair()
    {
        Some.Match((k, v, ctx) => $"{ctx}{k}={v}", ctx => $"{ctx}none", ">").Should().Be(">a=1");
        None.Match((k, v, ctx) => $"{ctx}{k}={v}", ctx => $"{ctx}none", ">").Should().Be(">none");
    }

    [Fact]
    public void Match_KeyValue_Actions_ShouldRunOnlyChosenBranch()
    {
        var log = new List<string>();

        Some.Match((k, v) => log.Add($"{k}={v}"), () => log.Add("none"));
        None.Match((k, v) => log.Add($"{k}={v}"), () => log.Add("none"));

        log.Should().Equal("a=1", "none");
    }

    [Fact]
    public void Match_KeyValue_ActionsWithContext_ShouldRunOnlyChosenBranch()
    {
        var log = new List<string>();

        Some.Match((k, v, ctx) => ctx.Add($"{k}={v}"), ctx => ctx.Add("none"), log);
        None.Match((k, v, ctx) => ctx.Add($"{k}={v}"), ctx => ctx.Add("none"), log);

        log.Should().Equal("a=1", "none");
    }

    #endregion

    #region KeyValuePair ValueTask

    [Fact]
    public async Task MatchAsync_KeyValue_ValueTaskFunc_ShouldDeconstructPair()
    {
        (await Some.MatchAsync((k, v, _) => new ValueTask<string>($"{k}={v}"), _ => new ValueTask<string>("none"))).Should().Be("a=1");
        (await None.MatchAsync((k, v, _) => new ValueTask<string>($"{k}={v}"), _ => new ValueTask<string>("none"))).Should().Be("none");
    }

    [Fact]
    public async Task MatchAsync_KeyValue_ValueTaskFuncWithContext_ShouldDeconstructPair()
    {
        (await Some.MatchAsync((k, v, ctx, _) => new ValueTask<string>($"{ctx}{k}={v}"), (ctx, _) => new ValueTask<string>($"{ctx}none"), ">")).Should().Be(">a=1");
        (await None.MatchAsync((k, v, ctx, _) => new ValueTask<string>($"{ctx}{k}={v}"), (ctx, _) => new ValueTask<string>($"{ctx}none"), ">")).Should().Be(">none");
    }

    [Fact]
    public async Task MatchAsync_KeyValue_ValueTaskActions_ShouldRunOnlyChosenBranch()
    {
        var log = new List<string>();

        await Some.MatchAsync((k, v, _) => { log.Add($"{k}={v}"); return ValueTask.CompletedTask; }, _ => { log.Add("none"); return ValueTask.CompletedTask; });
        await None.MatchAsync((k, v, _) => { log.Add($"{k}={v}"); return ValueTask.CompletedTask; }, _ => { log.Add("none"); return ValueTask.CompletedTask; });

        log.Should().Equal("a=1", "none");
    }

    [Fact]
    public async Task MatchAsync_KeyValue_ValueTaskActionsWithContext_ShouldRunOnlyChosenBranch()
    {
        var log = new List<string>();

        await Some.MatchAsync((k, v, ctx, _) => { ctx.Add($"{k}={v}"); return ValueTask.CompletedTask; }, (ctx, _) => { ctx.Add("none"); return ValueTask.CompletedTask; }, log);
        await None.MatchAsync((k, v, ctx, _) => { ctx.Add($"{k}={v}"); return ValueTask.CompletedTask; }, (ctx, _) => { ctx.Add("none"); return ValueTask.CompletedTask; }, log);

        log.Should().Equal("a=1", "none");
    }

    #endregion

    #region KeyValuePair Task

    [Fact]
    public async Task MatchAsync_KeyValue_TaskFunc_ShouldDeconstructPair()
    {
        (await Some.MatchAsync((k, v, _) => Task.FromResult($"{k}={v}"), _ => Task.FromResult("none"))).Should().Be("a=1");
        (await None.MatchAsync((k, v, _) => Task.FromResult($"{k}={v}"), _ => Task.FromResult("none"))).Should().Be("none");
    }

    [Fact]
    public async Task MatchAsync_KeyValue_TaskFuncWithContext_ShouldDeconstructPair()
    {
        (await Some.MatchAsync((k, v, ctx, _) => Task.FromResult($"{ctx}{k}={v}"), (ctx, _) => Task.FromResult($"{ctx}none"), ">")).Should().Be(">a=1");
        (await None.MatchAsync((k, v, ctx, _) => Task.FromResult($"{ctx}{k}={v}"), (ctx, _) => Task.FromResult($"{ctx}none"), ">")).Should().Be(">none");
    }

    [Fact]
    public async Task MatchAsync_KeyValue_TaskActions_ShouldRunOnlyChosenBranch()
    {
        var log = new List<string>();

        await Some.MatchAsync((k, v, _) => { log.Add($"{k}={v}"); return Task.CompletedTask; }, _ => { log.Add("none"); return Task.CompletedTask; });
        await None.MatchAsync((k, v, _) => { log.Add($"{k}={v}"); return Task.CompletedTask; }, _ => { log.Add("none"); return Task.CompletedTask; });

        log.Should().Equal("a=1", "none");
    }

    [Fact]
    public async Task MatchAsync_KeyValue_TaskActionsWithContext_ShouldRunOnlyChosenBranch()
    {
        var log = new List<string>();

        await Some.MatchAsync((k, v, ctx, _) => { ctx.Add($"{k}={v}"); return Task.CompletedTask; }, (ctx, _) => { ctx.Add("none"); return Task.CompletedTask; }, log);
        await None.MatchAsync((k, v, ctx, _) => { ctx.Add($"{k}={v}"); return Task.CompletedTask; }, (ctx, _) => { ctx.Add("none"); return Task.CompletedTask; }, log);

        log.Should().Equal("a=1", "none");
    }

    [Fact]
    public async Task MatchAsync_KeyValue_ShouldForwardCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        CancellationToken seen = default;

        await Some.MatchAsync((_, _, ct) => { seen = ct; return Task.FromResult(0); }, _ => Task.FromResult(0), cts.Token);

        seen.Should().Be(cts.Token);
    }

    #endregion
}
