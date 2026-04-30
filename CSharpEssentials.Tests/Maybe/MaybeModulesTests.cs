using CSharpEssentials.Maybe;
using FluentAssertions;
using System.Globalization;

namespace CSharpEssentials.Tests.Maybe;

public class MaybeModulesTests
{
    #region Map

    [Fact]
    public void Map_WithValue_ShouldTransformValue()
    {
        var maybe = Maybe<int>.From(5);

        Maybe<int> result = maybe.Map(x => x * 2);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public void Map_WithNoValue_ShouldReturnNone()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Maybe<int> result = maybe.Map(x => x * 2);

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region Bind

    [Fact]
    public void Bind_WithValue_ShouldChainOperations()
    {
        var maybe = Maybe<int>.From(5);

        Maybe<string> result = maybe.Bind(x => Maybe<string>.From(x.ToString(CultureInfo.InvariantCulture)));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be("5");
    }

    [Fact]
    public void Bind_WithNoValue_ShouldReturnNone()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Maybe<string> result = maybe.Bind(x => Maybe<string>.From(x.ToString(CultureInfo.InvariantCulture)));

        result.HasNoValue.Should().BeTrue();
    }

    [Fact]
    public void Bind_ReturningNone_ShouldReturnNone()
    {
        var maybe = Maybe<int>.From(5);

        Maybe<string> result = maybe.Bind(_ => Maybe<string>.None);

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region Match

    [Fact]
    public void Match_WithValue_ShouldCallSomeFunc()
    {
        var maybe = Maybe<int>.From(42);

        string result = maybe.Match(
            some: x => $"Value: {x}",
            none: () => "No value");

        result.Should().Be("Value: 42");
    }

    [Fact]
    public void Match_WithNoValue_ShouldCallNoneFunc()
    {
        Maybe<int> maybe = Maybe<int>.None;

        string result = maybe.Match(
            some: x => $"Value: {x}",
            none: () => "No value");

        result.Should().Be("No value");
    }

    #endregion

    #region Where

    [Fact]
    public void Where_WithMatchingPredicate_ShouldReturnSome()
    {
        var maybe = Maybe<int>.From(10);

        Maybe<int> result = maybe.Where(x => x > 5);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public void Where_WithNonMatchingPredicate_ShouldReturnNone()
    {
        var maybe = Maybe<int>.From(3);

        Maybe<int> result = maybe.Where(x => x > 5);

        result.HasNoValue.Should().BeTrue();
    }

    [Fact]
    public void Where_WithNoValue_ShouldReturnNone()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Maybe<int> result = maybe.Where(x => x > 5);

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region Or

    [Fact]
    public void Or_WithValue_ShouldReturnOriginalValue()
    {
        var maybe = Maybe<int>.From(42);

        Maybe<int> result = maybe.Or(Maybe<int>.From(100));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Or_WithNoValue_ShouldReturnAlternative()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Maybe<int> result = maybe.Or(Maybe<int>.From(100));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(100);
    }

    [Fact]
    public void Or_WithFunc_ShouldCallFuncOnNone()
    {
        Maybe<int> maybe = Maybe<int>.None;
        bool funcCalled = false;

        Maybe<int> result = maybe.Or(() =>
        {
            funcCalled = true;
            return 100;
        });

        funcCalled.Should().BeTrue();
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(100);
    }

    [Fact]
    public void Or_WithFunc_ShouldNotCallFuncOnSome()
    {
        var maybe = Maybe<int>.From(42);
        bool funcCalled = false;

        Maybe<int> result = maybe.Or(() =>
        {
            funcCalled = true;
            return 100;
        });

        funcCalled.Should().BeFalse();
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    #endregion

    #region Execute

    [Fact]
    public void Execute_WithValue_ShouldCallAction()
    {
        var maybe = Maybe<int>.From(42);
        int capturedValue = 0;

        maybe.Execute(x => capturedValue = x);

        capturedValue.Should().Be(42);
    }

    [Fact]
    public void Execute_WithNoValue_ShouldNotCallAction()
    {
        Maybe<int> maybe = Maybe<int>.None;
        int capturedValue = -1;

        maybe.Execute(x => capturedValue = x);

        capturedValue.Should().Be(-1);
    }

    #endregion

    #region ToList

    [Fact]
    public void ToList_WithValue_ShouldReturnListWithOneItem()
    {
        var maybe = Maybe<int>.From(42);

        var result = maybe.ToList();

        result.Should().HaveCount(1);
        result[0].Should().Be(42);
    }

    [Fact]
    public void ToList_WithNoValue_ShouldReturnEmptyList()
    {
        Maybe<int> maybe = Maybe<int>.None;

        var result = maybe.ToList();

        result.Should().BeEmpty();
    }

    #endregion

    #region AsNullable

    [Fact]
    public void AsNullable_WithValue_ShouldReturnValue()
    {
        var maybe = Maybe<string>.From("test");

        string? result = maybe.AsNullable();

        result.Should().Be("test");
    }

    [Fact]
    public void AsNullable_WithNoValue_ShouldReturnNull()
    {
        Maybe<string> maybe = Maybe<string>.None;

        string? result = maybe.AsNullable();

        result.Should().BeNull();
    }

    #endregion

    #region Select (LINQ)

    [Fact]
    public void Select_ShouldWorkLikeMap()
    {
        var maybe = Maybe<int>.From(5);

        Maybe<int> result = maybe.Select(x => x * 2);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    #endregion
}
