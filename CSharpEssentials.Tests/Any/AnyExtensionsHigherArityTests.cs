using CSharpEssentials.Any;
using FluentAssertions;

namespace CSharpEssentials.Tests.Any;

public class AnyExtensionsHigherArityTests
{
    private static readonly Guid SampleGuid = Guid.Parse("8f1c2a8e-3b4d-4e5f-9a6b-7c8d9e0f1a2b");

    #region Any<int, string, bool, double>

    [Fact]
    public void Deconstruct_AnyT4_Fourth_ShouldReturnOnlyFourth()
    {
        Any<int, string, bool, double> any = 2.5;
        (int first, string? second, bool third, double fourth) = any;

        first.Should().Be(0);
        second.Should().BeNull();
        third.Should().Be(false);
        fourth.Should().Be(2.5);
    }

    [Fact]
    public void Deconstruct_AnyT4_First_ShouldReturnOnlyFirst()
    {
        Any<int, string, bool, double> any = 42;
        (int first, string? second, bool third, double fourth) = any;

        first.Should().Be(42);
        second.Should().BeNull();
        third.Should().Be(false);
        fourth.Should().Be(0);
    }

    [Fact]
    public void ToTuple_AnyT4_Second_ShouldReturnOnlySecond()
    {
        Any<int, string, bool, double> any = "hello";
        (int First, string? Second, bool Third, double Fourth) tuple = any.ToTuple();

        tuple.First.Should().Be(0);
        tuple.Second.Should().Be("hello");
        tuple.Third.Should().Be(false);
        tuple.Fourth.Should().Be(0);
    }

    [Fact]
    public void ToTuple_AnyT4_Fourth_ShouldReturnOnlyFourth()
    {
        Any<int, string, bool, double> any = 2.5;
        (int First, string? Second, bool Third, double Fourth) tuple = any.ToTuple();

        tuple.First.Should().Be(0);
        tuple.Second.Should().BeNull();
        tuple.Third.Should().Be(false);
        tuple.Fourth.Should().Be(2.5);
    }

    [Fact]
    public void Is_AnyT4_Fourth_WithMatchingType_ShouldReturnTrue()
    {
        Any<int, string, bool, double> any = 2.5;

        any.Is<double, int, string, bool, double>().Should().BeTrue();
    }

    [Fact]
    public void Is_AnyT4_WithNonMatchingType_ShouldReturnFalse()
    {
        Any<int, string, bool, double> any = 2.5;

        any.Is<int, int, string, bool, double>().Should().BeFalse();
    }

    [Fact]
    public void As_AnyT4_Fourth_WithMatchingType_ShouldReturnValue()
    {
        Any<int, string, bool, double> any = 2.5;

        double result = any.As<double, int, string, bool, double>();

        result.Should().Be(2.5);
    }

    [Fact]
    public void As_AnyT4_WithNonMatchingType_ShouldReturnDefault()
    {
        Any<int, string, bool, double> any = 2.5;

        string? result = any.As<string, int, string, bool, double>();

        result.Should().BeNull();
    }

    [Fact]
    public void TryAs_AnyT4_Fourth_WithMatchingType_ShouldReturnTrueAndValue()
    {
        Any<int, string, bool, double> any = 2.5;

        bool success = any.TryAs<double, int, string, bool, double>(out double value);

        success.Should().BeTrue();
        value.Should().Be(2.5);
    }

    [Fact]
    public void TryAs_AnyT4_WithNonMatchingType_ShouldReturnFalseAndDefault()
    {
        Any<int, string, bool, double> any = 2.5;

        bool success = any.TryAs<string, int, string, bool, double>(out string? value);

        success.Should().BeFalse();
        value.Should().BeNull();
    }

    #endregion

    #region Any<int, string, bool, double, long>

    [Fact]
    public void Deconstruct_AnyT5_Fifth_ShouldReturnOnlyFifth()
    {
        Any<int, string, bool, double, long> any = 7L;
        (int first, string? second, bool third, double fourth, long fifth) = any;

        first.Should().Be(0);
        second.Should().BeNull();
        third.Should().Be(false);
        fourth.Should().Be(0);
        fifth.Should().Be(7L);
    }

