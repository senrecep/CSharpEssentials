using CSharpEssentials.Maybe;
using FluentAssertions;

namespace CSharpEssentials.Tests.Maybe;

public class MaybeMapAsyncTests
{
    #region MapAsync on Maybe<T> — Task<TOut>

    [Fact]
    public async Task MapAsync_Task_WithValue_ShouldTransformValue()
    {
        var maybe = Maybe<int>.From(5);

        Maybe<string> result = await maybe.MapAsync(v => Task.FromResult(v.ToString()));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be("5");
    }

    [Fact]
    public async Task MapAsync_Task_WithNoValue_ShouldReturnNone()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Maybe<string> result = await maybe.MapAsync(v => Task.FromResult(v.ToString()));

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region MapAsync on Maybe<T> — ValueTask<TOut>

    [Fact]
    public async Task MapAsync_ValueTask_WithValue_ShouldTransformValue()
    {
        var maybe = Maybe<int>.From(10);

        Maybe<int> result = await maybe.MapAsync(v => ValueTask.FromResult(v * 2));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(20);
    }

    [Fact]
    public async Task MapAsync_ValueTask_WithNoValue_ShouldReturnNone()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Maybe<int> result = await maybe.MapAsync(v => ValueTask.FromResult(v * 2));

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region MapAsync extension on Task<Maybe<T>> — sync selector

    [Fact]
    public async Task MapAsync_TaskMaybe_SyncSelector_WithValue_ShouldTransformValue()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(3));

        Maybe<int> result = await maybeTask.MapAsync(v => v * 4);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(12);
    }

    [Fact]
    public async Task MapAsync_TaskMaybe_SyncSelector_WithNoValue_ShouldReturnNone()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.None);

        Maybe<int> result = await maybeTask.MapAsync(v => v * 4);

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region MapAsync extension on Task<Maybe<T>> — async selector

    [Fact]
    public async Task MapAsync_TaskMaybe_AsyncSelector_WithValue_ShouldTransformValue()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(7));

        Maybe<string> result = await maybeTask.MapAsync(v => Task.FromResult(v.ToString()));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be("7");
    }

    [Fact]
    public async Task MapAsync_TaskMaybe_AsyncSelector_WithNoValue_ShouldReturnNone()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.None);

        Maybe<string> result = await maybeTask.MapAsync(v => Task.FromResult(v.ToString()));

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region MapAsync extension on ValueTask<Maybe<T>>

    [Fact]
    public async Task MapAsync_ValueTaskMaybe_SyncSelector_WithValue_ShouldTransformValue()
    {
        ValueTask<Maybe<int>> maybeTask = ValueTask.FromResult(Maybe<int>.From(6));

        Maybe<int> result = await maybeTask.MapAsync(v => v + 1);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(7);
    }

    [Fact]
    public async Task MapAsync_ValueTaskMaybe_SyncSelector_WithNoValue_ShouldReturnNone()
    {
        ValueTask<Maybe<int>> maybeTask = ValueTask.FromResult(Maybe<int>.None);

        Maybe<int> result = await maybeTask.MapAsync(v => v + 1);

        result.HasNoValue.Should().BeTrue();
    }

    #endregion
}
