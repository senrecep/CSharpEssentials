// Produced by scripts/generate-typed-results.py; do not edit.
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;

public sealed class ResultTE1E2CompensateTests
{
    [Fact]
    public void CompensateNotFoundError_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;

        Result<int, ConflictError> result = source.Compensate((NotFoundError e) => { calls++; return 9; });

        calls.Should().Be(0);
        Result<int, ConflictError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public void CompensateNotFoundError_Should_ReturnTheCompensatingValue_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.NotFound;
        int calls = 0;

        Result<int, ConflictError> result = source.Compensate((NotFoundError e) => { calls++; return 9; });

        calls.Should().Be(1);
        Result<int, ConflictError> expected = 9;
        result.Should().Be(expected);
    }

    [Fact]
    public void CompensateNotFoundError_Should_ReturnTheCompensatingError_When_TheResultHoldsNotFoundErrorAndTheCompensationFails()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.NotFound;
        int calls = 0;

        Result<int, ConflictError> result = source.Compensate((NotFoundError e) => { calls++; return new Result<int, ConflictError>(TypedFixture.Conflict); });

        calls.Should().Be(1);
        Result<int, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public void CompensateNotFoundError_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;

        Result<int, ConflictError> result = source.Compensate((NotFoundError e) => { calls++; return 9; });

        calls.Should().Be(0);
        Result<int, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public void CompensateNotFoundError_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;

        Action act = () => _ = source.Compensate((NotFoundError e) => { calls++; return 9; });

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public void CompensateConflictError_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;

        Result<int, NotFoundError> result = source.Compensate((ConflictError e) => { calls++; return 9; });

        calls.Should().Be(0);
        Result<int, NotFoundError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public void CompensateConflictError_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.NotFound;
        int calls = 0;

        Result<int, NotFoundError> result = source.Compensate((ConflictError e) => { calls++; return 9; });

        calls.Should().Be(0);
        Result<int, NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void CompensateConflictError_Should_ReturnTheCompensatingValue_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;

        Result<int, NotFoundError> result = source.Compensate((ConflictError e) => { calls++; return 9; });

        calls.Should().Be(1);
        Result<int, NotFoundError> expected = 9;
        result.Should().Be(expected);
    }

    [Fact]
    public void CompensateConflictError_Should_ReturnTheCompensatingError_When_TheResultHoldsConflictErrorAndTheCompensationFails()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;

        Result<int, NotFoundError> result = source.Compensate((ConflictError e) => { calls++; return new Result<int, NotFoundError>(TypedFixture.NotFound); });

        calls.Should().Be(1);
        Result<int, NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void CompensateConflictError_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;

        Action act = () => _ = source.Compensate((ConflictError e) => { calls++; return 9; });

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }
}
