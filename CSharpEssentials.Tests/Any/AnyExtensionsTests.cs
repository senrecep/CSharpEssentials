using CSharpEssentials.Any;
using FluentAssertions;

namespace CSharpEssentials.Tests.Any;

public class AnyExtensionsTests
{
    #region Any<T0, T1>

    [Fact]
    public void Deconstruct_AnyT2_First_ShouldReturnFirst()
    {
        Any<int, string> any = 42;
        (int first, string? second) = any;

        first.Should().Be(42);
        second.Should().BeNull();
    }

    [Fact]
    public void Deconstruct_AnyT2_Second_ShouldReturnSecond()
    {
        Any<int, string> any = "hello";
        (int first, string? second) = any;

        first.Should().Be(0);
        second.Should().Be("hello");
    }

    [Fact]
    public void ToTuple_AnyT2_First_ShouldReturnFirst()
    {
        Any<int, string> any = 42;
        (int First, string? Second) tuple = any.ToTuple();

        tuple.First.Should().Be(42);
        tuple.Second.Should().BeNull();
    }

    [Fact]
    public void ToTuple_AnyT2_Second_ShouldReturnSecond()
    {
        Any<int, string> any = "hello";
        (int First, string? Second) tuple = any.ToTuple();

        tuple.First.Should().Be(0);
        tuple.Second.Should().Be("hello");
    }

    [Fact]
    public void Is_AnyT2_First_WithMatchingType_ShouldReturnTrue()
    {
        Any<int, string> any = 42;

        bool result = any.Is<int, int, string>();

        result.Should().BeTrue();
    }

    [Fact]
    public void Is_AnyT2_First_WithNonMatchingType_ShouldReturnFalse()
    {
        Any<int, string> any = 42;

        bool result = any.Is<string, int, string>();

        result.Should().BeFalse();
    }

    [Fact]
    public void Is_AnyT2_Second_WithMatchingType_ShouldReturnTrue()
    {
        Any<int, string> any = "hello";

        bool result = any.Is<string, int, string>();

        result.Should().BeTrue();
    }

    [Fact]
    public void As_AnyT2_First_WithMatchingType_ShouldReturnValue()
    {
        Any<int, string> any = 42;

        int? result = any.As<int, int, string>();

        result.Should().Be(42);
    }

    [Fact]
    public void As_AnyT2_First_WithNonMatchingType_ShouldReturnDefault()
    {
        Any<int, string> any = 42;

        string? result = any.As<string, int, string>();

        result.Should().BeNull();
    }

    [Fact]
    public void As_AnyT2_Second_WithMatchingType_ShouldReturnValue()
    {
        Any<int, string> any = "hello";

        string? result = any.As<string, int, string>();

        result.Should().Be("hello");
    }

    [Fact]
    public void TryAs_AnyT2_First_WithMatchingType_ShouldReturnTrueAndValue()
    {
        Any<int, string> any = 42;

        bool success = any.TryAs<int, int, string>(out int value);

        success.Should().BeTrue();
        value.Should().Be(42);
    }

