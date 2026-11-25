using CSharpEssentials.Maybe;
using CSharpEssentials.ResultPattern.Testing;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results.Testing;

public class MaybeAssertionExtensionsTests
{
    [Fact]
    public void ShouldHaveValue_Should_Return_Value_When_Maybe_Has_Value()
    {
        Maybe<string> maybe = "pen";

        maybe.ShouldHaveValue().Should().Be("pen");
    }

    [Fact]
    public void ShouldHaveValue_Should_Throw_When_Maybe_Is_None()
    {
        Action act = () => Maybe<string>.None.ShouldHaveValue();

        act.Should().Throw<ResultAssertionException>().WithMessage("Expected a value of type String, but the maybe has none.");
    }

    [Fact]
    public void ShouldBeNone_Should_Pass_When_Maybe_Is_None()
    {
        Action act = () => Maybe<int>.None.ShouldBeNone();

        act.Should().NotThrow();
    }

    [Fact]
    public void ShouldBeNone_Should_Show_Value_When_Maybe_Has_Value()
    {
        Maybe<int> maybe = 4;

        Action act = () => maybe.ShouldBeNone();

        act.Should().Throw<ResultAssertionException>().WithMessage("Expected no value, but the maybe has Int32 4.");
    }
}
