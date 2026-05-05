using CSharpEssentials.Core;
using FluentAssertions;
using static CSharpEssentials.Tests.TestData;

namespace CSharpEssentials.Tests.Core;

public class RandomItemsExtensionsTests
{
    [Fact]
    public void GetRandomItem_WithList_ShouldReturnItemFromList()
    {
        List<int> list = Collections.IntList;
        int result = list.GetRandomItem();

        list.Should().Contain(result);
    }

    [Fact]
    public void GetRandomItem_WithArray_ShouldReturnItemFromArray()
    {
        int[] array = Collections.IntArray;
        int result = array.GetRandomItem();

        array.Should().Contain(result);
    }

    [Fact]
    public void GetRandomItem_WithSpan_ShouldReturnItemFromSpan()
    {
        Span<int> span = stackalloc int[] { 1, 2, 3, 4, 5 };
        int result = span.GetRandomItem();

        span.ToArray().Should().Contain(result);
    }

    [Fact]
    public void GetRandomItems_WithList_ShouldReturnCorrectCount()
    {
        List<int> list = Collections.IntList;
        int[] result = list.GetRandomItems(3);

        result.Should().HaveCount(3);
        result.Should().OnlyContain(x => list.Contains(x));
    }

    [Fact]
    public void GetRandomItems_WithCountGreaterThanLength_ShouldReturnAllItems()
    {
        List<int> list = Collections.IntList;
        int[] result = list.GetRandomItems(10);

        result.Should().HaveCount(list.Count);
        result.Should().BeEquivalentTo(list);
    }

    [Fact]
    public void GetRandomItems_WithArray_ShouldReturnCorrectCount()
    {
        int[] array = Collections.IntArray;
        int[] result = array.GetRandomItems(3);

        result.Should().HaveCount(3);
        result.Should().OnlyContain(x => array.Contains(x));
    }

    [Fact]
    public void GetRandomItems_WithSpan_ShouldReturnCorrectCount()
    {
        int[] array = { 1, 2, 3, 4, 5 };
        Span<int> span = array;
        int[] result = span.GetRandomItems(3);

        result.Should().HaveCount(3);
        result.Should().OnlyContain(x => array.Contains(x));
    }

    [Fact]
    public void GetRandomItems_WithZeroCount_ShouldReturnEmpty()
    {
        List<int> list = Collections.IntList;
        int[] result = list.GetRandomItems(0);

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetRandomItems_Distribution_ShouldBeApproximatelyEven()
    {
        List<int> list = Collections.IntList;
        var results = new List<int>();

        for (int i = 0; i < 1000; i++)
        {
            results.AddRange(list.GetRandomItems(1));
        }

        var groups = results.GroupBy(x => x).ToList();
        groups.Should().HaveCount(list.Count);

        double expectedCount = 1000.0 / list.Count;
        foreach (IGrouping<int, int> group in groups)
        {
            double ratio = group.Count() / expectedCount;
            ratio.Should().BeApproximately(1.0, 0.3, "Item {0} should be approximately evenly distributed", group.Key);
        }
    }

    [Fact]
    public void GetRandomItems_WithEmptyCollection_ShouldThrow()
    {
        List<int> list = Collections.EmptyIntList;

        // RandomNumberGenerator.GetInt32 throws when given invalid range
        Assert.ThrowsAny<ArgumentException>(() => list.GetRandomItem());
    }
}

