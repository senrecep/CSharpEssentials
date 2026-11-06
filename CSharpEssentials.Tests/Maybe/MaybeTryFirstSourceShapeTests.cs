using System.Collections.ObjectModel;
using CSharpEssentials.Maybe;
using FluentAssertions;

namespace CSharpEssentials.Tests.Maybe;

public class MaybeTryFirstSourceShapeTests
{
    public static TheoryData<string> Shapes() => ["array", "list", "readonly-collection", "readonly-list", "lazy"];

    private static IEnumerable<string?> Source(string shape, params string?[] items) => shape switch
    {
        "array" => items,
        "list" => new List<string?>(items),
        "readonly-collection" => new ReadOnlyCollection<string?>(items),
        "readonly-list" => new ReadOnlyListOnly<string?>(items),
        _ => Lazy(items),
    };

    private static IEnumerable<string?> Lazy(string?[] items)
    {
        foreach (string? item in items)
            yield return item;
    }

    [Theory]
    [MemberData(nameof(Shapes))]
    public void TryFirst_Should_Return_First_Match_And_Stop(string shape)
    {
        var seen = new List<string?>();

        Maybe<string?> result = Source(shape, "a", "bb", "cc", "d").TryFirst(item =>
        {
            seen.Add(item);
            return item!.Length == 2;
        });

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be("bb");
        seen.Should().Equal("a", "bb");
    }

    [Theory]
    [MemberData(nameof(Shapes))]
    public void TryFirst_Should_Return_None_On_Miss_After_Visiting_Every_Item(string shape)
    {
        int calls = 0;

        Maybe<string?> result = Source(shape, "a", "b", "c").TryFirst(_ => { calls++; return false; });

        result.HasNoValue.Should().BeTrue();
        calls.Should().Be(3);
    }

    [Theory]
    [MemberData(nameof(Shapes))]
    public void TryFirst_Should_Return_None_When_Match_Is_Null(string shape)
    {
        Maybe<string?> result = Source(shape, "a", null, "c").TryFirst(static item => item is null);

        result.HasNoValue.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(Shapes))]
    public void TryFirst_Should_Return_None_For_Empty_Source(string shape)
    {
        Source(shape).TryFirst(static _ => true).HasNoValue.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(Shapes))]
    public void TryFirst_Should_Propagate_Predicate_Exceptions(string shape)
    {
        Action act = () => Source(shape, "a").TryFirst(static _ => throw new InvalidOperationException("boom"));

        act.Should().Throw<InvalidOperationException>().WithMessage("boom");
    }

    [Theory]
    [MemberData(nameof(Shapes))]
    public void TryFirst_Without_Predicate_Should_Return_First_Item(string shape)
    {
        Maybe<string?> result = Source(shape, "a", "b").TryFirst();

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be("a");
    }

    [Theory]
    [MemberData(nameof(Shapes))]
    public void TryFirst_Without_Predicate_Should_Return_None_For_Empty_Source(string shape)
    {
        Source(shape).TryFirst().HasNoValue.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(Shapes))]
    public void TryFirst_Without_Predicate_Should_Return_None_When_First_Item_Is_Null(string shape)
    {
        Source(shape, null, "b").TryFirst().HasNoValue.Should().BeTrue();
    }

    private sealed class ReadOnlyListOnly<T>(IReadOnlyList<T> items) : IReadOnlyList<T>
    {
        public T this[int index] => items[index];

        public int Count => items.Count;

        public IEnumerator<T> GetEnumerator() => items.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