    [Fact]
    public void Deconstruct_AnyT5_First_ShouldReturnOnlyFirst()
    {
        Any<int, string, bool, double, long> any = 42;
        (int first, string? second, bool third, double fourth, long fifth) = any;

        first.Should().Be(42);
        second.Should().BeNull();
        third.Should().Be(false);
        fourth.Should().Be(0);
        fifth.Should().Be(0L);
    }

    [Fact]
    public void ToTuple_AnyT5_Second_ShouldReturnOnlySecond()
    {
        Any<int, string, bool, double, long> any = "hello";
        (int First, string? Second, bool Third, double Fourth, long Fifth) tuple = any.ToTuple();

        tuple.First.Should().Be(0);
        tuple.Second.Should().Be("hello");
        tuple.Third.Should().Be(false);
        tuple.Fourth.Should().Be(0);
        tuple.Fifth.Should().Be(0L);
    }

    [Fact]
    public void ToTuple_AnyT5_Fifth_ShouldReturnOnlyFifth()
    {
        Any<int, string, bool, double, long> any = 7L;
        (int First, string? Second, bool Third, double Fourth, long Fifth) tuple = any.ToTuple();

        tuple.First.Should().Be(0);
        tuple.Second.Should().BeNull();
        tuple.Third.Should().Be(false);
        tuple.Fourth.Should().Be(0);
        tuple.Fifth.Should().Be(7L);
    }

    [Fact]
    public void Is_AnyT5_Fifth_WithMatchingType_ShouldReturnTrue()
    {
        Any<int, string, bool, double, long> any = 7L;

        any.Is<long, int, string, bool, double, long>().Should().BeTrue();
    }

    [Fact]
    public void Is_AnyT5_WithNonMatchingType_ShouldReturnFalse()
    {
        Any<int, string, bool, double, long> any = 7L;

        any.Is<int, int, string, bool, double, long>().Should().BeFalse();
    }

    [Fact]
    public void As_AnyT5_Fifth_WithMatchingType_ShouldReturnValue()
    {
        Any<int, string, bool, double, long> any = 7L;

        long result = any.As<long, int, string, bool, double, long>();

        result.Should().Be(7L);
    }

    [Fact]
    public void As_AnyT5_WithNonMatchingType_ShouldReturnDefault()
    {
        Any<int, string, bool, double, long> any = 7L;

        string? result = any.As<string, int, string, bool, double, long>();

        result.Should().BeNull();
    }

    [Fact]
    public void TryAs_AnyT5_Fifth_WithMatchingType_ShouldReturnTrueAndValue()
    {
        Any<int, string, bool, double, long> any = 7L;

        bool success = any.TryAs<long, int, string, bool, double, long>(out long value);

        success.Should().BeTrue();
        value.Should().Be(7L);
    }

    [Fact]
    public void TryAs_AnyT5_WithNonMatchingType_ShouldReturnFalseAndDefault()
    {
        Any<int, string, bool, double, long> any = 7L;

        bool success = any.TryAs<string, int, string, bool, double, long>(out string? value);

        success.Should().BeFalse();
        value.Should().BeNull();
    }

    #endregion

    #region Any<int, string, bool, double, long, decimal>

    [Fact]
    public void Deconstruct_AnyT6_Sixth_ShouldReturnOnlySixth()
    {
        Any<int, string, bool, double, long, decimal> any = 9.9m;
        (int first, string? second, bool third, double fourth, long fifth, decimal sixth) = any;

        first.Should().Be(0);
        second.Should().BeNull();
        third.Should().Be(false);
        fourth.Should().Be(0);
        fifth.Should().Be(0L);
        sixth.Should().Be(9.9m);
    }

