using CSharpEssentials.Any;
using FluentAssertions;
using System.Text.Json;

namespace CSharpEssentials.Tests.Any;

public class AnyT5Tests
{
    [Fact]
    public void ImplicitConversion_First_ShouldSetCorrectIndexAndProperties()
    {
        Any<int, string, bool, double, long> any = 42;
        any.Index.Should().Be(0);
        any.IsFirst.Should().BeTrue();
        any.IsSecond.Should().BeFalse();
        any.IsThird.Should().BeFalse();
        any.IsFourth.Should().BeFalse();
        any.IsFifth.Should().BeFalse();
        any.Value.Should().Be(42);
    }

    [Fact]
    public void ImplicitConversion_Second_ShouldSetCorrectIndexAndProperties()
    {
        Any<int, string, bool, double, long> any = "hello";
        any.Index.Should().Be(1);
        any.IsFirst.Should().BeFalse();
        any.IsSecond.Should().BeTrue();
        any.IsThird.Should().BeFalse();
        any.IsFourth.Should().BeFalse();
        any.IsFifth.Should().BeFalse();
        any.Value.Should().Be("hello");
    }

    [Fact]
    public void ImplicitConversion_Third_ShouldSetCorrectIndexAndProperties()
    {
        Any<int, string, bool, double, long> any = true;
        any.Index.Should().Be(2);
        any.IsFirst.Should().BeFalse();
        any.IsSecond.Should().BeFalse();
        any.IsThird.Should().BeTrue();
        any.IsFourth.Should().BeFalse();
        any.IsFifth.Should().BeFalse();
        any.Value.Should().Be(true);
    }

    [Fact]
    public void ImplicitConversion_Fourth_ShouldSetCorrectIndexAndProperties()
    {
        Any<int, string, bool, double, long> any = 3.14;
        any.Index.Should().Be(3);
        any.IsFirst.Should().BeFalse();
        any.IsSecond.Should().BeFalse();
        any.IsThird.Should().BeFalse();
        any.IsFourth.Should().BeTrue();
        any.IsFifth.Should().BeFalse();
        any.Value.Should().Be(3.14);
    }

    [Fact]
    public void ImplicitConversion_Fifth_ShouldSetCorrectIndexAndProperties()
    {
        Any<int, string, bool, double, long> any = 99L;
        any.Index.Should().Be(4);
        any.IsFirst.Should().BeFalse();
        any.IsSecond.Should().BeFalse();
        any.IsThird.Should().BeFalse();
        any.IsFourth.Should().BeFalse();
        any.IsFifth.Should().BeTrue();
        any.Value.Should().Be(99L);
    }

    [Fact]
    public void GetFirst_WhenFirst_ShouldReturnValue()
    {
        Any<int, string, bool, double, long> any = 42;
        any.GetFirst().Should().Be(42);
    }

