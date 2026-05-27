using CSharpEssentials.Maybe;
using FluentAssertions;

namespace CSharpEssentials.Tests.Maybe;

public class MaybeCollectionExtensionsTests
{
    [Fact]
    public void Sequence_AllValues_ShouldReturnMaybeArray()
    {
        Maybe<int>[] source = [1, 2, 3];

        Maybe<int[]> result = source.Sequence();

        result.HasValue.Should().BeTrue();
        result.Value.Should().Equal(1, 2, 3);
    }

    [Fact]
    public void Sequence_WithNone_ShouldReturnNone()
    {
        Maybe<int>[] source = [1, Maybe<int>.None, 3];

        Maybe<int[]> result = source.Sequence();

        result.HasNoValue.Should().BeTrue();
    }

    [Fact]
    public void Traverse_ShouldProjectAndSequence()
    {
        int[] source = [1, 2, 3];

        Maybe<string[]> result = source.Traverse(value => Maybe<string>.From($"item-{value}"));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Equal("item-1", "item-2", "item-3");
    }

    [Fact]
    public void Partition_ShouldReturnValuesAndNoneCount()
    {
        Maybe<int>[] source = [1, Maybe<int>.None, 3, Maybe<int>.None];

        (int[] values, int noneCount) = source.Partition();

        values.Should().Equal(1, 3);
        noneCount.Should().Be(2);
    }

    [Fact]
    public void CollectionExtensions_WithNullSource_ShouldThrowArgumentNullException()
    {
        IEnumerable<Maybe<int>> maybes = null!;
        IEnumerable<int> values = null!;

        Action[] actions =
        [
            () => maybes.Sequence(),
            () => maybes.Partition(),
            () => values.Traverse(value => Maybe<int>.From(value))
        ];

        foreach (Action action in actions)
            action.Should().Throw<ArgumentNullException>().WithParameterName("source");
    }
}
