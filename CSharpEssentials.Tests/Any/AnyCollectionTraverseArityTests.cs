using CSharpEssentials.Any;
using FluentAssertions;

namespace CSharpEssentials.Tests.Any;

public class AnyCollectionTraverseArityTests
{
    private static readonly Guid SampleGuid = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public void Traverse_T5_ShouldProjectAndPartition()
    {
        int[] source = [0, 1, 2, 3, 4, 4];

        var result = source.Traverse<int, int, string, bool, double, decimal>(value => value switch
        {
            0 => value,
            1 => "one",
            2 => true,
            3 => 3.5d,
            _ => 4.5m
        });

        result.First.Should().Equal(0);
        result.Second.Should().Equal("one");
        result.Third.Should().Equal(true);
        result.Fourth.Should().Equal(3.5d);
        result.Fifth.Should().Equal(4.5m, 4.5m);
    }

    [Fact]
    public void Traverse_T6_ShouldProjectAndPartition()
    {
        int[] source = [0, 1, 2, 3, 4, 5];

        var result = source.Traverse<int, int, string, bool, double, decimal, long>(value => value switch
        {
            0 => value,
            1 => "one",
            2 => true,
            3 => 3.5d,
            4 => 4.5m,
            _ => 5L
        });

        result.First.Should().Equal(0);
        result.Second.Should().Equal("one");
        result.Third.Should().Equal(true);
        result.Fourth.Should().Equal(3.5d);
        result.Fifth.Should().Equal(4.5m);
        result.Sixth.Should().Equal(5L);
    }

    [Fact]
    public void Traverse_T7_ShouldProjectAndPartition()
    {
        int[] source = [0, 1, 2, 3, 4, 5, 6];

        var result = source.Traverse<int, int, string, bool, double, decimal, long, Guid>(value => value switch
        {
            0 => value,
            1 => "one",
            2 => true,
            3 => 3.5d,
            4 => 4.5m,
            5 => 5L,
            _ => SampleGuid
        });

        result.First.Should().Equal(0);
        result.Second.Should().Equal("one");
        result.Third.Should().Equal(true);
        result.Fourth.Should().Equal(3.5d);
        result.Fifth.Should().Equal(4.5m);
        result.Sixth.Should().Equal(5L);
        result.Seventh.Should().Equal(SampleGuid);
    }
}
