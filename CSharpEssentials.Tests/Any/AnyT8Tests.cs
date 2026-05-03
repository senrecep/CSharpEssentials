using CSharpEssentials.Any;
using FluentAssertions;
using System.Text.Json;

namespace CSharpEssentials.Tests.Any;

public class AnyT8Tests
{
    private static readonly Guid TestGuid = Guid.Parse("a1b2c3d4-e5f6-4789-8abc-def012345678");

    [Fact]
    public void ImplicitConversion_First_ShouldSetCorrectIndexAndProperties()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 42;
        any.Index.Should().Be(0);
        any.IsFirst.Should().BeTrue();
        any.IsSecond.Should().BeFalse();
        any.IsThird.Should().BeFalse();
        any.IsFourth.Should().BeFalse();
        any.IsFifth.Should().BeFalse();
        any.IsSixth.Should().BeFalse();
        any.IsSeventh.Should().BeFalse();
        any.IsEighth.Should().BeFalse();
        any.Value.Should().Be(42);
    }

    [Fact]
    public void ImplicitConversion_Second_ShouldSetCorrectIndexAndProperties()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = "hello";
        any.Index.Should().Be(1);
        any.IsFirst.Should().BeFalse();
        any.IsSecond.Should().BeTrue();
        any.IsThird.Should().BeFalse();
        any.IsFourth.Should().BeFalse();
        any.IsFifth.Should().BeFalse();
        any.IsSixth.Should().BeFalse();
        any.IsSeventh.Should().BeFalse();
        any.IsEighth.Should().BeFalse();
        any.Value.Should().Be("hello");
    }

    [Fact]
    public void ImplicitConversion_Third_ShouldSetCorrectIndexAndProperties()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = true;
        any.Index.Should().Be(2);
        any.IsFirst.Should().BeFalse();
        any.IsSecond.Should().BeFalse();
        any.IsThird.Should().BeTrue();
        any.IsFourth.Should().BeFalse();
        any.IsFifth.Should().BeFalse();
        any.IsSixth.Should().BeFalse();
        any.IsSeventh.Should().BeFalse();
        any.IsEighth.Should().BeFalse();
        any.Value.Should().Be(true);
    }

    [Fact]
    public void ImplicitConversion_Fourth_ShouldSetCorrectIndexAndProperties()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 3.14;
        any.Index.Should().Be(3);
        any.IsFirst.Should().BeFalse();
        any.IsSecond.Should().BeFalse();
        any.IsThird.Should().BeFalse();
        any.IsFourth.Should().BeTrue();
        any.IsFifth.Should().BeFalse();
        any.IsSixth.Should().BeFalse();
        any.IsSeventh.Should().BeFalse();
        any.IsEighth.Should().BeFalse();
        any.Value.Should().Be(3.14);
    }

    [Fact]
    public void ImplicitConversion_Fifth_ShouldSetCorrectIndexAndProperties()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 99L;
        any.Index.Should().Be(4);
        any.IsFirst.Should().BeFalse();
        any.IsSecond.Should().BeFalse();
        any.IsThird.Should().BeFalse();
        any.IsFourth.Should().BeFalse();
        any.IsFifth.Should().BeTrue();
        any.IsSixth.Should().BeFalse();
        any.IsSeventh.Should().BeFalse();
        any.IsEighth.Should().BeFalse();
        any.Value.Should().Be(99L);
    }

    [Fact]
    public void ImplicitConversion_Sixth_ShouldSetCorrectIndexAndProperties()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 3.99m;
        any.Index.Should().Be(5);
        any.IsFirst.Should().BeFalse();
        any.IsSecond.Should().BeFalse();
        any.IsThird.Should().BeFalse();
        any.IsFourth.Should().BeFalse();
        any.IsFifth.Should().BeFalse();
        any.IsSixth.Should().BeTrue();
        any.IsSeventh.Should().BeFalse();
        any.IsEighth.Should().BeFalse();
        any.Value.Should().Be(3.99m);
    }

    [Fact]
    public void ImplicitConversion_Seventh_ShouldSetCorrectIndexAndProperties()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = TestGuid;
        any.Index.Should().Be(6);
        any.IsFirst.Should().BeFalse();
        any.IsSecond.Should().BeFalse();
        any.IsThird.Should().BeFalse();
        any.IsFourth.Should().BeFalse();
        any.IsFifth.Should().BeFalse();
        any.IsSixth.Should().BeFalse();
        any.IsSeventh.Should().BeTrue();
        any.IsEighth.Should().BeFalse();
        any.Value.Should().Be(TestGuid);
    }

    [Fact]
    public void ImplicitConversion_Eighth_ShouldSetCorrectIndexAndProperties()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 'x';
        any.Index.Should().Be(7);
        any.IsFirst.Should().BeFalse();
        any.IsSecond.Should().BeFalse();
        any.IsThird.Should().BeFalse();
        any.IsFourth.Should().BeFalse();
        any.IsFifth.Should().BeFalse();
        any.IsSixth.Should().BeFalse();
        any.IsSeventh.Should().BeFalse();
        any.IsEighth.Should().BeTrue();
        any.Value.Should().Be('x');
    }

    [Fact]
    public void GetFirst_WhenFirst_ShouldReturnValue()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 42;
        any.GetFirst().Should().Be(42);
    }

    [Fact]
    public void GetFirst_WhenNotFirst_ShouldThrowInvalidOperationException()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = "hello";
        any.Invoking(a => a.GetFirst()).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetSecond_WhenSecond_ShouldReturnValue()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = "hello";
        any.GetSecond().Should().Be("hello");
    }

    [Fact]
    public void GetSecond_WhenNotSecond_ShouldThrowInvalidOperationException()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 42;
        any.Invoking(a => a.GetSecond()).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetThird_WhenThird_ShouldReturnValue()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = true;
        any.GetThird().Should().BeTrue();
    }

    [Fact]
    public void GetThird_WhenNotThird_ShouldThrowInvalidOperationException()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 42;
        any.Invoking(a => a.GetThird()).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetFourth_WhenFourth_ShouldReturnValue()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 3.14;
        any.GetFourth().Should().Be(3.14);
    }

    [Fact]
    public void GetFourth_WhenNotFourth_ShouldThrowInvalidOperationException()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 42;
        any.Invoking(a => a.GetFourth()).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetFifth_WhenFifth_ShouldReturnValue()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 99L;
        any.GetFifth().Should().Be(99L);
    }

    [Fact]
    public void GetFifth_WhenNotFifth_ShouldThrowInvalidOperationException()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 42;
        any.Invoking(a => a.GetFifth()).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetSixth_WhenSixth_ShouldReturnValue()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 3.99m;
        any.GetSixth().Should().Be(3.99m);
    }

    [Fact]
    public void GetSixth_WhenNotSixth_ShouldThrowInvalidOperationException()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 42;
        any.Invoking(a => a.GetSixth()).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetSeventh_WhenSeventh_ShouldReturnValue()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = TestGuid;
        any.GetSeventh().Should().Be(TestGuid);
    }

    [Fact]
    public void GetSeventh_WhenNotSeventh_ShouldThrowInvalidOperationException()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 42;
        any.Invoking(a => a.GetSeventh()).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetEighth_WhenEighth_ShouldReturnValue()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 'x';
        any.GetEighth().Should().Be('x');
    }

    [Fact]
    public void GetEighth_WhenNotEighth_ShouldThrowInvalidOperationException()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 42;
        any.Invoking(a => a.GetEighth()).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Switch_WhenFirst_ShouldExecuteFirstAction()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 42;
        bool executed = false;
        var status = any.Switch(first: _ => executed = true);
        executed.Should().BeTrue();
        status.Should().Be(AnyActionStatus.Executed);
    }

    [Fact]
    public void Switch_WhenSecond_ShouldExecuteSecondAction()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = "hello";
        bool executed = false;
        var status = any.Switch(second: _ => executed = true);
        executed.Should().BeTrue();
        status.Should().Be(AnyActionStatus.Executed);
    }

    [Fact]
    public void Switch_WhenThird_ShouldExecuteThirdAction()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = true;
        bool executed = false;
        var status = any.Switch(third: _ => executed = true);
        executed.Should().BeTrue();
        status.Should().Be(AnyActionStatus.Executed);
    }

    [Fact]
    public void Switch_WhenFourth_ShouldExecuteFourthAction()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 3.14;
        bool executed = false;
        var status = any.Switch(fourth: _ => executed = true);
        executed.Should().BeTrue();
        status.Should().Be(AnyActionStatus.Executed);
    }

    [Fact]
    public void Switch_WhenFifth_ShouldExecuteFifthAction()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 99L;
        bool executed = false;
        var status = any.Switch(fifth: _ => executed = true);
        executed.Should().BeTrue();
        status.Should().Be(AnyActionStatus.Executed);
    }

    [Fact]
    public void Switch_WhenSixth_ShouldExecuteSixthAction()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 3.99m;
        bool executed = false;
        var status = any.Switch(sixth: _ => executed = true);
        executed.Should().BeTrue();
        status.Should().Be(AnyActionStatus.Executed);
    }

    [Fact]
    public void Switch_WhenSeventh_ShouldExecuteSeventhAction()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = TestGuid;
        bool executed = false;
        var status = any.Switch(seventh: _ => executed = true);
        executed.Should().BeTrue();
        status.Should().Be(AnyActionStatus.Executed);
    }

    [Fact]
    public void Switch_WhenEighth_ShouldExecuteEighthAction()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 'x';
        bool executed = false;
        var status = any.Switch(eighth: _ => executed = true);
        executed.Should().BeTrue();
        status.Should().Be(AnyActionStatus.Executed);
    }

    [Fact]
    public void Switch_WithNoMatchingAction_ShouldReturnNotExecuted()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 42;
        var status = any.Switch(second: _ => { });
        status.Should().Be(AnyActionStatus.NotExecuted);
    }

    [Fact]
    public void Switch_WithNullValue_ShouldThrowInvalidOperationException()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = (string)null!;
        any.Invoking(a => a.Switch()).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Match_WhenFirst_ShouldReturnFirstResult()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 42;
        var result = any.Match(first: x => x * 2, second: _ => 0, third: _ => 0, fourth: _ => 0, fifth: _ => 0, sixth: _ => 0, seventh: _ => 0, eighth: _ => 0);
        result.Status.Should().Be(AnyActionStatus.Executed);
        result.Result.Should().Be(84);
    }

    [Fact]
    public void Match_WhenSecond_ShouldReturnSecondResult()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = "hello";
        var result = any.Match(first: _ => 0, second: x => x.Length, third: _ => 0, fourth: _ => 0, fifth: _ => 0, sixth: _ => 0, seventh: _ => 0, eighth: _ => 0);
        result.Status.Should().Be(AnyActionStatus.Executed);
        result.Result.Should().Be(5);
    }

    [Fact]
    public void Match_WhenThird_ShouldReturnThirdResult()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = true;
        var result = any.Match(first: _ => 0, second: _ => 0, third: x => x ? 1 : 0, fourth: _ => 0, fifth: _ => 0, sixth: _ => 0, seventh: _ => 0, eighth: _ => 0);
        result.Status.Should().Be(AnyActionStatus.Executed);
        result.Result.Should().Be(1);
    }

    [Fact]
    public void Match_WhenFourth_ShouldReturnFourthResult()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 3.14;
        var result = any.Match(first: _ => 0, second: _ => 0, third: _ => 0, fourth: x => (int)x, fifth: _ => 0, sixth: _ => 0, seventh: _ => 0, eighth: _ => 0);
        result.Status.Should().Be(AnyActionStatus.Executed);
        result.Result.Should().Be(3);
    }

    [Fact]
    public void Match_WhenFifth_ShouldReturnFifthResult()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 99L;
        var result = any.Match(first: _ => 0, second: _ => 0, third: _ => 0, fourth: _ => 0, fifth: x => (int)x, sixth: _ => 0, seventh: _ => 0, eighth: _ => 0);
        result.Status.Should().Be(AnyActionStatus.Executed);
        result.Result.Should().Be(99);
    }

    [Fact]
    public void Match_WhenSixth_ShouldReturnSixthResult()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 3.99m;
        var result = any.Match(first: _ => 0, second: _ => 0, third: _ => 0, fourth: _ => 0, fifth: _ => 0, sixth: x => (int)x, seventh: _ => 0, eighth: _ => 0);
        result.Status.Should().Be(AnyActionStatus.Executed);
        result.Result.Should().Be(3);
    }

    [Fact]
    public void Match_WhenSeventh_ShouldReturnSeventhResult()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = TestGuid;
        var result = any.Match(first: _ => 0, second: _ => 0, third: _ => 0, fourth: _ => 0, fifth: _ => 0, sixth: _ => 0, seventh: x => x.GetHashCode(), eighth: _ => 0);
        result.Status.Should().Be(AnyActionStatus.Executed);
        result.Result.Should().Be(TestGuid.GetHashCode());
    }

    [Fact]
    public void Match_WhenEighth_ShouldReturnEighthResult()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 'x';
        var result = any.Match(first: _ => 0, second: _ => 0, third: _ => 0, fourth: _ => 0, fifth: _ => 0, sixth: _ => 0, seventh: _ => 0, eighth: x => (int)x);
        result.Status.Should().Be(AnyActionStatus.Executed);
        result.Result.Should().Be(120);
    }

    [Fact]
    public void Match_WithNoMatchingFunction_ShouldReturnNotExecuted()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 42;
        var result = any.Match(second: _ => 0);
        result.Status.Should().Be(AnyActionStatus.NotExecuted);
    }

    [Fact]
    public void Match_WithNullValue_ShouldThrowInvalidOperationException()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = (string)null!;
        any.Invoking(a => a.Match<int>()).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ToString_ShouldNotBeNullOrEmpty()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 42;
        any.ToString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void JsonSerialization_First_ShouldRoundTrip()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> original = 42;
        string json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<Any<int, string, bool, double, long, decimal, Guid, char>>(json);
        deserialized.Index.Should().Be(original.Index);
        deserialized.GetFirst().Should().Be(42);
    }

    [Fact]
    public void JsonSerialization_Second_ShouldRoundTrip()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> original = "hello";
        string json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<Any<int, string, bool, double, long, decimal, Guid, char>>(json);
        deserialized.Index.Should().Be(original.Index);
        deserialized.GetSecond().Should().Be("hello");
    }

    [Fact]
    public void JsonSerialization_Third_ShouldRoundTrip()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> original = true;
        string json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<Any<int, string, bool, double, long, decimal, Guid, char>>(json);
        deserialized.Index.Should().Be(original.Index);
        deserialized.GetThird().Should().BeTrue();
    }

    [Fact]
    public void JsonSerialization_Fourth_ShouldRoundTrip()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> original = 3.14;
        string json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<Any<int, string, bool, double, long, decimal, Guid, char>>(json);
        deserialized.Index.Should().Be(original.Index);
        deserialized.GetFourth().Should().Be(3.14);
    }

    [Fact]
    public void JsonSerialization_Fifth_ShouldRoundTrip()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> original = 99L;
        string json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<Any<int, string, bool, double, long, decimal, Guid, char>>(json);
        deserialized.Index.Should().Be(original.Index);
        deserialized.GetFifth().Should().Be(99L);
    }

    [Fact]
    public void JsonSerialization_Sixth_ShouldRoundTrip()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> original = 3.99m;
        string json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<Any<int, string, bool, double, long, decimal, Guid, char>>(json);
        deserialized.Index.Should().Be(original.Index);
        deserialized.GetSixth().Should().Be(3.99m);
    }

    [Fact]
    public void JsonSerialization_Seventh_ShouldRoundTrip()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> original = TestGuid;
        string json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<Any<int, string, bool, double, long, decimal, Guid, char>>(json);
        deserialized.Index.Should().Be(original.Index);
        deserialized.GetSeventh().Should().Be(TestGuid);
    }

    [Fact]
    public void JsonSerialization_Eighth_ShouldRoundTrip()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> original = 'x';
        string json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<Any<int, string, bool, double, long, decimal, Guid, char>>(json);
        deserialized.Index.Should().Be(original.Index);
        deserialized.GetEighth().Should().Be('x');
    }

    [Fact]
    public void Factory_First_ShouldCreateWithFirstType()
    {
        var any = Any<int, string, bool, double, long, decimal, Guid, char>.First(42);
        any.Index.Should().Be(0);
        any.GetFirst().Should().Be(42);
    }

    [Fact]
    public void Factory_Second_ShouldCreateWithSecondType()
    {
        var any = Any<int, string, bool, double, long, decimal, Guid, char>.Second("hello");
        any.Index.Should().Be(1);
        any.GetSecond().Should().Be("hello");
    }

    [Fact]
    public void Factory_Third_ShouldCreateWithThirdType()
    {
        var any = Any<int, string, bool, double, long, decimal, Guid, char>.Third(true);
        any.Index.Should().Be(2);
        any.GetThird().Should().BeTrue();
    }

    [Fact]
    public void Factory_Fourth_ShouldCreateWithFourthType()
    {
        var any = Any<int, string, bool, double, long, decimal, Guid, char>.Fourth(3.14);
        any.Index.Should().Be(3);
        any.GetFourth().Should().Be(3.14);
    }

    [Fact]
    public void Factory_Fifth_ShouldCreateWithFifthType()
    {
        var any = Any<int, string, bool, double, long, decimal, Guid, char>.Fifth(99L);
        any.Index.Should().Be(4);
        any.GetFifth().Should().Be(99L);
    }

    [Fact]
    public void Factory_Sixth_ShouldCreateWithSixthType()
    {
        var any = Any<int, string, bool, double, long, decimal, Guid, char>.Sixth(3.99m);
        any.Index.Should().Be(5);
        any.GetSixth().Should().Be(3.99m);
    }

    [Fact]
    public void Factory_Seventh_ShouldCreateWithSeventhType()
    {
        var any = Any<int, string, bool, double, long, decimal, Guid, char>.Seventh(TestGuid);
        any.Index.Should().Be(6);
        any.GetSeventh().Should().Be(TestGuid);
    }

    [Fact]
    public void Factory_Eighth_ShouldCreateWithEighthType()
    {
        var any = Any<int, string, bool, double, long, decimal, Guid, char>.Eighth('x');
        any.Index.Should().Be(7);
        any.GetEighth().Should().Be('x');
    }
}
