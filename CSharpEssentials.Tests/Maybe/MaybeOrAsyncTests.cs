using CSharpEssentials.Maybe;
using FluentAssertions;

namespace CSharpEssentials.Tests.Maybe;

public class MaybeOrAsyncTests
{
    #region Or async — Task<T> fallback operation

    [Fact]
    public async Task Or_TaskT_WithValue_ShouldReturnOriginal()
    {
        var maybe = Maybe<int>.From(5);

        Maybe<int> result = await maybe.Or(() => Task.FromResult(99));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    [Fact]
    public async Task Or_TaskT_WithNoValue_ShouldReturnFallback()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Maybe<int> result = await maybe.Or(() => Task.FromResult(99));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(99);
    }

    #endregion

    #region Or async — Task<Maybe<T>> fallback

    [Fact]
    public async Task Or_TaskMaybe_WithValue_ShouldReturnOriginal()
    {
        var maybe = Maybe<int>.From(1);

        Maybe<int> result = await maybe.Or(Task.FromResult(Maybe<int>.From(99)));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(1);
    }

    [Fact]
    public async Task Or_TaskMaybe_WithNoValue_ShouldReturnFallback()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Maybe<int> result = await maybe.Or(Task.FromResult(Maybe<int>.From(99)));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(99);
    }

    #endregion

    #region Or async — Func<Task<Maybe<T>>> fallback operation

    [Fact]
    public async Task Or_FuncTaskMaybe_WithValue_ShouldReturnOriginal()
    {
        var maybe = Maybe<int>.From(2);

        Maybe<int> result = await maybe.Or(() => Task.FromResult(Maybe<int>.From(99)));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(2);
    }

    [Fact]
    public async Task Or_FuncTaskMaybe_WithNoValue_ShouldReturnFallback()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Maybe<int> result = await maybe.Or(() => Task.FromResult(Maybe<int>.From(99)));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(99);
    }

    #endregion

    #region Or async — ValueTask<T> fallback operation

    [Fact]
    public async Task Or_ValueTaskT_WithValue_ShouldReturnOriginal()
    {
        var maybe = Maybe<int>.From(3);

        Maybe<int> result = await maybe.Or(() => ValueTask.FromResult(99));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(3);
    }

    [Fact]
    public async Task Or_ValueTaskT_WithNoValue_ShouldReturnFallback()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Maybe<int> result = await maybe.Or(() => ValueTask.FromResult(99));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(99);
    }

    #endregion

    #region Or async — ValueTask<Maybe<T>> fallback

    [Fact]
    public async Task Or_ValueTaskMaybe_WithValue_ShouldReturnOriginal()
    {
        var maybe = Maybe<int>.From(4);

        Maybe<int> result = await maybe.Or(ValueTask.FromResult(Maybe<int>.From(99)));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(4);
    }

    [Fact]
    public async Task Or_ValueTaskMaybe_WithNoValue_ShouldReturnFallback()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Maybe<int> result = await maybe.Or(ValueTask.FromResult(Maybe<int>.From(99)));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(99);
    }

    #endregion

    #region Or async — Func<ValueTask<Maybe<T>>> fallback operation

    [Fact]
    public async Task Or_FuncValueTaskMaybe_WithValue_ShouldReturnOriginal()
    {
        var maybe = Maybe<int>.From(6);

        Maybe<int> result = await maybe.Or(() => ValueTask.FromResult(Maybe<int>.From(99)));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(6);
    }

    [Fact]
    public async Task Or_FuncValueTaskMaybe_WithNoValue_ShouldReturnFallback()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Maybe<int> result = await maybe.Or(() => ValueTask.FromResult(Maybe<int>.From(99)));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(99);
    }

    #endregion

    #region Or extension on Task<Maybe<T>>

    [Fact]
    public async Task Or_TaskMaybeExtension_WithValue_ShouldReturnOriginal()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(7));

        Maybe<int> result = await maybeTask.Or(99);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(7);
    }

    [Fact]
    public async Task Or_TaskMaybeExtension_WithNoValue_ShouldReturnFallback()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.None);

        Maybe<int> result = await maybeTask.Or(99);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(99);
    }

    #endregion
}