    [Fact]
    public void TryAs_AnyT2_First_WithNonMatchingType_ShouldReturnFalse()
    {
        Any<int, string> any = 42;

        bool success = any.TryAs<string, int, string>(out string? value);

        success.Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void TryAs_AnyT2_Second_WithMatchingType_ShouldReturnTrueAndValue()
    {
        Any<int, string> any = "hello";

        bool success = any.TryAs<string, int, string>(out string? value);

        success.Should().BeTrue();
        value.Should().Be("hello");
    }

    #endregion

    #region Any<T0, T1, T2>

    [Fact]
    public void Deconstruct_AnyT3_First_ShouldReturnFirst()
    {
        Any<int, string, double> any = 42;
        (int first, string? second, double third) = any;

        first.Should().Be(42);
        second.Should().BeNull();
        third.Should().Be(0);
    }

    [Fact]
    public void Deconstruct_AnyT3_Second_ShouldReturnSecond()
    {
        Any<int, string, double> any = "hello";
        (int first, string? second, double third) = any;

        first.Should().Be(0);
        second.Should().Be("hello");
        third.Should().Be(0);
    }

    [Fact]
    public void Deconstruct_AnyT3_Third_ShouldReturnThird()
    {
        Any<int, string, double> any = 3.14;
        (int first, string? second, double third) = any;

        first.Should().Be(0);
        second.Should().BeNull();
        third.Should().Be(3.14);
    }

    [Fact]
    public void ToTuple_AnyT3_First_ShouldReturnFirst()
    {
        Any<int, string, double> any = 42;
        (int First, string? Second, double Third) tuple = any.ToTuple();

        tuple.First.Should().Be(42);
        tuple.Second.Should().BeNull();
        tuple.Third.Should().Be(0);
    }

    [Fact]
    public void ToTuple_AnyT3_Second_ShouldReturnSecond()
    {
        Any<int, string, double> any = "hello";
        (int First, string? Second, double Third) tuple = any.ToTuple();

        tuple.First.Should().Be(0);
        tuple.Second.Should().Be("hello");
        tuple.Third.Should().Be(0);
    }

    [Fact]
    public void ToTuple_AnyT3_Third_ShouldReturnThird()
    {
        Any<int, string, double> any = 3.14;
        (int First, string? Second, double Third) tuple = any.ToTuple();

        tuple.First.Should().Be(0);
        tuple.Second.Should().BeNull();
        tuple.Third.Should().Be(3.14);
    }

    [Fact]
    public void Is_AnyT3_First_WithMatchingType_ShouldReturnTrue()
    {
        Any<int, string, double> any = 42;

        bool result = any.Is<int, int, string, double>();

        result.Should().BeTrue();
    }

    [Fact]
    public void Is_AnyT3_Third_WithMatchingType_ShouldReturnTrue()
    {
        Any<int, string, double> any = 3.14;

        bool result = any.Is<double, int, string, double>();

        result.Should().BeTrue();
    }

    [Fact]
    public void Is_AnyT3_WithNonMatchingType_ShouldReturnFalse()
    {
        Any<int, string, double> any = 42;

        bool result = any.Is<bool, int, string, double>();

        result.Should().BeFalse();
    }

    [Fact]
    public void As_AnyT3_First_WithMatchingType_ShouldReturnValue()
    {
        Any<int, string, double> any = 42;

        int? result = any.As<int, int, string, double>();

        result.Should().Be(42);
    }

    [Fact]
    public void As_AnyT3_Third_WithMatchingType_ShouldReturnValue()
    {
        Any<int, string, double> any = 3.14;

        double? result = any.As<double, int, string, double>();

        result.Should().Be(3.14);
    }

    [Fact]
    public void As_AnyT3_WithNonMatchingType_ShouldReturnDefault()
    {
        Any<int, string, double> any = 42;

        string? result = any.As<string, int, string, double>();

        result.Should().BeNull();
    }

    [Fact]
    public void TryAs_AnyT3_First_WithMatchingType_ShouldReturnTrueAndValue()
    {
        Any<int, string, double> any = 42;

        bool success = any.TryAs<int, int, string, double>(out int value);

        success.Should().BeTrue();
        value.Should().Be(42);
    }

    [Fact]
    public void TryAs_AnyT3_Third_WithMatchingType_ShouldReturnTrueAndValue()
    {
        Any<int, string, double> any = 3.14;

        bool success = any.TryAs<double, int, string, double>(out double value);

        success.Should().BeTrue();
        value.Should().Be(3.14);
    }

    [Fact]
    public void TryAs_AnyT3_WithNonMatchingType_ShouldReturnFalse()
    {
        Any<int, string, double> any = 42;

        bool success = any.TryAs<bool, int, string, double>(out bool value);

        success.Should().BeFalse();
        value.Should().BeFalse();
    }

    #endregion
}
