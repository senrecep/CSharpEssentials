using CSharpEssentials.Any;
using FluentAssertions;
using System.Globalization;

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
    public void Partition_T3_ShouldSplitAllBranches()
    {
        Any<int, string, bool>[] source = [1, "two", true, 3, false];

        (int[] first, string[] second, bool[] third) = source.Partition();

        first.Should().Equal(1, 3);
        second.Should().Equal("two");
        third.Should().Equal(true, false);
    }

    [Fact]
    public void Partition_T4_ShouldSplitAllBranches()
    {
        Any<int, string, bool, decimal>[] source = [1, "two", true, 4.5m];

        var result = source.Partition();

        result.First.Should().Equal(1);
        result.Second.Should().Equal("two");
        result.Third.Should().Equal(true);
        result.Fourth.Should().Equal(4.5m);
    }

    [Fact]
    public void Partition_T5_ShouldSplitAllBranches()
    {
        Any<int, string, bool, decimal, Guid>[] source =
        [
            1,
            "two",
            true,
            4.5m,
            Guid.Parse("11111111-1111-1111-1111-111111111111")
        ];

        var result = source.Partition();

        result.First.Should().Equal(1);
        result.Second.Should().Equal("two");
        result.Third.Should().Equal(true);
        result.Fourth.Should().Equal(4.5m);
        result.Fifth.Should().Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"));
    }

    [Fact]
    public void Partition_T6_ShouldSplitAllBranches()
    {
        Any<int, string, bool, decimal, Guid, long>[] source =
        [
            1,
            "two",
            true,
            4.5m,
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            6L
        ];

        var result = source.Partition();

        result.First.Should().Equal(1);
        result.Second.Should().Equal("two");
        result.Third.Should().Equal(true);
        result.Fourth.Should().Equal(4.5m);
        result.Fifth.Should().Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        result.Sixth.Should().Equal(6L);
    }

    [Fact]
    public void Partition_T7_ShouldSplitAllBranches()
    {
        Any<int, string, bool, decimal, Guid, long, DateTime>[] source =
        [
            1,
            "two",
            true,
            4.5m,
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            6L,
            new DateTime(2026, 5, 27, 0, 0, 0, DateTimeKind.Utc)
        ];

        var result = source.Partition();

        result.First.Should().Equal(1);
        result.Second.Should().Equal("two");
        result.Third.Should().Equal(true);
        result.Fourth.Should().Equal(4.5m);
        result.Fifth.Should().Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        result.Sixth.Should().Equal(6L);
        result.Seventh.Should().Equal(new DateTime(2026, 5, 27, 0, 0, 0, DateTimeKind.Utc));
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

    [Fact]
    public void Partition_WithEmptySource_ShouldReturnEmptyArrays()
    {
        var result = Array.Empty<Any<int, string>>().Partition();

        result.First.Should().BeEmpty();
        result.Second.Should().BeEmpty();
    }

    [Fact]
    public void CollectionExtensions_WithNullSource_ShouldThrowArgumentNullException()
    {
        IEnumerable<Any<int, string>> any2 = null!;
        IEnumerable<Any<int, string, bool, decimal, Guid, long, DateTime, TimeSpan>> any8 = null!;
        IEnumerable<int> values = null!;

        Action[] actions =
        [
            () => any2.Partition(),
            () => any8.Partition(),
            () => values.Traverse<int, int, string>(value => value),
            () => values.Traverse<int, int, string, bool, decimal, Guid, long, DateTime, TimeSpan>(value => value switch
            {
                0 => value,
                1 => value.ToString(CultureInfo.InvariantCulture),
                2 => true,
                3 => 3.5m,
                4 => Guid.Empty,
                5 => 5L,
                6 => DateTime.UnixEpoch,
                _ => TimeSpan.Zero
            })
        ];

        foreach (Action action in actions)
            action.Should().Throw<ArgumentNullException>().WithParameterName("source");
    }

    [Fact]
    public void Traverse_WithNullSelector_ShouldThrowArgumentNullException()
    {
        int[] values = [1];
        Func<int, Any<int, string>> selector = null!;

        Action action = () => values.Traverse<int, int, string>(selector);

        action.Should().Throw<ArgumentNullException>().WithParameterName("selector");
    }
}
