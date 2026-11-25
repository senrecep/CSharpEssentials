// Produced by scripts/generate-typed-results.py; do not edit.
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;

public sealed class ResultTE1SelectTests
{
    [Fact]
    public void Select_Should_MapTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;

        Result<string, NotFoundError> result = source.Select(v => { calls++; return TypedFixture.Text(v); });

        calls.Should().Be(1);
        Result<string, NotFoundError> expected = TypedFixture.Text(5);
        result.Should().Be(expected);
    }

    [Fact]
    public void Select_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;

        Result<string, NotFoundError> result = source.Select(v => { calls++; return TypedFixture.Text(v); });

        calls.Should().Be(0);
        Result<string, NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void Select_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;

        Action act = () => _ = source.Select(v => { calls++; return TypedFixture.Text(v); });

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public void Select_Should_Throw_When_TheSelectorReturnsNull()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;

        Action act = () => _ = source.Select<string>(v => { calls++; return null!; });

        act.Should().Throw<ArgumentNullException>();
        calls.Should().Be(1);
    }

    [Fact]
    public void SelectMany_Should_ProjectBothValues_When_BothResultsHoldAValue()
    {
        Result<int, NotFoundError> source = 5;
        Result<int, NotFoundError> other = 7;

        Result<string, NotFoundError> result = from a in source from b in other select TypedFixture.Text(a + b);

        Result<string, NotFoundError> expected = TypedFixture.Text(12);
        result.Should().Be(expected);
    }

    [Fact]
    public void SelectMany_Should_ReturnTheInnerError_When_TheInnerResultFails()
    {
        Result<int, NotFoundError> source = 5;
        Result<int, NotFoundError> other = TypedFixture.NotFound;

        Result<string, NotFoundError> result = from a in source from b in other select TypedFixture.Text(a + b);

        Result<string, NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void SelectMany_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        Result<int, NotFoundError> other = 7;

        Result<string, NotFoundError> result = from a in source from b in other select TypedFixture.Text(a + b);

        Result<string, NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void SelectMany_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        Result<int, NotFoundError> other = 7;

        Action act = () => _ = from a in source from b in other select TypedFixture.Text(a + b);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SelectManyWiden1_Should_ProjectBothValues_When_BothResultsHoldAValue()
    {
        Result<int, NotFoundError> source = 5;
        Result<int, ValidationError> other = 7;

        Result<string, NotFoundError, ValidationError> result = from a in source from b in other select TypedFixture.Text(a + b);

        Result<string, NotFoundError, ValidationError> expected = TypedFixture.Text(12);
        result.Should().Be(expected);
    }

    [Fact]
    public void SelectManyWiden1_Should_MoveTheInnerErrorToItsAddedSlot_When_TheInnerResultHoldsValidationError()
    {
        Result<int, NotFoundError> source = 5;
        Result<int, ValidationError> other = TypedFixture.Invalid;

        Result<string, NotFoundError, ValidationError> result = from a in source from b in other select TypedFixture.Text(a + b);

        Result<string, NotFoundError, ValidationError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public void SelectManyWiden1_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        Result<int, ValidationError> other = 7;

        Result<string, NotFoundError, ValidationError> result = from a in source from b in other select TypedFixture.Text(a + b);

        Result<string, NotFoundError, ValidationError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void SelectManyWiden2_Should_ProjectBothValues_When_BothResultsHoldAValue()
    {
        Result<int, NotFoundError> source = 5;
        Result<int, ValidationError, PlainError> other = 7;

        Result<string, NotFoundError, ValidationError, PlainError> result = from a in source from b in other select TypedFixture.Text(a + b);

        Result<string, NotFoundError, ValidationError, PlainError> expected = TypedFixture.Text(12);
        result.Should().Be(expected);
    }

    [Fact]
    public void SelectManyWiden2_Should_MoveTheInnerErrorToItsAddedSlot_When_TheInnerResultHoldsValidationError()
    {
        Result<int, NotFoundError> source = 5;
        Result<int, ValidationError, PlainError> other = TypedFixture.Invalid;

        Result<string, NotFoundError, ValidationError, PlainError> result = from a in source from b in other select TypedFixture.Text(a + b);

        Result<string, NotFoundError, ValidationError, PlainError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public void SelectManyWiden2_Should_MoveTheInnerErrorToItsAddedSlot_When_TheInnerResultHoldsPlainError()
    {
        Result<int, NotFoundError> source = 5;
        Result<int, ValidationError, PlainError> other = TypedFixture.Plain;

        Result<string, NotFoundError, ValidationError, PlainError> result = from a in source from b in other select TypedFixture.Text(a + b);

        Result<string, NotFoundError, ValidationError, PlainError> expected = TypedFixture.Plain;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Plain);
    }

    [Fact]
    public void SelectManyWiden2_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        Result<int, ValidationError, PlainError> other = 7;

        Result<string, NotFoundError, ValidationError, PlainError> result = from a in source from b in other select TypedFixture.Text(a + b);

        Result<string, NotFoundError, ValidationError, PlainError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }
}
