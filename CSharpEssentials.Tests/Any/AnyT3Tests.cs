using System.Text.Json;
using CSharpEssentials.Any;
using FluentAssertions;

namespace CSharpEssentials.Tests.Any;

public class AnyT3Tests
{
    [Fact]
    public void ImplicitConversion_First_ShouldSetCorrectIndexAndProperties()
    {
        Any<int, string, bool> any = 42;
        any.Index.Should().Be(0);
        any.IsFirst.Should().BeTrue();
        any.IsSecond.Should().BeFalse();
        any.IsThird.Should().BeFalse();
        any.Value.Should().Be(42);
    }

    [Fact]
    public void ImplicitConversion_Second_ShouldSetCorrectIndexAndProperties()
    {
        Any<int, string, bool> any = "hello";
        any.Index.Should().Be(1);
        any.IsFirst.Should().BeFalse();
        any.IsSecond.Should().BeTrue();
        any.IsThird.Should().BeFalse();
        any.Value.Should().Be("hello");
    }

    [Fact]
    public void ImplicitConversion_Third_ShouldSetCorrectIndexAndProperties()
    {
        Any<int, string, bool> any = true;
        any.Index.Should().Be(2);
        any.IsFirst.Should().BeFalse();
        any.IsSecond.Should().BeFalse();
        any.IsThird.Should().BeTrue();
        any.Value.Should().Be(true);
    }

    [Fact]
    public void GetFirst_WhenFirst_ShouldReturnValue()
    {
        Any<int, string, bool> any = 42;
        any.GetFirst().Should().Be(42);
    }

    [Fact]
    public void GetFirst_WhenNotFirst_ShouldThrowInvalidOperationException()
    {
        Any<int, string, bool> any = "hello";
        any.Invoking(a => a.GetFirst()).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetSecond_WhenSecond_ShouldReturnValue()
    {
        Any<int, string, bool> any = "hello";
        any.GetSecond().Should().Be("hello");
    }

    [Fact]
    public void GetSecond_WhenNotSecond_ShouldThrowInvalidOperationException()
    {
        Any<int, string, bool> any = 42;
        any.Invoking(a => a.GetSecond()).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetThird_WhenThird_ShouldReturnValue()
    {
        Any<int, string, bool> any = true;
        any.GetThird().Should().BeTrue();
    }

    [Fact]
    public void GetThird_WhenNotThird_ShouldThrowInvalidOperationException()
    {
        Any<int, string, bool> any = 42;
        any.Invoking(a => a.GetThird()).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Switch_WhenFirst_ShouldExecuteFirstAction()
    {
        Any<int, string, bool> any = 42;
        bool executed = false;
        AnyActionStatus status = any.Switch(first: _ => executed = true);
        executed.Should().BeTrue();
        status.Should().Be(AnyActionStatus.Executed);
    }

    [Fact]
    public void Switch_WhenSecond_ShouldExecuteSecondAction()
    {
        Any<int, string, bool> any = "hello";
        bool executed = false;
        AnyActionStatus status = any.Switch(second: _ => executed = true);
        executed.Should().BeTrue();
        status.Should().Be(AnyActionStatus.Executed);
    }

    [Fact]
    public void Switch_WhenThird_ShouldExecuteThirdAction()
    {
        Any<int, string, bool> any = true;
        bool executed = false;
        AnyActionStatus status = any.Switch(third: _ => executed = true);
        executed.Should().BeTrue();
        status.Should().Be(AnyActionStatus.Executed);
    }

    [Fact]
    public void Switch_WithNoMatchingAction_ShouldReturnNotExecuted()
    {
        Any<int, string, bool> any = 42;
        AnyActionStatus status = any.Switch(second: _ => { });
        status.Should().Be(AnyActionStatus.NotExecuted);
    }

    [Fact]
    public void Switch_WithNullValue_ShouldThrowInvalidOperationException()
    {
        Any<int, string, bool> any = (string)null!;
        any.Invoking(a => a.Switch()).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Match_WhenFirst_ShouldReturnFirstResult()
    {
        Any<int, string, bool> any = 42;
        AnyActionResult<int> result = any.Match(first: x => x * 2, second: _ => 0, third: _ => 0);
        result.Status.Should().Be(AnyActionStatus.Executed);
        result.Result.Should().Be(84);
    }

    [Fact]
    public void Match_WhenSecond_ShouldReturnSecondResult()
    {
        Any<int, string, bool> any = "hello";
        AnyActionResult<int> result = any.Match(first: _ => 0, second: x => x.Length, third: _ => 0);
        result.Status.Should().Be(AnyActionStatus.Executed);
        result.Result.Should().Be(5);
    }

    [Fact]
    public void Match_WhenThird_ShouldReturnThirdResult()
    {
        Any<int, string, bool> any = true;
        AnyActionResult<int> result = any.Match(first: _ => 0, second: _ => 0, third: x => x ? 1 : 0);
        result.Status.Should().Be(AnyActionStatus.Executed);
        result.Result.Should().Be(1);
    }

    [Fact]
    public void Match_WithNoMatchingFunction_ShouldReturnNotExecuted()
    {
        Any<int, string, bool> any = 42;
        AnyActionResult<int> result = any.Match(second: _ => 0);
        result.Status.Should().Be(AnyActionStatus.NotExecuted);
    }

    [Fact]
    public void Match_WithNullValue_ShouldThrowInvalidOperationException()
    {
        Any<int, string, bool> any = (string)null!;
        any.Invoking(a => a.Match<int>()).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ToString_ShouldNotBeNullOrEmpty()
    {
        Any<int, string, bool> any = 42;
        any.ToString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void JsonSerialization_First_ShouldRoundTrip()
    {
        Any<int, string, bool> original = 42;
        string json = JsonSerializer.Serialize(original);
        Any<int, string, bool> deserialized = JsonSerializer.Deserialize<Any<int, string, bool>>(json);
        deserialized.Index.Should().Be(original.Index);
        deserialized.GetFirst().Should().Be(42);
    }

    [Fact]
    public void JsonSerialization_Second_ShouldRoundTrip()
    {
        Any<int, string, bool> original = "hello";
        string json = JsonSerializer.Serialize(original);
        Any<int, string, bool> deserialized = JsonSerializer.Deserialize<Any<int, string, bool>>(json);
        deserialized.Index.Should().Be(original.Index);
        deserialized.GetSecond().Should().Be("hello");
    }

    [Fact]
    public void JsonSerialization_Third_ShouldRoundTrip()
    {
        Any<int, string, bool> original = true;
        string json = JsonSerializer.Serialize(original);
        Any<int, string, bool> deserialized = JsonSerializer.Deserialize<Any<int, string, bool>>(json);
        deserialized.Index.Should().Be(original.Index);
        deserialized.GetThird().Should().BeTrue();
    }

    [Fact]
    public void Factory_First_ShouldCreateWithFirstType()
    {
        var any = Any<int, string, bool>.First(42);
        any.Index.Should().Be(0);
        any.GetFirst().Should().Be(42);
    }

    [Fact]
    public void Factory_Second_ShouldCreateWithSecondType()
    {
        var any = Any<int, string, bool>.Second("hello");
        any.Index.Should().Be(1);
        any.GetSecond().Should().Be("hello");
    }

    [Fact]
    public void Factory_Third_ShouldCreateWithThirdType()
    {
        var any = Any<int, string, bool>.Third(true);
        any.Index.Should().Be(2);
        any.GetThird().Should().BeTrue();
    }
}
