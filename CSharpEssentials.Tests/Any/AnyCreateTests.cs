using System.Text.Json;
using CSharpEssentials.Any;
using FluentAssertions;
using AnyFactory = CSharpEssentials.Any.Any;

namespace CSharpEssentials.Tests.Any;

public class AnyCreateTests
{
    private static readonly Guid SampleGuid = Guid.Parse("8f1c2a8e-3b4d-4e5f-9a6b-7c8d9e0f1a2b");

    [Fact]
    public void Create_AnyT3_EachPosition_ShouldSetIndexAndValue()
    {
        AnyFactory.Create<int, string, bool>(1).Should().Match<Any<int, string, bool>>(a => a.Index == 0 && (int)a.Value! == 1);
        AnyFactory.Create<int, string, bool>("a").Should().Match<Any<int, string, bool>>(a => a.Index == 1 && (string)a.Value! == "a");
        AnyFactory.Create<int, string, bool>(true).Should().Match<Any<int, string, bool>>(a => a.Index == 2 && (bool)a.Value!);
    }

    [Fact]
    public void Create_AnyT4_EachPosition_ShouldSetIndexAndValue()
    {
        AnyFactory.Create<int, string, bool, double>(1).Index.Should().Be(0);
        AnyFactory.Create<int, string, bool, double>("a").Index.Should().Be(1);
        AnyFactory.Create<int, string, bool, double>(true).Index.Should().Be(2);
        Any<int, string, bool, double> fourth = AnyFactory.Create<int, string, bool, double>(2.5);

        fourth.Index.Should().Be(3);
        fourth.GetFourth().Should().Be(2.5);
    }

    [Fact]
    public void Create_AnyT5_EachPosition_ShouldSetIndexAndValue()
    {
        AnyFactory.Create<int, string, bool, double, long>(1).Index.Should().Be(0);
        AnyFactory.Create<int, string, bool, double, long>("a").Index.Should().Be(1);
        AnyFactory.Create<int, string, bool, double, long>(true).Index.Should().Be(2);
        AnyFactory.Create<int, string, bool, double, long>(2.5).Index.Should().Be(3);
        Any<int, string, bool, double, long> fifth = AnyFactory.Create<int, string, bool, double, long>(7L);

        fifth.Index.Should().Be(4);
        fifth.GetFifth().Should().Be(7L);
    }

    [Fact]
    public void Create_AnyT6_EachPosition_ShouldSetIndexAndValue()
    {
        AnyFactory.Create<int, string, bool, double, long, decimal>(1).Index.Should().Be(0);
        AnyFactory.Create<int, string, bool, double, long, decimal>("a").Index.Should().Be(1);
        AnyFactory.Create<int, string, bool, double, long, decimal>(true).Index.Should().Be(2);
        AnyFactory.Create<int, string, bool, double, long, decimal>(2.5).Index.Should().Be(3);
        AnyFactory.Create<int, string, bool, double, long, decimal>(7L).Index.Should().Be(4);
        Any<int, string, bool, double, long, decimal> sixth = AnyFactory.Create<int, string, bool, double, long, decimal>(9.9m);

        sixth.Index.Should().Be(5);
        sixth.GetSixth().Should().Be(9.9m);
    }

    [Fact]
    public void Create_AnyT7_EachPosition_ShouldSetIndexAndValue()
    {
        AnyFactory.Create<int, string, bool, double, long, decimal, Guid>(1).Index.Should().Be(0);
        AnyFactory.Create<int, string, bool, double, long, decimal, Guid>("a").Index.Should().Be(1);
        AnyFactory.Create<int, string, bool, double, long, decimal, Guid>(true).Index.Should().Be(2);
        AnyFactory.Create<int, string, bool, double, long, decimal, Guid>(2.5).Index.Should().Be(3);
        AnyFactory.Create<int, string, bool, double, long, decimal, Guid>(7L).Index.Should().Be(4);
        AnyFactory.Create<int, string, bool, double, long, decimal, Guid>(9.9m).Index.Should().Be(5);
        Any<int, string, bool, double, long, decimal, Guid> seventh = AnyFactory.Create<int, string, bool, double, long, decimal, Guid>(SampleGuid);

        seventh.Index.Should().Be(6);
        seventh.GetSeventh().Should().Be(SampleGuid);
    }

    [Fact]
    public void Create_AnyT8_EachPosition_ShouldSetIndexAndValue()
    {
        AnyFactory.Create<int, string, bool, double, long, decimal, Guid, char>(1).Index.Should().Be(0);
        AnyFactory.Create<int, string, bool, double, long, decimal, Guid, char>("a").Index.Should().Be(1);
        AnyFactory.Create<int, string, bool, double, long, decimal, Guid, char>(true).Index.Should().Be(2);
        AnyFactory.Create<int, string, bool, double, long, decimal, Guid, char>(2.5).Index.Should().Be(3);
        AnyFactory.Create<int, string, bool, double, long, decimal, Guid, char>(7L).Index.Should().Be(4);
        AnyFactory.Create<int, string, bool, double, long, decimal, Guid, char>(9.9m).Index.Should().Be(5);
        AnyFactory.Create<int, string, bool, double, long, decimal, Guid, char>(SampleGuid).Index.Should().Be(6);
        Any<int, string, bool, double, long, decimal, Guid, char> eighth = AnyFactory.Create<int, string, bool, double, long, decimal, Guid, char>('z');

        eighth.Index.Should().Be(7);
        eighth.GetEighth().Should().Be('z');
    }

    [Fact]
    public void Constructor_WithMatchingInstance_ShouldKeepValue()
    {
        var any = new Any<int, string>(1, "hello");

        any.IsSecond.Should().BeTrue();
        any.GetSecond().Should().Be("hello");
    }

    [Fact]
    public void Constructor_WithJsonElement_ShouldConvertToTargetType()
    {
        JsonElement element = JsonDocument.Parse("42").RootElement;

        var any = new Any<int, string>(0, element);

        any.IsFirst.Should().BeTrue();
        any.GetFirst().Should().Be(42);
    }

    [Fact]
    public void Constructor_WithNullValue_ShouldThrowArgumentNullException()
    {
        Action act = () => _ = new Any<int, string>(0, null);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithOutOfRangeIndex_ShouldThrowInvalidOperationException()
    {
        Action act = () => _ = new Any<int, string>(2, 1);

        act.Should().Throw<InvalidOperationException>().WithMessage("2 is not valid index*");
    }

    [Fact]
    public void Constructor_WithValueOfWrongType_ShouldThrowInvalidOperationException()
    {
        Action act = () => _ = new Any<int, string>(0, "not an int");

        act.Should().Throw<InvalidOperationException>().WithMessage("Value is not valid type*");
    }
}
