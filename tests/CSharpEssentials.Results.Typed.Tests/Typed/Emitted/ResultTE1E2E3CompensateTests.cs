// Produced by scripts/generate-typed-results.py; do not edit.
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;

public sealed class ResultTE1E2E3CompensateTests
{
    [Fact]
    public void CompensateNotFoundError_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;

        Result<int, ConflictError, RateLimited> result = source.Compensate((NotFoundError e) => { calls++; return 9; });

        calls.Should().Be(0);
        Result<int, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public void CompensateNotFoundError_Should_ReturnTheCompensatingValue_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;

        Result<int, ConflictError, RateLimited> result = source.Compensate((NotFoundError e) => { calls++; return 9; });

        calls.Should().Be(1);
        Result<int, ConflictError, RateLimited> expected = 9;
        result.Should().Be(expected);
    }

    [Fact]
    public void CompensateNotFoundError_Should_ReturnTheCompensatingError_When_TheResultHoldsNotFoundErrorAndTheCompensationFails()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;

        Result<int, ConflictError, RateLimited> result = source.Compensate((NotFoundError e) => { calls++; return new Result<int, ConflictError, RateLimited>(TypedFixture.Limited); });

        calls.Should().Be(1);
        Result<int, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public void CompensateNotFoundError_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;

        Result<int, ConflictError, RateLimited> result = source.Compensate((NotFoundError e) => { calls++; return 9; });

        calls.Should().Be(0);
        Result<int, ConflictError, RateLimited> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public void CompensateNotFoundError_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;

        Result<int, ConflictError, RateLimited> result = source.Compensate((NotFoundError e) => { calls++; return 9; });

        calls.Should().Be(0);
        Result<int, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public void CompensateNotFoundError_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;

        Action act = () => _ = source.Compensate((NotFoundError e) => { calls++; return 9; });

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public void CompensateConflictError_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;

        Result<int, NotFoundError, RateLimited> result = source.Compensate((ConflictError e) => { calls++; return 9; });

        calls.Should().Be(0);
        Result<int, NotFoundError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public void CompensateConflictError_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;

        Result<int, NotFoundError, RateLimited> result = source.Compensate((ConflictError e) => { calls++; return 9; });

        calls.Should().Be(0);
        Result<int, NotFoundError, RateLimited> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void CompensateConflictError_Should_ReturnTheCompensatingValue_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;

        Result<int, NotFoundError, RateLimited> result = source.Compensate((ConflictError e) => { calls++; return 9; });

        calls.Should().Be(1);
        Result<int, NotFoundError, RateLimited> expected = 9;
        result.Should().Be(expected);
    }

    [Fact]
    public void CompensateConflictError_Should_ReturnTheCompensatingError_When_TheResultHoldsConflictErrorAndTheCompensationFails()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;

        Result<int, NotFoundError, RateLimited> result = source.Compensate((ConflictError e) => { calls++; return new Result<int, NotFoundError, RateLimited>(TypedFixture.Limited); });

        calls.Should().Be(1);
        Result<int, NotFoundError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public void CompensateConflictError_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;

        Result<int, NotFoundError, RateLimited> result = source.Compensate((ConflictError e) => { calls++; return 9; });

        calls.Should().Be(0);
        Result<int, NotFoundError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public void CompensateConflictError_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;

        Action act = () => _ = source.Compensate((ConflictError e) => { calls++; return 9; });

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public void CompensateRateLimited_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;

        Result<int, NotFoundError, ConflictError> result = source.Compensate((RateLimited e) => { calls++; return 9; });

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public void CompensateRateLimited_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;

        Result<int, NotFoundError, ConflictError> result = source.Compensate((RateLimited e) => { calls++; return 9; });

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void CompensateRateLimited_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;

        Result<int, NotFoundError, ConflictError> result = source.Compensate((RateLimited e) => { calls++; return 9; });

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public void CompensateRateLimited_Should_ReturnTheCompensatingValue_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;

        Result<int, NotFoundError, ConflictError> result = source.Compensate((RateLimited e) => { calls++; return 9; });

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError> expected = 9;
        result.Should().Be(expected);
    }

    [Fact]
    public void CompensateRateLimited_Should_ReturnTheCompensatingError_When_TheResultHoldsRateLimitedAndTheCompensationFails()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;

        Result<int, NotFoundError, ConflictError> result = source.Compensate((RateLimited e) => { calls++; return new Result<int, NotFoundError, ConflictError>(TypedFixture.Conflict); });

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public void CompensateRateLimited_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;

        Action act = () => _ = source.Compensate((RateLimited e) => { calls++; return 9; });

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }
}
