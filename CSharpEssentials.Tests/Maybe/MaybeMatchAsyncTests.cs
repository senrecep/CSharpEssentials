using CSharpEssentials.Maybe;
using FluentAssertions;

namespace CSharpEssentials.Tests.Maybe;

public class MaybeMatchAsyncTests
{
    #region Match Task<TE> returning

    [Fact]
    public async Task MatchAsync_TaskTE_WithValue_ShouldInvokeSome()
    {
        var maybe = Maybe<int>.From(5);

        string result = await maybe.Match(
            (v, ct) => Task.FromResult(v.ToString()),
            ct => Task.FromResult("none"));

        result.Should().Be("5");
    }

    [Fact]
    public async Task MatchAsync_TaskTE_WithNoValue_ShouldInvokeNone()
    {
        Maybe<int> maybe = Maybe<int>.None;

        string result = await maybe.Match(
            (v, ct) => Task.FromResult(v.ToString()),
            ct => Task.FromResult("none"));

        result.Should().Be("none");
    }

    [Fact]
    public async Task MatchAsync_TaskTE_WithContext_WithValue_ShouldInvokeSome()
    {
        var maybe = Maybe<int>.From(10);
        string context = "ctx";

        string result = await maybe.Match(
            (v, c, ct) => Task.FromResult(v + c),
            (c, ct) => Task.FromResult("no-" + c),
            context);

        result.Should().Be("10ctx");
    }

    [Fact]
    public async Task MatchAsync_TaskTE_WithContext_WithNoValue_ShouldInvokeNone()
    {
        Maybe<int> maybe = Maybe<int>.None;
        string context = "ctx";

        string result = await maybe.Match(
            (v, c, ct) => Task.FromResult(v + c),
            (c, ct) => Task.FromResult("no-" + c),
            context);

        result.Should().Be("no-ctx");
    }

    #endregion

    #region Match Task (void) returning

    [Fact]
    public async Task MatchAsync_Task_WithValue_ShouldInvokeSome()
    {
        var maybe = Maybe<int>.From(7);
        int captured = 0;

        await maybe.Match(
            (v, ct) => { captured = v; return Task.CompletedTask; },
            ct => { captured = -1; return Task.CompletedTask; });

        captured.Should().Be(7);
    }

    [Fact]
    public async Task MatchAsync_Task_WithNoValue_ShouldInvokeNone()
    {
        Maybe<int> maybe = Maybe<int>.None;
        int captured = 0;

        await maybe.Match(
            (v, ct) => { captured = v; return Task.CompletedTask; },
            ct => { captured = -1; return Task.CompletedTask; });

        captured.Should().Be(-1);
    }

    [Fact]
    public async Task MatchAsync_Task_WithContext_WithValue_ShouldInvokeSome()
    {
        var maybe = Maybe<int>.From(3);
        int captured = 0;

        await maybe.Match(
            (v, c, ct) => { captured = v + c; return Task.CompletedTask; },
            (c, ct) => { captured = c; return Task.CompletedTask; },
            10);

        captured.Should().Be(13);
    }

    [Fact]
    public async Task MatchAsync_Task_WithContext_WithNoValue_ShouldInvokeNone()
    {
        Maybe<int> maybe = Maybe<int>.None;
        int captured = 0;

        await maybe.Match(
            (v, c, ct) => { captured = v + c; return Task.CompletedTask; },
            (c, ct) => { captured = c; return Task.CompletedTask; },
            10);

        captured.Should().Be(10);
    }

    #endregion

    #region Match ValueTask<TE> returning

    [Fact]
    public async Task MatchAsync_ValueTaskTE_WithValue_ShouldInvokeSome()
    {
        var maybe = Maybe<int>.From(42);

        string result = await maybe.Match(
            (v, ct) => ValueTask.FromResult(v.ToString()),
            ct => ValueTask.FromResult("none"));

        result.Should().Be("42");
    }

    [Fact]
    public async Task MatchAsync_ValueTaskTE_WithNoValue_ShouldInvokeNone()
    {
        Maybe<int> maybe = Maybe<int>.None;

        string result = await maybe.Match(
            (v, ct) => ValueTask.FromResult(v.ToString()),
            ct => ValueTask.FromResult("none"));

        result.Should().Be("none");
    }

    [Fact]
    public async Task MatchAsync_ValueTaskTE_WithContext_WithValue_ShouldInvokeSome()
    {
        var maybe = Maybe<int>.From(5);

        string result = await maybe.Match(
            (v, c, ct) => ValueTask.FromResult(v + c),
            (c, ct) => ValueTask.FromResult("no-" + c),
            "_ctx");

        result.Should().Be("5_ctx");
    }