    [Fact]
    public void Deconstruct_AnyT6_First_ShouldReturnOnlyFirst()
    {
        Any<int, string, bool, double, long, decimal> any = 42;
        (int first, string? second, bool third, double fourth, long fifth, decimal sixth) = any;

        first.Should().Be(42);
        second.Should().BeNull();
        third.Should().Be(false);
        fourth.Should().Be(0);
        fifth.Should().Be(0L);
        sixth.Should().Be(0m);
    }

    [Fact]
    public void ToTuple_AnyT6_Second_ShouldReturnOnlySecond()
    {
        Any<int, string, bool, double, long, decimal> any = "hello";
        (int First, string? Second, bool Third, double Fourth, long Fifth, decimal Sixth) tuple = any.ToTuple();

        tuple.First.Should().Be(0);
        tuple.Second.Should().Be("hello");
        tuple.Third.Should().Be(false);
        tuple.Fourth.Should().Be(0);
        tuple.Fifth.Should().Be(0L);
        tuple.Sixth.Should().Be(0m);
    }

    [Fact]
    public void ToTuple_AnyT6_Sixth_ShouldReturnOnlySixth()
    {
        Any<int, string, bool, double, long, decimal> any = 9.9m;
        (int First, string? Second, bool Third, double Fourth, long Fifth, decimal Sixth) tuple = any.ToTuple();

        tuple.First.Should().Be(0);
        tuple.Second.Should().BeNull();
        tuple.Third.Should().Be(false);
        tuple.Fourth.Should().Be(0);
        tuple.Fifth.Should().Be(0L);
        tuple.Sixth.Should().Be(9.9m);
    }

    [Fact]
    public void Is_AnyT6_Sixth_WithMatchingType_ShouldReturnTrue()
    {
        Any<int, string, bool, double, long, decimal> any = 9.9m;

        any.Is<decimal, int, string, bool, double, long, decimal>().Should().BeTrue();
    }

    [Fact]
    public void Is_AnyT6_WithNonMatchingType_ShouldReturnFalse()
    {
        Any<int, string, bool, double, long, decimal> any = 9.9m;

        any.Is<int, int, string, bool, double, long, decimal>().Should().BeFalse();
    }

    [Fact]
    public void As_AnyT6_Sixth_WithMatchingType_ShouldReturnValue()
    {
        Any<int, string, bool, double, long, decimal> any = 9.9m;

        decimal result = any.As<decimal, int, string, bool, double, long, decimal>();

        result.Should().Be(9.9m);
    }

    [Fact]
    public void As_AnyT6_WithNonMatchingType_ShouldReturnDefault()
    {
        Any<int, string, bool, double, long, decimal> any = 9.9m;

        string? result = any.As<string, int, string, bool, double, long, decimal>();

        result.Should().BeNull();
    }

    [Fact]
    public void TryAs_AnyT6_Sixth_WithMatchingType_ShouldReturnTrueAndValue()
    {
        Any<int, string, bool, double, long, decimal> any = 9.9m;

        bool success = any.TryAs<decimal, int, string, bool, double, long, decimal>(out decimal value);

        success.Should().BeTrue();
        value.Should().Be(9.9m);
    }

    [Fact]
    public void TryAs_AnyT6_WithNonMatchingType_ShouldReturnFalseAndDefault()
    {
        Any<int, string, bool, double, long, decimal> any = 9.9m;

        bool success = any.TryAs<string, int, string, bool, double, long, decimal>(out string? value);

        success.Should().BeFalse();
        value.Should().BeNull();
    }

    #endregion

    #region Any<int, string, bool, double, long, decimal, Guid>

    [Fact]
    public void Deconstruct_AnyT7_Seventh_ShouldReturnOnlySeventh()
    {
        Any<int, string, bool, double, long, decimal, Guid> any = SampleGuid;
        (int first, string? second, bool third, double fourth, long fifth, decimal sixth, Guid seventh) = any;

        first.Should().Be(0);
        second.Should().BeNull();
        third.Should().Be(false);
        fourth.Should().Be(0);
        fifth.Should().Be(0L);
        sixth.Should().Be(0m);
        seventh.Should().Be(SampleGuid);
    }

