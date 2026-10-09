using CSharpEssentials.Maybe;
using FluentAssertions;

namespace CSharpEssentials.Tests.Maybe;

public sealed class MaybeEqualityTests
{
    [Fact]
    public void None_Should_NotEqual_DefaultValue()
    {
        Maybe<int> none = Maybe<int>.None;

        (none == 0).Should().BeFalse();
        (none != 0).Should().BeTrue();
        none.Equals((object)0).Should().BeFalse();
    }

    [Fact]
    public void Some_Should_Equal_SameValue_Including_Default()
    {
        Maybe<int> zero = Maybe<int>.From(0);
        Maybe<int> forty = Maybe<int>.From(42);

        (zero == 0).Should().BeTrue();
        (zero != 0).Should().BeFalse();
        (forty == 42).Should().BeTrue();
        (forty != 42).Should().BeFalse();
        (forty == 41).Should().BeFalse();
        (forty != 41).Should().BeTrue();
        forty.Equals((object)42).Should().BeTrue();
    }

    [Fact]
    public void Operators_Should_Be_Symmetric_For_Maybe_Operands()
    {
        Maybe<int> a = Maybe<int>.From(1);
        Maybe<int> b = Maybe<int>.From(1);
        Maybe<int> none = Maybe<int>.None;

        (a == b).Should().BeTrue();
        (a != b).Should().BeFalse();
        (a == none).Should().BeFalse();
        (none != a).Should().BeTrue();
        (none == Maybe<int>.None).Should().BeTrue();
    }

    [Fact]
    public void ReferenceType_None_Should_Equal_Null()
    {
        Maybe<string> none = Maybe<string>.None;

        (none == null).Should().BeTrue();
        (none != null).Should().BeFalse();
        (null == none).Should().BeTrue();
        (null != none).Should().BeFalse();
        none.Equals((object?)null).Should().BeFalse();
    }

    [Fact]
    public void ReferenceType_None_Should_NotEqualObjectNull_When_ComparedThroughObject()
    {
        Maybe<string> none = Maybe<string>.None;

        // A typed null means absence (operator ==(Maybe<T>, T?)), but Equals(object?) and operator ==(Maybe<T>, object)
        // follow the BCL contract that x.Equals(null) is false. The static type of the operand picks the rule.
        ((none == (string?)null), (none == (object)null!), none.Equals((object?)null)).Should().Be((true, false, false));
    }

    [Fact]
    public void ReferenceType_Some_Should_Compare_By_Value()
    {
        Maybe<string> some = Maybe<string>.From("abc");

        (some == "abc").Should().BeTrue();
        (some != "abc").Should().BeFalse();
        (some == "xyz").Should().BeFalse();
        (some == null).Should().BeFalse();
        (some != null).Should().BeTrue();
        (null == some).Should().BeFalse();
        (null != some).Should().BeTrue();
        ("abc" == some).Should().BeTrue();
        ("xyz" != some).Should().BeTrue();
    }

    [Fact]
    public void ReverseOrder_Should_Match_ForwardOrder_For_ValueType()
    {
        Maybe<int> none = Maybe<int>.None;
        Maybe<int> some5 = Maybe<int>.From(5);

        (0 == none).Should().BeFalse();
        (0 != none).Should().BeTrue();
        (5 == some5).Should().BeTrue();
        (5 != some5).Should().BeFalse();
        (4 == some5).Should().BeFalse();
        (4 != some5).Should().BeTrue();
    }

    [Fact]
    public void EqualsDefault_Should_Compile_And_Bind_To_Default_Of_T()
    {
        Maybe<string> noneRef = Maybe<string>.None;
        Maybe<string> someRef = Maybe<string>.From("a");
        Maybe<int> noneVal = Maybe<int>.None;
        Maybe<int> someZero = Maybe<int>.From(0);

        (noneRef == default).Should().BeTrue();
        (default == noneRef).Should().BeTrue();
        (someRef == default).Should().BeFalse();
        (noneVal == default).Should().BeFalse();
        (someZero == default).Should().BeTrue();
    }

    [Fact]
    public void NullableValueType_Should_Treat_Null_As_Absence()
    {
        Maybe<int?> none = Maybe<int?>.None;
        Maybe<int?> some = Maybe<int?>.From(3);
        int? nothing = NullInt();

        (none == nothing).Should().BeTrue();
        (some == nothing).Should().BeFalse();
        (some == 3).Should().BeTrue();
        (nothing == none).Should().BeTrue();
        (none == 0).Should().BeFalse();
    }

    [Fact]
    public void Nested_Maybe_Should_Compare_By_Inner_Value()
    {
        Maybe<Maybe<int>> none = Maybe<Maybe<int>>.None;
        Maybe<Maybe<int>> some = Maybe<Maybe<int>>.From(Maybe<int>.From(1));
        Maybe<Maybe<int>> someNone = Maybe<Maybe<int>>.From(Maybe<int>.None);

        (some == Maybe<int>.From(1)).Should().BeTrue();
        (some == Maybe<int>.From(2)).Should().BeFalse();
        (none == Maybe<int>.None).Should().BeFalse();
        (someNone == Maybe<int>.None).Should().BeTrue();
        (some == Maybe<Maybe<int>>.From(Maybe<int>.From(1))).Should().BeTrue();
        (some == none).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_Should_Be_Consistent_With_Equals()
    {
        Maybe<string> a = Maybe<string>.From("abc");
        Maybe<string> b = Maybe<string>.From("abc");

        a.GetHashCode().Should().Be(b.GetHashCode());
        a.GetHashCode().Should().Be("abc".GetHashCode(StringComparison.Ordinal));
        Maybe<string>.None.GetHashCode().Should().Be(0);
    }

    [Fact]
    public void FromTry_Should_Rethrow_OperationCanceledException()
    {
        Action act = () => Maybe<int>.FromTry(() => throw new OperationCanceledException());

        act.Should().Throw<OperationCanceledException>();
    }

    [Fact]
    public void FromTry_Should_Rethrow_TaskCanceledException()
    {
        Action act = () => Maybe<int>.FromTry(() => throw new TaskCanceledException());

        act.Should().Throw<TaskCanceledException>();
    }

    [Fact]
    public void FromTry_Should_ReturnNone_For_Other_Exceptions()
    {
        Maybe<int>.FromTry(() => throw new InvalidOperationException()).HasNoValue.Should().BeTrue();
        Maybe<int>.FromTry(() => throw new ArgumentException("x")).HasNoValue.Should().BeTrue();
        CSharpEssentials.Maybe.Maybe.FromTry<int>(() => throw new FormatException()).HasNoValue.Should().BeTrue();
    }

    [Fact]
    public void ToString_Should_ReturnSome_When_HasValue()
    {
        Maybe<int>.From(42).ToString().Should().Be("Some(42)");
        Maybe<string>.From("abc").ToString().Should().Be("Some(abc)");
        Maybe<int>.From(0).ToString().Should().Be("Some(0)");
        Maybe<Uri>.From(new Uri("https://example.com/")).ToString().Should().Be("Some(https://example.com/)");
    }

    [Fact]
    public void ToString_Should_ReturnNone_When_NoValue()
    {
        Maybe<int>.None.ToString().Should().Be("None");
        Maybe<string>.From((string?)null).ToString().Should().Be("None");
    }

    private static int? NullInt() => null;
}
