using FluentAssertions;
using static CSharpEssentials.Tests.TestData;
using CSharpEssentials.Core;

namespace CSharpEssentials.Tests.Core;

public class CollectionExtensionsTests
{
    private static readonly int[] ExpectedAddRangeItems = [4, 5, 6];
    private static readonly int[] ExpectedForEachDoubled = [2, 4, 6];
    private static readonly int[] ExpectedForEachOriginal = [1, 2, 3];
    private static readonly int[] ExpectedWhereIfFiltered = [4, 5];
    private static readonly int[] ExpectedWhereIfIndexed = [1, 3, 5];
    private static readonly string[] ExpectedWithoutNulls = ["a", "c", "e"];

    [Fact]
    public void IfAdd_WhenConditionIsTrue_ShouldAddItem()
    {
        List<int> list = [1, 2, 3];
        ICollection<int> result = list.IfAdd(true, 4);

        list.Should().Contain(4);
        result.Should().BeSameAs(list);
    }

    [Fact]
    public void IfAdd_WhenConditionIsFalse_ShouldNotAddItem()
    {
        List<int> list = [1, 2, 3];
        ICollection<int> result = list.IfAdd(false, 4);

        list.Should().NotContain(4);
        result.Should().BeSameAs(list);
    }

    [Fact]
    public void IfAddRange_WhenConditionIsTrue_ShouldAddItems()
    {
        List<int> list = [1, 2, 3];
        ICollection<int> result = list.IfAddRange(true, 4, 5, 6);

        list.Should().Contain(ExpectedAddRangeItems);
        result.Should().BeSameAs(list);
    }

    [Fact]
    public void IfAddRange_WhenConditionIsFalse_ShouldNotAddItems()
    {
        List<int> list = [1, 2, 3];
        ICollection<int> result = list.IfAddRange(false, 4, 5, 6);

        list.Should().NotContain(ExpectedAddRangeItems);
        result.Should().BeSameAs(list);
    }

    [Fact]
    public void ForEach_ShouldExecuteActionAndReturnItems()
    {
        List<int> list = [1, 2, 3];
        List<int> executed = [];

        IEnumerable<int> result = CSharpEssentials.Core.CollectionExtensions.ForEach(list, x => executed.Add(x * 2));
        var resultList = result.ToList();

        executed.Should().BeEquivalentTo(ExpectedForEachDoubled);
        resultList.Should().BeEquivalentTo(ExpectedForEachOriginal);
    }

    [Fact]
    public void WhereIf_WithIEnumerable_WhenConditionIsTrue_ShouldFilter()
    {
        int[] source = [1, 2, 3, 4, 5];
        var result = source.WhereIf(true, x => x > 3).ToList();

        result.Should().BeEquivalentTo(ExpectedWhereIfFiltered);
    }

    [Fact]
    public void WhereIf_WithIEnumerable_WhenConditionIsFalse_ShouldReturnAll()
    {
        int[] source = [1, 2, 3, 4, 5];
        var result = source.WhereIf(false, x => x > 3).ToList();

        result.Should().BeEquivalentTo(source);
    }

    [Fact]
    public void WhereIf_WithIndex_ShouldWorkCorrectly()
    {
        int[] source = [1, 2, 3, 4, 5];
        var result = source.WhereIf(true, (x, i) => i % 2 == 0).ToList();

        result.Should().BeEquivalentTo(ExpectedWhereIfIndexed);
    }

    [Fact]
    public void WithoutNulls_ShouldRemoveNulls()
    {
        string?[] source = Collections.NullableStringArray;
        var result = source.WithoutNulls().ToList();

        result.Should().NotContainNulls();
        result.Should().BeEquivalentTo(ExpectedWithoutNulls);
    }

    [Fact]
    public void WithoutNulls_WithPropertySelector_ShouldFilterByProperty()
    {
        var source = new[] { new { Name = "a", Value = (string?)"x" }, new { Name = "b", Value = (string?)null }, new { Name = "c", Value = (string?)"y" } };
        var result = source.WithoutNulls(x => x.Value).ToList();

        result.Should().HaveCount(2);
        result.Should().OnlyContain(x => x.Value != null);
    }

    [Fact]
    public void HasSameElements_WithSameElements_ShouldReturnTrue()
    {
        int[] source1 = [1, 2, 3];
        int[] source2 = [3, 2, 1];

        source1.HasSameElements(source2).Should().BeTrue();
    }

    [Fact]
    public void HasSameElements_WithDifferentElements_ShouldReturnFalse()
    {
        int[] source1 = [1, 2, 3];
        int[] source2 = [1, 2, 4];

        source1.HasSameElements(source2).Should().BeFalse();
    }

    [Fact]
    public void HasSameElements_WithDuplicates_ShouldHandleCorrectly()
    {
        int[] source1 = [1, 2, 2, 3];
        int[] source2 = [1, 2, 3, 3];

        source1.HasSameElements(source2).Should().BeTrue();
    }

    [Fact]
    public void AllTrue_WhenAllTrue_ShouldReturnTrue()
    {
        bool[] source = [true, true, true];
        source.AllTrue().Should().BeTrue();
    }

    [Fact]
    public void AllTrue_WhenAnyFalse_ShouldReturnFalse()
    {
        bool[] source = [true, false, true];
        source.AllTrue().Should().BeFalse();
    }

    [Fact]
    public void AllTrue_WithEmptyCollection_ShouldReturnTrue()
    {
        bool[] source = [];
        source.AllTrue().Should().BeTrue();
    }

    [Fact]
    public void AllFalse_WhenAllFalse_ShouldReturnTrue()
    {
        bool[] source = [false, false, false];
        source.AllFalse().Should().BeTrue();
    }

    [Fact]
    public void AllFalse_WhenAnyTrue_ShouldReturnFalse()
    {
        bool[] source = [false, true, false];
        source.AllFalse().Should().BeFalse();
    }

    [Fact]
    public void AllFalse_WithEmptyCollection_ShouldReturnTrue()
    {
        bool[] source = [];
        source.AllFalse().Should().BeTrue();
    }
}
