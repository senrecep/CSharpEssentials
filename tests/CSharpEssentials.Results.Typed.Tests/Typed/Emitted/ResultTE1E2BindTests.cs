// Produced by scripts/generate-typed-results.py; do not edit.
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;

public sealed class ResultTE1E2BindTests
{
    [Fact]
    public void Bind_Should_ReturnTheInnerResult_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;

        Result<string, NotFoundError, ConflictError> result = source.Bind<string>(v => { calls++; return TypedFixture.Text(v); });

        calls.Should().Be(1);
        Result<string, NotFoundError, ConflictError> expected = TypedFixture.Text(5);
        result.Should().Be(expected);
    }

    [Fact]
    public void Bind_Should_ReturnTheInnerError_When_TheInnerResultFails()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;

        Result<string, NotFoundError, ConflictError> result = source.Bind<string>(v => { calls++; return TypedFixture.Conflict; });

        calls.Should().Be(1);
        Result<string, NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public void Bind_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.NotFound;
        int calls = 0;

        Result<string, NotFoundError, ConflictError> result = source.Bind<string>(v => { calls++; return TypedFixture.Text(v); });

        calls.Should().Be(0);
        Result<string, NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void Bind_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;

        Result<string, NotFoundError, ConflictError> result = source.Bind<string>(v => { calls++; return TypedFixture.Text(v); });

        calls.Should().Be(0);
        Result<string, NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public void Bind_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;

        Action act = () => _ = source.Bind<string>(v => { calls++; return TypedFixture.Text(v); });

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public void BindWiden1_Should_ReturnTheInnerValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;

        Result<string, NotFoundError, ConflictError, ValidationError> result = source.BindWiden<string, ValidationError>(v => { calls++; return TypedFixture.Text(v); });

        calls.Should().Be(1);
        Result<string, NotFoundError, ConflictError, ValidationError> expected = TypedFixture.Text(5);
        result.Should().Be(expected);
    }

    [Fact]
    public void BindWiden1_Should_MoveTheInnerErrorToItsAddedSlot_When_TheInnerResultHoldsValidationError()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;

        Result<string, NotFoundError, ConflictError, ValidationError> result = source.BindWiden<string, ValidationError>(v => { calls++; return new Result<string, ValidationError>(TypedFixture.Invalid); });

        calls.Should().Be(1);
        Result<string, NotFoundError, ConflictError, ValidationError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public void BindWiden1_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.NotFound;
        int calls = 0;

        Result<string, NotFoundError, ConflictError, ValidationError> result = source.BindWiden<string, ValidationError>(v => { calls++; return TypedFixture.Text(v); });

        calls.Should().Be(0);
        Result<string, NotFoundError, ConflictError, ValidationError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void BindWiden1_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;

        Result<string, NotFoundError, ConflictError, ValidationError> result = source.BindWiden<string, ValidationError>(v => { calls++; return TypedFixture.Text(v); });

        calls.Should().Be(0);
        Result<string, NotFoundError, ConflictError, ValidationError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public void BindWiden1_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;

        Action act = () => _ = source.BindWiden<string, ValidationError>(v => { calls++; return TypedFixture.Text(v); });

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }
}
