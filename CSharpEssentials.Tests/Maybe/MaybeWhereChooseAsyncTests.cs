using CSharpEssentials.Maybe;
using FluentAssertions;
using System.Globalization;

namespace CSharpEssentials.Tests.Maybe;

public class MaybeWhereChooseAsyncTests
{
    #region Where — Func<T, Task<bool>>

    [Fact]
    public async Task Where_TaskPredicate_WithValue_PredicateTrue_ShouldReturnValue()
    {
        var maybe = Maybe<int>.From(10);

        Maybe<int> result = await maybe.Where(v => Task.FromResult(v > 5));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task Where_TaskPredicate_WithValue_PredicateFalse_ShouldReturnNone()
    {
        var maybe = Maybe<int>.From(3);

        Maybe<int> result = await maybe.Where(v => Task.FromResult(v > 5));

        result.HasNoValue.Should().BeTrue();
    }

    [Fact]
    public async Task Where_TaskPredicate_WithNoValue_ShouldReturnNone()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Maybe<int> result = await maybe.Where(v => Task.FromResult(v > 5));

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region Where — Func<T, ValueTask<bool>>

    [Fact]
    public async Task Where_ValueTaskPredicate_WithValue_PredicateTrue_ShouldReturnValue()
    {
        var maybe = Maybe<string>.From("hello");

        Maybe<string> result = await maybe.Where(v => ValueTask.FromResult(v.Length > 3));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be("hello");
    }

    [Fact]
    public async Task Where_ValueTaskPredicate_WithValue_PredicateFalse_ShouldReturnNone()
    {
        var maybe = Maybe<string>.From("hi");

        Maybe<string> result = await maybe.Where(v => ValueTask.FromResult(v.Length > 3));

        result.HasNoValue.Should().BeTrue();
    }

    [Fact]
    public async Task Where_ValueTaskPredicate_WithNoValue_ShouldReturnNone()
    {
        Maybe<string> maybe = Maybe<string>.None;

        Maybe<string> result = await maybe.Where(v => ValueTask.FromResult(v.Length > 3));

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region Where extension on Task<Maybe<T>>

    [Fact]
    public async Task Where_TaskMaybe_SyncPredicate_WithValue_PredicateTrue_ShouldReturnValue()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(20));

        Maybe<int> result = await maybeTask.Where(v => v > 10);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(20);
    }

    [Fact]
    public async Task Where_TaskMaybe_SyncPredicate_WithValue_PredicateFalse_ShouldReturnNone()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.Where(v => v > 10);

        result.HasNoValue.Should().BeTrue();
    }

    [Fact]
    public async Task Where_TaskMaybe_SyncPredicate_WithNoValue_ShouldReturnNone()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.None);

        Maybe<int> result = await maybeTask.Where(v => v > 10);

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region ChooseAsync — IEnumerable<Task<Maybe<T>>>

    [Fact]
    public async Task ChooseAsync_WithMixedTasks_ShouldReturnOnlyValues()
    {
        var tasks = new List<Task<Maybe<int>>>
        {
            Task.FromResult(Maybe<int>.From(1)),
            Task.FromResult(Maybe<int>.None),
            Task.FromResult(Maybe<int>.From(3)),
            Task.FromResult(Maybe<int>.None),
            Task.FromResult(Maybe<int>.From(5))
        };

        var results = new List<int>();
        await foreach (int value in tasks.ChooseAsync())
            results.Add(value);

        results.Should().Equal(1, 3, 5);
    }

    [Fact]
    public async Task ChooseAsync_AllNone_ShouldReturnEmpty()
    {
        var tasks = new List<Task<Maybe<int>>>
        {
            Task.FromResult(Maybe<int>.None),
            Task.FromResult(Maybe<int>.None)
        };

        var results = new List<int>();
        await foreach (int value in tasks.ChooseAsync())
            results.Add(value);

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task ChooseAsync_WithSelector_ShouldTransformValues()
    {
        var tasks = new List<Task<Maybe<int>>>
        {
            Task.FromResult(Maybe<int>.From(2)),
            Task.FromResult(Maybe<int>.None),
            Task.FromResult(Maybe<int>.From(4))
        };

        var results = new List<string>();
        await foreach (string value in tasks.ChooseAsync(v => v.ToString(CultureInfo.InvariantCulture)))
            results.Add(value);

        results.Should().Equal("2", "4");
    }

    #endregion
}