    [Fact]
    public void Deconstruct_AnyT7_First_ShouldReturnOnlyFirst()
    {
        Any<int, string, bool, double, long, decimal, Guid> any = 42;
        (int first, string? second, bool third, double fourth, long fifth, decimal sixth, Guid seventh) = any;

        first.Should().Be(42);
        second.Should().BeNull();
        third.Should().Be(false);
        fourth.Should().Be(0);
        fifth.Should().Be(0L);
        sixth.Should().Be(0m);
        seventh.Should().Be(Guid.Empty);
    }

    [Fact]
    public void ToTuple_AnyT7_Second_ShouldReturnOnlySecond()
    {
        Any<int, string, bool, double, long, decimal, Guid> any = "hello";
        (int First, string? Second, bool Third, double Fourth, long Fifth, decimal Sixth, Guid Seventh) tuple = any.ToTuple();

        tuple.First.Should().Be(0);
        tuple.Second.Should().Be("hello");
        tuple.Third.Should().Be(false);
        tuple.Fourth.Should().Be(0);
        tuple.Fifth.Should().Be(0L);
        tuple.Sixth.Should().Be(0m);
        tuple.Seventh.Should().Be(Guid.Empty);
    }

    [Fact]
    public void ToTuple_AnyT7_Seventh_ShouldReturnOnlySeventh()
    {
        Any<int, string, bool, double, long, decimal, Guid> any = SampleGuid;
        (int First, string? Second, bool Third, double Fourth, long Fifth, decimal Sixth, Guid Seventh) tuple = any.ToTuple();

        tuple.First.Should().Be(0);
        tuple.Second.Should().BeNull();
        tuple.Third.Should().Be(false);
        tuple.Fourth.Should().Be(0);
        tuple.Fifth.Should().Be(0L);
        tuple.Sixth.Should().Be(0m);
        tuple.Seventh.Should().Be(SampleGuid);
    }

    [Fact]
    public void Is_AnyT7_Seventh_WithMatchingType_ShouldReturnTrue()
    {
        Any<int, string, bool, double, long, decimal, Guid> any = SampleGuid;

        any.Is<Guid, int, string, bool, double, long, decimal, Guid>().Should().BeTrue();
    }

    [Fact]
    public void Is_AnyT7_WithNonMatchingType_ShouldReturnFalse()
    {
        Any<int, string, bool, double, long, decimal, Guid> any = SampleGuid;

        any.Is<int, int, string, bool, double, long, decimal, Guid>().Should().BeFalse();
    }

    [Fact]
    public void As_AnyT7_Seventh_WithMatchingType_ShouldReturnValue()
    {
        Any<int, string, bool, double, long, decimal, Guid> any = SampleGuid;

        Guid result = any.As<Guid, int, string, bool, double, long, decimal, Guid>();

        result.Should().Be(SampleGuid);
    }

    [Fact]
    public void As_AnyT7_WithNonMatchingType_ShouldReturnDefault()
    {
        Any<int, string, bool, double, long, decimal, Guid> any = SampleGuid;

        string? result = any.As<string, int, string, bool, double, long, decimal, Guid>();

        result.Should().BeNull();
    }

    [Fact]
    public void TryAs_AnyT7_Seventh_WithMatchingType_ShouldReturnTrueAndValue()
    {
        Any<int, string, bool, double, long, decimal, Guid> any = SampleGuid;

        bool success = any.TryAs<Guid, int, string, bool, double, long, decimal, Guid>(out Guid value);

        success.Should().BeTrue();
        value.Should().Be(SampleGuid);
    }

    [Fact]
    public void TryAs_AnyT7_WithNonMatchingType_ShouldReturnFalseAndDefault()
    {
        Any<int, string, bool, double, long, decimal, Guid> any = SampleGuid;

        bool success = any.TryAs<string, int, string, bool, double, long, decimal, Guid>(out string? value);

        success.Should().BeFalse();
        value.Should().BeNull();
    }

    #endregion

    #region Any<int, string, bool, double, long, decimal, Guid, char>