    [Fact]
    public void GetFirst_WhenNotFirst_ShouldThrowInvalidOperationException()
    {
        Any<int, string, bool, double, long> any = "hello";
        any.Invoking(a => a.GetFirst()).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetSecond_WhenSecond_ShouldReturnValue()
    {
        Any<int, string, bool, double, long> any = "hello";
        any.GetSecond().Should().Be("hello");
    }

    [Fact]
    public void GetSecond_WhenNotSecond_ShouldThrowInvalidOperationException()
    {
        Any<int, string, bool, double, long> any = 42;
        any.Invoking(a => a.GetSecond()).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetThird_WhenThird_ShouldReturnValue()
    {
        Any<int, string, bool, double, long> any = true;
        any.GetThird().Should().BeTrue();
    }

    [Fact]
    public void GetThird_WhenNotThird_ShouldThrowInvalidOperationException()
    {
        Any<int, string, bool, double, long> any = 42;
        any.Invoking(a => a.GetThird()).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetFourth_WhenFourth_ShouldReturnValue()
    {
        Any<int, string, bool, double, long> any = 3.14;
        any.GetFourth().Should().Be(3.14);
    }

    [Fact]
    public void GetFourth_WhenNotFourth_ShouldThrowInvalidOperationException()
    {
        Any<int, string, bool, double, long> any = 42;
        any.Invoking(a => a.GetFourth()).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetFifth_WhenFifth_ShouldReturnValue()
    {
        Any<int, string, bool, double, long> any = 99L;
        any.GetFifth().Should().Be(99L);
    }

    [Fact]
    public void GetFifth_WhenNotFifth_ShouldThrowInvalidOperationException()
    {
        Any<int, string, bool, double, long> any = 42;
        any.Invoking(a => a.GetFifth()).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Switch_WhenFirst_ShouldExecuteFirstAction()
    {
        Any<int, string, bool, double, long> any = 42;
        bool executed = false;
        var status = any.Switch(first: _ => executed = true);
        executed.Should().BeTrue();
        status.Should().Be(AnyActionStatus.Executed);
    }

    [Fact]
    public void Switch_WhenSecond_ShouldExecuteSecondAction()
    {
        Any<int, string, bool, double, long> any = "hello";
        bool executed = false;
        var status = any.Switch(second: _ => executed = true);
        executed.Should().BeTrue();
        status.Should().Be(AnyActionStatus.Executed);
    }

    [Fact]
    public void Switch_WhenThird_ShouldExecuteThirdAction()
    {
        Any<int, string, bool, double, long> any = true;
        bool executed = false;
        var status = any.Switch(third: _ => executed = true);
        executed.Should().BeTrue();
        status.Should().Be(AnyActionStatus.Executed);
    }

    [Fact]
    public void Switch_WhenFourth_ShouldExecuteFourthAction()
    {
        Any<int, string, bool, double, long> any = 3.14;
        bool executed = false;
        var status = any.Switch(fourth: _ => executed = true);
        executed.Should().BeTrue();
        status.Should().Be(AnyActionStatus.Executed);
    }

    [Fact]
    public void Switch_WhenFifth_ShouldExecuteFifthAction()
    {
        Any<int, string, bool, double, long> any = 99L;
        bool executed = false;
        var status = any.Switch(fifth: _ => executed = true);
        executed.Should().BeTrue();
        status.Should().Be(AnyActionStatus.Executed);
    }

    [Fact]
    public void Switch_WithNoMatchingAction_ShouldReturnNotExecuted()
    {
        Any<int, string, bool, double, long> any = 42;
        var status = any.Switch(second: _ => { });
        status.Should().Be(AnyActionStatus.NotExecuted);
    }

    [Fact]
    public void Switch_WithNullValue_ShouldThrowInvalidOperationException()
    {
        Any<int, string, bool, double, long> any = (string)null!;
        any.Invoking(a => a.Switch()).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Match_WhenFirst_ShouldReturnFirstResult()
    {
        Any<int, string, bool, double, long> any = 42;
        var result = any.Match(first: x => x * 2, second: _ => 0, third: _ => 0, fourth: _ => 0, fifth: _ => 0);
        result.Status.Should().Be(AnyActionStatus.Executed);
        result.Result.Should().Be(84);
    }

    [Fact]
    public void Match_WhenSecond_ShouldReturnSecondResult()
    {
        Any<int, string, bool, double, long> any = "hello";
        var result = any.Match(first: _ => 0, second: x => x.Length, third: _ => 0, fourth: _ => 0, fifth: _ => 0);
        result.Status.Should().Be(AnyActionStatus.Executed);
        result.Result.Should().Be(5);
    }

    [Fact]
    public void Match_WhenThird_ShouldReturnThirdResult()
    {
        Any<int, string, bool, double, long> any = true;
        var result = any.Match(first: _ => 0, second: _ => 0, third: x => x ? 1 : 0, fourth: _ => 0, fifth: _ => 0);
        result.Status.Should().Be(AnyActionStatus.Executed);
        result.Result.Should().Be(1);
    }

    [Fact]
    public void Match_WhenFourth_ShouldReturnFourthResult()
    {
        Any<int, string, bool, double, long> any = 3.14;
        var result = any.Match(first: _ => 0, second: _ => 0, third: _ => 0, fourth: x => (int)x, fifth: _ => 0);
        result.Status.Should().Be(AnyActionStatus.Executed);
        result.Result.Should().Be(3);
    }

    [Fact]
    public void Match_WhenFifth_ShouldReturnFifthResult()
    {
        Any<int, string, bool, double, long> any = 99L;
        var result = any.Match(first: _ => 0, second: _ => 0, third: _ => 0, fourth: _ => 0, fifth: x => (int)x);
        result.Status.Should().Be(AnyActionStatus.Executed);
        result.Result.Should().Be(99);
    }

    [Fact]
    public void Match_WithNoMatchingFunction_ShouldReturnNotExecuted()
    {
        Any<int, string, bool, double, long> any = 42;
        var result = any.Match(second: _ => 0);
        result.Status.Should().Be(AnyActionStatus.NotExecuted);
    }

    [Fact]
    public void Match_WithNullValue_ShouldThrowInvalidOperationException()
    {
        Any<int, string, bool, double, long> any = (string)null!;
        any.Invoking(a => a.Match<int>()).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ToString_ShouldNotBeNullOrEmpty()
    {
        Any<int, string, bool, double, long> any = 42;
        any.ToString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void JsonSerialization_First_ShouldRoundTrip()
    {
        Any<int, string, bool, double, long> original = 42;
        string json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<Any<int, string, bool, double, long>>(json);
        deserialized.Index.Should().Be(original.Index);
        deserialized.GetFirst().Should().Be(42);
    }

    [Fact]
    public void JsonSerialization_Second_ShouldRoundTrip()
    {
        Any<int, string, bool, double, long> original = "hello";
        string json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<Any<int, string, bool, double, long>>(json);
        deserialized.Index.Should().Be(original.Index);
        deserialized.GetSecond().Should().Be("hello");
    }

    [Fact]
    public void JsonSerialization_Third_ShouldRoundTrip()
    {
        Any<int, string, bool, double, long> original = true;
        string json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<Any<int, string, bool, double, long>>(json);
        deserialized.Index.Should().Be(original.Index);
        deserialized.GetThird().Should().BeTrue();
    }

    [Fact]
    public void JsonSerialization_Fourth_ShouldRoundTrip()
    {
        Any<int, string, bool, double, long> original = 3.14;
        string json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<Any<int, string, bool, double, long>>(json);
        deserialized.Index.Should().Be(original.Index);
        deserialized.GetFourth().Should().Be(3.14);
    }

    [Fact]
    public void JsonSerialization_Fifth_ShouldRoundTrip()
    {
        Any<int, string, bool, double, long> original = 99L;
        string json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<Any<int, string, bool, double, long>>(json);
        deserialized.Index.Should().Be(original.Index);
        deserialized.GetFifth().Should().Be(99L);
    }

    [Fact]
    public void Factory_First_ShouldCreateWithFirstType()
    {
        var any = Any<int, string, bool, double, long>.First(42);
        any.Index.Should().Be(0);
        any.GetFirst().Should().Be(42);
    }

    [Fact]
    public void Factory_Second_ShouldCreateWithSecondType()
    {
        var any = Any<int, string, bool, double, long>.Second("hello");
        any.Index.Should().Be(1);
        any.GetSecond().Should().Be("hello");
    }

    [Fact]
    public void Factory_Third_ShouldCreateWithThirdType()
    {
        var any = Any<int, string, bool, double, long>.Third(true);
        any.Index.Should().Be(2);
        any.GetThird().Should().BeTrue();
    }

    [Fact]
    public void Factory_Fourth_ShouldCreateWithFourthType()
    {
        var any = Any<int, string, bool, double, long>.Fourth(3.14);
        any.Index.Should().Be(3);
        any.GetFourth().Should().Be(3.14);
    }

    [Fact]
    public void Factory_Fifth_ShouldCreateWithFifthType()
    {
        var any = Any<int, string, bool, double, long>.Fifth(99L);
        any.Index.Should().Be(4);
        any.GetFifth().Should().Be(99L);
    }
}
