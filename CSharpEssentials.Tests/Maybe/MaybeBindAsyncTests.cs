using CSharpEssentials.Maybe;
using FluentAssertions;

namespace CSharpEssentials.Tests.Maybe;

public class MaybeBindAsyncTests
{
    #region BindAsync on Maybe<T> — Task<Maybe<TOut>>

    [Fact]
    public async Task BindAsync_Task_WithValue_ShouldChainOperations()
    {
        var maybe = Maybe<int>.From(5);

        Maybe<string> result = await maybe.BindAsync(v => Task.FromResult(Maybe<string>.From(v.ToString())));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be("5");
    }

    [Fact]
    public async Task BindAsync_Task_WithNoValue_ShouldReturnNone()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Maybe<string> result = await maybe.BindAsync(v => Task.FromResult(Maybe<string>.From(v.ToString())));

        result.HasNoValue.Should().BeTrue();
    }

    [Fact]
    public async Task BindAsync_Task_ReturningNone_ShouldReturnNone()
    {
        var maybe = Maybe<int>.From(5);

        Maybe<string> result = await maybe.BindAsync(_ => Task.FromResult(Maybe<string>.None));

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region BindAsync extension on Task<Maybe<T>> — sync selector

    [Fact]
    public async Task BindAsync_TaskMaybe_SyncSelector_WithValue_ShouldChain()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(10));

        Maybe<int> result = await maybeTask.BindAsync(v => Maybe<int>.From(v * 2));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(20);
    }

    [Fact]
    public async Task BindAsync_TaskMaybe_SyncSelector_WithNoValue_ShouldReturnNone()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.None);

        Maybe<int> result = await maybeTask.BindAsync(v => Maybe<int>.From(v * 2));

        result.HasNoValue.Should().BeTrue();
    }

    [Fact]
    public async Task BindAsync_TaskMaybe_SyncSelector_ReturningNone_ShouldReturnNone()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(10));

        Maybe<int> result = await maybeTask.BindAsync(_ => Maybe<int>.None);

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region BindAsync extension on ValueTask<Maybe<T>> — ValueTask selector

    [Fact]
    public async Task BindAsync_ValueTaskMaybe_WithValue_ShouldChain()
    {
        ValueTask<Maybe<int>> maybeTask = ValueTask.FromResult(Maybe<int>.From(3));

        Maybe<string> result = await maybeTask.BindAsync(v => ValueTask.FromResult(Maybe<string>.From(v.ToString())));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be("3");
    }

    [Fact]
    public async Task BindAsync_ValueTaskMaybe_WithNoValue_ShouldReturnNone()
    {
        ValueTask<Maybe<int>> maybeTask = ValueTask.FromResult(Maybe<int>.None);

        Maybe<string> result = await maybeTask.BindAsync(v => ValueTask.FromResult(Maybe<string>.From(v.ToString())));

        result.HasNoValue.Should().BeTrue();
    }

    [Fact]
    public async Task BindAsync_ValueTaskMaybe_ReturningNone_ShouldReturnNone()
    {
        ValueTask<Maybe<int>> maybeTask = ValueTask.FromResult(Maybe<int>.From(3));

        Maybe<string> result = await maybeTask.BindAsync(_ => ValueTask.FromResult(Maybe<string>.None));

        result.HasNoValue.Should().BeTrue();
    }

    #endregion
}
