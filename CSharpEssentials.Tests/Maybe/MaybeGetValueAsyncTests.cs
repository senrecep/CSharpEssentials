using CSharpEssentials.Maybe;
using FluentAssertions;
using System.Globalization;

namespace CSharpEssentials.Tests.Maybe;

public class MaybeGetValueAsyncTests
{
    #region GetValueOrDefaultAsync — Func<Task<T>>

    [Fact]
    public async Task GetValueOrDefaultAsync_FuncTaskT_WithValue_ShouldReturnValue()
    {
        var maybe = Maybe<int>.From(42);

        int result = await maybe.GetValueOrDefaultAsync(() => Task.FromResult(0));

        result.Should().Be(42);
    }

    [Fact]
    public async Task GetValueOrDefaultAsync_FuncTaskT_WithNoValue_ShouldReturnDefault()
    {
        Maybe<int> maybe = Maybe<int>.None;

        int result = await maybe.GetValueOrDefaultAsync(() => Task.FromResult(99));

        result.Should().Be(99);
    }

    #endregion

    #region GetValueOrDefaultAsync<TOut> — sync selector + Func<Task<TOut>>

    [Fact]
    public async Task GetValueOrDefaultAsync_SyncSelectorFuncTask_WithValue_ShouldApplySelector()
    {
        var maybe = Maybe<int>.From(5);

        string result = await maybe.GetValueOrDefaultAsync(
            v => v.ToString(CultureInfo.InvariantCulture),
            () => Task.FromResult("none"));

        result.Should().Be("5");
    }

    [Fact]
    public async Task GetValueOrDefaultAsync_SyncSelectorFuncTask_WithNoValue_ShouldReturnDefault()
    {
        Maybe<int> maybe = Maybe<int>.None;

        string result = await maybe.GetValueOrDefaultAsync(
            v => v.ToString(CultureInfo.InvariantCulture),
            () => Task.FromResult("none"));

        result.Should().Be("none");
    }

    #endregion

    #region GetValueOrDefaultAsync<TOut> — async selector + sync default

    [Fact]
    public async Task GetValueOrDefaultAsync_AsyncSelectorSyncDefault_WithValue_ShouldApplySelector()
    {
        var maybe = Maybe<int>.From(7);

        string result = await maybe.GetValueOrDefaultAsync(
            v => Task.FromResult(v.ToString(CultureInfo.InvariantCulture)),
            "none");

        result.Should().Be("7");
    }

    [Fact]
    public async Task GetValueOrDefaultAsync_AsyncSelectorSyncDefault_WithNoValue_ShouldReturnDefault()
    {
        Maybe<int> maybe = Maybe<int>.None;

        string result = await maybe.GetValueOrDefaultAsync(
            v => Task.FromResult(v.ToString(CultureInfo.InvariantCulture)),
            "none");

        result.Should().Be("none");
    }

    #endregion

    #region GetValueOrDefaultAsync extension on Task<Maybe<T>> — sync default

    [Fact]
    public async Task GetValueOrDefaultAsync_TaskMaybe_SyncDefault_WithValue_ShouldReturnValue()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(10));

        int result = await maybeTask.GetValueOrDefaultAsync(() => 0);

        result.Should().Be(10);
    }

    [Fact]
    public async Task GetValueOrDefaultAsync_TaskMaybe_SyncDefault_WithNoValue_ShouldReturnDefault()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.None);

        int result = await maybeTask.GetValueOrDefaultAsync(() => 99);

        result.Should().Be(99);
    }

    #endregion

    #region GetValueOrDefaultAsync extension on Task<Maybe<T>> — selector + sync default value

    [Fact]
    public async Task GetValueOrDefaultAsync_TaskMaybe_SelectorSyncDefault_WithValue_ShouldApplySelector()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(3));

        string result = await maybeTask.GetValueOrDefaultAsync(v => v.ToString(CultureInfo.InvariantCulture), "none");

        result.Should().Be("3");
    }

    [Fact]
    public async Task GetValueOrDefaultAsync_TaskMaybe_SelectorSyncDefault_WithNoValue_ShouldReturnDefault()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.None);

        string result = await maybeTask.GetValueOrDefaultAsync(v => v.ToString(CultureInfo.InvariantCulture), "none");

        result.Should().Be("none");
    }

    #endregion

    #region GetValueOrDefaultAsync extension on Task<Maybe<T>> — selector + Func default

    [Fact]
    public async Task GetValueOrDefaultAsync_TaskMaybe_SelectorFuncDefault_WithValue_ShouldApplySelector()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(8));

        string result = await maybeTask.GetValueOrDefaultAsync(v => v.ToString(CultureInfo.InvariantCulture), () => "none");

        result.Should().Be("8");
    }

    [Fact]
    public async Task GetValueOrDefaultAsync_TaskMaybe_SelectorFuncDefault_WithNoValue_ShouldReturnDefault()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.None);

        string result = await maybeTask.GetValueOrDefaultAsync(v => v.ToString(CultureInfo.InvariantCulture), () => "none");

        result.Should().Be("none");
    }

    #endregion
}
