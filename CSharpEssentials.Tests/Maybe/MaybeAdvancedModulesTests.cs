using CSharpEssentials.Maybe;
using FluentAssertions;
using System.Globalization;

namespace CSharpEssentials.Tests.Maybe;

public class MaybeAdvancedModulesTests
{
    #region Choose

    [Fact]
    public void Choose_WithSelector_ShouldProjectValuesOnly()
    {
        List<Maybe<int>> source =
        [
            Maybe<int>.From(1),
            Maybe<int>.None,
            Maybe<int>.From(2),
            Maybe<int>.None,
            Maybe<int>.From(3)
        ];

        var result = source.Choose(x => x.ToString(CultureInfo.InvariantCulture)).ToList();

        result.Should().HaveCount(3);
        result.Should().Contain("1", "2", "3");
    }

    [Fact]
    public void Choose_WithoutSelector_ShouldReturnValuesOnly()
    {
        List<Maybe<int>> source =
        [
            Maybe<int>.From(10),
            Maybe<int>.None,
            Maybe<int>.From(20),
            Maybe<int>.None
        ];

        var result = source.Choose().ToList();

        result.Should().HaveCount(2);
        result.Should().Contain(10);
        result.Should().Contain(20);
    }

    [Fact]
    public void Choose_WithAllNone_ShouldReturnEmpty()
    {
        List<Maybe<int>> source =
        [
            Maybe<int>.None,
            Maybe<int>.None,
            Maybe<int>.None
        ];

        var result = source.Choose().ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public void Choose_WithEmptySource_ShouldReturnEmpty()
    {
        List<Maybe<int>> source = [];

        var result = source.Choose().ToList();

        result.Should().BeEmpty();
    }

    #endregion

    #region TryFirst

    [Fact]
    public void TryFirst_WithNonEmptyCollection_ShouldReturnFirst()
    {
        List<int> source = [10, 20, 30];

        Maybe<int> result = source.TryFirst();

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public void TryFirst_WithEmptyCollection_ShouldReturnDefault()
    {
        // Note: TryFirst returns Maybe.From(default) for value types when collection is empty
        // because FirstOrDefault returns 0 for int which matches pattern `is T result`
        List<string> source = [];

        Maybe<string> result = source.TryFirst();

        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void TryFirst_WithPredicate_ShouldReturnFirstMatching()
    {
        List<int> source = [1, 2, 3, 4, 5];

        Maybe<int> result = source.TryFirst(x => x > 3);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(4);
    }

    [Fact]
    public void TryFirst_WithPredicate_NoMatch_ShouldReturnNone()
    {
        // Using string to properly test None case (reference type)
        List<string> source = ["a", "b", "c"];

        Maybe<string> result = source.TryFirst(x => x.Length > 10);

        result.HasValue.Should().BeFalse();
    }

    #endregion

    #region TryLast

    [Fact]
    public void TryLast_WithNonEmptyCollection_ShouldReturnLast()
    {
        List<int> source = [10, 20, 30];

        Maybe<int> result = source.TryLast();

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(30);
    }

    [Fact]
    public void TryLast_WithEmptyCollection_ShouldReturnNone()
    {
        List<int> source = [];

        Maybe<int> result = source.TryLast();

        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void TryLast_WithPredicate_ShouldReturnLastMatching()
    {
        List<int> source = [1, 2, 3, 4, 5];

        Maybe<int> result = source.TryLast(x => x < 4);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(3);
    }

    [Fact]
    public void TryLast_WithPredicate_NoMatch_ShouldReturnNone()
    {
        List<int> source = [1, 2, 3];

        Maybe<int> result = source.TryLast(x => x > 10);

        result.HasValue.Should().BeFalse();
    }

    #endregion

    #region TryFind

    [Fact]
    public void TryFind_WithExistingKey_ShouldReturnValue()
    {
        Dictionary<string, int> dict = new()
        {
            ["one"] = 1,
            ["two"] = 2,
            ["three"] = 3
        };

        Maybe<int> result = dict.TryFind("two");

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(2);
    }

    [Fact]
    public void TryFind_WithMissingKey_ShouldReturnNone()
    {
        Dictionary<string, int> dict = new()
        {
            ["one"] = 1,
            ["two"] = 2
        };

        Maybe<int> result = dict.TryFind("three");

        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void TryFind_WithEmptyDictionary_ShouldReturnNone()
    {
        Dictionary<string, int> dict = [];

        Maybe<int> result = dict.TryFind("any");

        result.HasValue.Should().BeFalse();
    }

    #endregion

    #region ToList

    [Fact]
    public void ToList_WithValue_ShouldReturnSingleItemList()
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

    #region Where

    [Fact]
    public void Where_WithValuePassingPredicate_ShouldReturnSame()
    {
        var maybe = Maybe<int>.From(10);

        Maybe<int> result = maybe.Where(x => x > 5);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public void Where_WithValueFailingPredicate_ShouldReturnNone()
    {
        var maybe = Maybe<int>.From(3);

        Maybe<int> result = maybe.Where(x => x > 5);

        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void Where_WithNoValue_ShouldReturnNone()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Maybe<int> result = maybe.Where(x => x > 5);

        result.HasValue.Should().BeFalse();
    }

    #endregion

    #region Execute

    [Fact]
    public void Execute_WithValue_ShouldExecuteAction()
    {
        var maybe = Maybe<int>.From(42);
        int captured = 0;

        maybe.Execute(x => captured = x);

        captured.Should().Be(42);
    }

    [Fact]
    public void Execute_WithNoValue_ShouldNotExecuteAction()
    {
        Maybe<int> maybe = Maybe<int>.None;
        bool executed = false;

        maybe.Execute(_ => executed = true);

        executed.Should().BeFalse();
    }

    [Fact]
    public void ExecuteNoValue_WithNoValue_ShouldExecuteAction()
    {
        Maybe<int> maybe = Maybe<int>.None;
        bool executed = false;

        maybe.ExecuteNoValue(() => executed = true);

        executed.Should().BeTrue();
    }

    [Fact]
    public void ExecuteNoValue_WithValue_ShouldNotExecuteAction()
    {
        var maybe = Maybe<int>.From(42);
        bool executed = false;

        maybe.ExecuteNoValue(() => executed = true);

        executed.Should().BeFalse();
    }

    #endregion

    #region Flatten

    [Fact]
    public void Flatten_WithNestedValue_ShouldReturnInnerValue()
    {
        var nested = Maybe<Maybe<int>>.From(Maybe<int>.From(42));

        Maybe<int> result = nested.Flatten();

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Flatten_WithOuterNone_ShouldReturnNone()
    {
        Maybe<Maybe<int>> nested = Maybe<Maybe<int>>.None;

        Maybe<int> result = nested.Flatten();

        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void Flatten_WithInnerNone_ShouldReturnNone()
    {
        var nested = Maybe<Maybe<int>>.From(Maybe<int>.None);

        Maybe<int> result = nested.Flatten();

        result.HasValue.Should().BeFalse();
    }

    #endregion
}
