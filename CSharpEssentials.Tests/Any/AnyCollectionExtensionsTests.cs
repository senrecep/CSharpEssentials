using CSharpEssentials.Any;
using FluentAssertions;

namespace CSharpEssentials.Tests.Any;

public class AnyCollectionExtensionsTests
{
    [Fact]
    public void Partition_T2_ShouldSplitBranches()
    {
        Any<int, string>[] source = [1, "two", 3, "four"];

        (int[] first, string[] second) = source.Partition();

        first.Should().Equal(1, 3);
        second.Should().Equal("two", "four");
    }

    [Fact]
    public void Sequence_T3_ShouldSplitAllBranches()
    {
        Any<int, string, bool>[] source = [1, "two", true, 3, false];

        (int[] first, string[] second, bool[] third) = source.Sequence();

        first.Should().Equal(1, 3);
        second.Should().Equal("two");
        third.Should().Equal(true, false);
    }

    [Fact]
    public void Traverse_T8_ShouldProjectAndPartition()
    {
        int[] source = [0, 1, 2, 3, 4, 5, 6, 7];

        var result = source.Traverse<int, int, string, bool, double, decimal, long, Guid, DateTime>(value => value switch
        {
            0 => value,
            1 => $"item-{value}",
            2 => true,
            3 => 3.5d,
            4 => 4.5m,
            5 => 5L,
            6 => Guid.Parse("11111111-1111-1111-1111-111111111111"),
            _ => new DateTime(2026, 5, 27, 0, 0, 0, DateTimeKind.Utc)
        });

        result.First.Should().Equal(0);
        result.Second.Should().Equal("item-1");
        result.Third.Should().Equal(true);
        result.Fourth.Should().Equal(3.5d);
        result.Fifth.Should().Equal(4.5m);
        result.Sixth.Should().Equal(5L);
        result.Seventh.Should().Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        result.Eighth.Should().Equal(new DateTime(2026, 5, 27, 0, 0, 0, DateTimeKind.Utc));
    }
}
