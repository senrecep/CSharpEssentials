// Produced by scripts/generate-typed-results.py; do not edit.
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;

public sealed class ResultTE1BindTests
{
    [Fact]
    public void Bind_Should_ReturnTheInnerResult_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;

        Result<string, NotFoundError> result = source.Bind<string>(v => { calls++; return TypedFixture.Text(v); });

        calls.Should().Be(1);
        Result<string, NotFoundError> expected = TypedFixture.Text(5);
        result.Should().Be(expected);
    }

    [Fact]
    public void Bind_Should_ReturnTheInnerError_When_TheInnerResultFails()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;

        Result<string, NotFoundError> result = source.Bind<string>(v => { calls++; return TypedFixture.NotFound; });

        calls.Should().Be(1);
        Result<string, NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void Bind_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;

        Result<string, NotFoundError> result = source.Bind<string>(v => { calls++; return TypedFixture.Text(v); });

        calls.Should().Be(0);
        Result<string, NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void Bind_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;

        Action act = () => _ = source.Bind<string>(v => { calls++; return TypedFixture.Text(v); });

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public void BindWiden1_Should_ReturnTheInnerValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;

        Result<string, NotFoundError, ValidationError> result = source.BindWiden<string, ValidationError>(v => { calls++; return TypedFixture.Text(v); });

        calls.Should().Be(1);
        Result<string, NotFoundError, ValidationError> expected = TypedFixture.Text(5);
        result.Should().Be(expected);
    }

    [Fact]
    public void BindWiden1_Should_MoveTheInnerErrorToItsAddedSlot_When_TheInnerResultHoldsValidationError()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;

        Result<string, NotFoundError, ValidationError> result = source.BindWiden<string, ValidationError>(v => { calls++; return new Result<string, ValidationError>(TypedFixture.Invalid); });

        calls.Should().Be(1);
        Result<string, NotFoundError, ValidationError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public void BindWiden1_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;

        Result<string, NotFoundError, ValidationError> result = source.BindWiden<string, ValidationError>(v => { calls++; return TypedFixture.Text(v); });

        calls.Should().Be(0);
        Result<string, NotFoundError, ValidationError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void BindWiden1_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;

        Action act = () => _ = source.BindWiden<string, ValidationError>(v => { calls++; return TypedFixture.Text(v); });

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public void BindWiden2_Should_ReturnTheInnerValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;

        Result<string, NotFoundError, ValidationError, PlainError> result = source.BindWiden<string, ValidationError, PlainError>(v => { calls++; return TypedFixture.Text(v); });

        calls.Should().Be(1);
        Result<string, NotFoundError, ValidationError, PlainError> expected = TypedFixture.Text(5);
        result.Should().Be(expected);
    }

    [Fact]
    public void BindWiden2_Should_MoveTheInnerErrorToItsAddedSlot_When_TheInnerResultHoldsValidationError()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;

        Result<string, NotFoundError, ValidationError, PlainError> result = source.BindWiden<string, ValidationError, PlainError>(v => { calls++; return new Result<string, ValidationError, PlainError>(TypedFixture.Invalid); });

        calls.Should().Be(1);
        Result<string, NotFoundError, ValidationError, PlainError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public void BindWiden2_Should_MoveTheInnerErrorToItsAddedSlot_When_TheInnerResultHoldsPlainError()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;

        Result<string, NotFoundError, ValidationError, PlainError> result = source.BindWiden<string, ValidationError, PlainError>(v => { calls++; return new Result<string, ValidationError, PlainError>(TypedFixture.Plain); });

        calls.Should().Be(1);
        Result<string, NotFoundError, ValidationError, PlainError> expected = TypedFixture.Plain;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Plain);
    }

    [Fact]
    public void BindWiden2_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;

        Result<string, NotFoundError, ValidationError, PlainError> result = source.BindWiden<string, ValidationError, PlainError>(v => { calls++; return TypedFixture.Text(v); });

        calls.Should().Be(0);
        Result<string, NotFoundError, ValidationError, PlainError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void BindWiden2_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;

        Action act = () => _ = source.BindWiden<string, ValidationError, PlainError>(v => { calls++; return TypedFixture.Text(v); });

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }
}