    [Fact]
    public async Task MatchAsync_ValueTaskTE_WithContext_WithNoValue_ShouldInvokeNone()
    {
        Maybe<int> maybe = Maybe<int>.None;

        string result = await maybe.Match(
            (v, c, ct) => ValueTask.FromResult(v + c),
            (c, ct) => ValueTask.FromResult("no-" + c),
            "_ctx");

        result.Should().Be("no-_ctx");
    }

    #endregion

    #region Match ValueTask (void) returning

    [Fact]
    public async Task MatchAsync_ValueTask_WithValue_ShouldInvokeSome()
    {
        var maybe = Maybe<int>.From(9);
        int captured = 0;

        await maybe.Match(
            (v, ct) => { captured = v; return ValueTask.CompletedTask; },
            ct => { captured = -1; return ValueTask.CompletedTask; });

        captured.Should().Be(9);
    }

    [Fact]
    public async Task MatchAsync_ValueTask_WithNoValue_ShouldInvokeNone()
    {
        Maybe<int> maybe = Maybe<int>.None;
        int captured = 0;

        await maybe.Match(
            (v, ct) => { captured = v; return ValueTask.CompletedTask; },
            ct => { captured = -1; return ValueTask.CompletedTask; });

        captured.Should().Be(-1);
    }

    [Fact]
    public async Task MatchAsync_ValueTask_WithContext_WithValue_ShouldInvokeSome()
    {
        var maybe = Maybe<int>.From(4);
        int captured = 0;

        await maybe.Match(
            (v, c, ct) => { captured = v * c; return ValueTask.CompletedTask; },
            (c, ct) => { captured = c; return ValueTask.CompletedTask; },
            3);

        captured.Should().Be(12);
    }

    [Fact]
    public async Task MatchAsync_ValueTask_WithContext_WithNoValue_ShouldInvokeNone()
    {
        Maybe<int> maybe = Maybe<int>.None;
        int captured = 0;

        await maybe.Match(
            (v, c, ct) => { captured = v * c; return ValueTask.CompletedTask; },
            (c, ct) => { captured = c; return ValueTask.CompletedTask; },
            3);

        captured.Should().Be(3);
    }

    #endregion

    #region KeyValuePair Match async extensions

    [Fact]
    public async Task MatchAsync_KeyValuePair_ValueTask_WithValue_ShouldInvokeSome()
    {
        var kvp = new KeyValuePair<string, int>("key", 42);
        var maybe = Maybe<KeyValuePair<string, int>>.From(kvp);
        string? capturedKey = null;
        int capturedValue = 0;

        await maybe.Match(
            (k, v, ct) => { capturedKey = k; capturedValue = v; return ValueTask.CompletedTask; },
            ct => ValueTask.CompletedTask);

        capturedKey.Should().Be("key");
        capturedValue.Should().Be(42);
    }

    [Fact]
    public async Task MatchAsync_KeyValuePair_ValueTask_WithNoValue_ShouldInvokeNone()
    {
        Maybe<KeyValuePair<string, int>> maybe = Maybe<KeyValuePair<string, int>>.None;
        bool noneCalled = false;

        await maybe.Match(
            (k, v, ct) => ValueTask.CompletedTask,
            ct => { noneCalled = true; return ValueTask.CompletedTask; });

        noneCalled.Should().BeTrue();
    }

    [Fact]
    public async Task MatchAsync_KeyValuePair_ValueTaskTE_WithValue_ShouldReturnSomeResult()
    {
        var kvp = new KeyValuePair<string, int>("x", 10);
        var maybe = Maybe<KeyValuePair<string, int>>.From(kvp);

        string result = await maybe.Match(
            (k, v, ct) => ValueTask.FromResult($"{k}={v}"),
            ct => ValueTask.FromResult("none"));

        result.Should().Be("x=10");
    }

    [Fact]
    public async Task MatchAsync_KeyValuePair_ValueTaskTE_WithNoValue_ShouldReturnNoneResult()
    {
        Maybe<KeyValuePair<string, int>> maybe = Maybe<KeyValuePair<string, int>>.None;

        string result = await maybe.Match(
            (k, v, ct) => ValueTask.FromResult($"{k}={v}"),
            ct => ValueTask.FromResult("none"));

        result.Should().Be("none");
    }

    #endregion
}