    [Fact]
    public void Deconstruct_AnyT8_Eighth_ShouldReturnOnlyEighth()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 'z';
        (int first, string? second, bool third, double fourth, long fifth, decimal sixth, Guid seventh, char eighth) = any;

        first.Should().Be(0);
        second.Should().BeNull();
        third.Should().Be(false);
        fourth.Should().Be(0);
        fifth.Should().Be(0L);
        sixth.Should().Be(0m);
        seventh.Should().Be(Guid.Empty);
        eighth.Should().Be('z');
    }

    [Fact]
    public void Deconstruct_AnyT8_First_ShouldReturnOnlyFirst()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 42;
        (int first, string? second, bool third, double fourth, long fifth, decimal sixth, Guid seventh, char eighth) = any;

        first.Should().Be(42);
        second.Should().BeNull();
        third.Should().Be(false);
        fourth.Should().Be(0);
        fifth.Should().Be(0L);
        sixth.Should().Be(0m);
        seventh.Should().Be(Guid.Empty);
        eighth.Should().Be('\0');
    }

    [Fact]
    public void ToTuple_AnyT8_Second_ShouldReturnOnlySecond()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = "hello";
        (int First, string? Second, bool Third, double Fourth, long Fifth, decimal Sixth, Guid Seventh, char Eighth) tuple = any.ToTuple();

        tuple.First.Should().Be(0);
        tuple.Second.Should().Be("hello");
        tuple.Third.Should().Be(false);
        tuple.Fourth.Should().Be(0);
        tuple.Fifth.Should().Be(0L);
        tuple.Sixth.Should().Be(0m);
        tuple.Seventh.Should().Be(Guid.Empty);
        tuple.Eighth.Should().Be('\0');
    }

    [Fact]
    public void ToTuple_AnyT8_Eighth_ShouldReturnOnlyEighth()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 'z';
        (int First, string? Second, bool Third, double Fourth, long Fifth, decimal Sixth, Guid Seventh, char Eighth) tuple = any.ToTuple();

        tuple.First.Should().Be(0);
        tuple.Second.Should().BeNull();
        tuple.Third.Should().Be(false);
        tuple.Fourth.Should().Be(0);
        tuple.Fifth.Should().Be(0L);
        tuple.Sixth.Should().Be(0m);
        tuple.Seventh.Should().Be(Guid.Empty);
        tuple.Eighth.Should().Be('z');
    }

    [Fact]
    public void Is_AnyT8_Eighth_WithMatchingType_ShouldReturnTrue()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 'z';

        any.Is<char, int, string, bool, double, long, decimal, Guid, char>().Should().BeTrue();
    }

    [Fact]
    public void Is_AnyT8_WithNonMatchingType_ShouldReturnFalse()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 'z';

        any.Is<int, int, string, bool, double, long, decimal, Guid, char>().Should().BeFalse();
    }

    [Fact]
    public void As_AnyT8_Eighth_WithMatchingType_ShouldReturnValue()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 'z';

        char result = any.As<char, int, string, bool, double, long, decimal, Guid, char>();

        result.Should().Be('z');
    }

    [Fact]
    public void As_AnyT8_WithNonMatchingType_ShouldReturnDefault()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 'z';

        string? result = any.As<string, int, string, bool, double, long, decimal, Guid, char>();

        result.Should().BeNull();
    }

    [Fact]
    public void TryAs_AnyT8_Eighth_WithMatchingType_ShouldReturnTrueAndValue()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 'z';

        bool success = any.TryAs<char, int, string, bool, double, long, decimal, Guid, char>(out char value);

        success.Should().BeTrue();
        value.Should().Be('z');
    }

    [Fact]
    public void TryAs_AnyT8_WithNonMatchingType_ShouldReturnFalseAndDefault()
    {
        Any<int, string, bool, double, long, decimal, Guid, char> any = 'z';

        bool success = any.TryAs<string, int, string, bool, double, long, decimal, Guid, char>(out string? value);

        success.Should().BeFalse();
        value.Should().BeNull();
    }

    #endregion
}
