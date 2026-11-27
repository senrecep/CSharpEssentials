// Produced by scripts/generate-typed-results.py; do not edit.
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;

public sealed class ResultTE1E2E3RecoverTests
{
    [Fact]
    public void RecoverNotFoundError_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;

        Result<int, ConflictError, RateLimited> result = source.Recover((NotFoundError e) => { calls++; return 9; });

        calls.Should().Be(0);
        Result<int, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public void RecoverNotFoundError_Should_ReturnTheRecoveredValue_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;

        Result<int, ConflictError, RateLimited> result = source.Recover((NotFoundError e) => { calls++; return 9; });

        calls.Should().Be(1);
        Result<int, ConflictError, RateLimited> expected = 9;
        result.Should().Be(expected);
    }

    [Fact]
    public void RecoverNotFoundError_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;

        Result<int, ConflictError, RateLimited> result = source.Recover((NotFoundError e) => { calls++; return 9; });

        calls.Should().Be(0);
        Result<int, ConflictError, RateLimited> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public void RecoverNotFoundError_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;

        Result<int, ConflictError, RateLimited> result = source.Recover((NotFoundError e) => { calls++; return 9; });

        calls.Should().Be(0);
        Result<int, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public void RecoverNotFoundError_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;

        Action act = () => _ = source.Recover((NotFoundError e) => { calls++; return 9; });

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public void RecoverConflictError_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;

        Result<int, NotFoundError, RateLimited> result = source.Recover((ConflictError e) => { calls++; return 9; });

        calls.Should().Be(0);
        Result<int, NotFoundError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public void RecoverConflictError_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;

        Result<int, NotFoundError, RateLimited> result = source.Recover((ConflictError e) => { calls++; return 9; });

        calls.Should().Be(0);
        Result<int, NotFoundError, RateLimited> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void RecoverConflictError_Should_ReturnTheRecoveredValue_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;

        Result<int, NotFoundError, RateLimited> result = source.Recover((ConflictError e) => { calls++; return 9; });

        calls.Should().Be(1);
        Result<int, NotFoundError, RateLimited> expected = 9;
        result.Should().Be(expected);
    }

    [Fact]
    public void RecoverConflictError_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;

        Result<int, NotFoundError, RateLimited> result = source.Recover((ConflictError e) => { calls++; return 9; });

        calls.Should().Be(0);
        Result<int, NotFoundError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public void RecoverConflictError_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;

        Action act = () => _ = source.Recover((ConflictError e) => { calls++; return 9; });

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public void RecoverRateLimited_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;

        Result<int, NotFoundError, ConflictError> result = source.Recover((RateLimited e) => { calls++; return 9; });

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public void RecoverRateLimited_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;

        Result<int, NotFoundError, ConflictError> result = source.Recover((RateLimited e) => { calls++; return 9; });

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void RecoverRateLimited_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;

        Result<int, NotFoundError, ConflictError> result = source.Recover((RateLimited e) => { calls++; return 9; });

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public void RecoverRateLimited_Should_ReturnTheRecoveredValue_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;

        Result<int, NotFoundError, ConflictError> result = source.Recover((RateLimited e) => { calls++; return 9; });

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError> expected = 9;
        result.Should().Be(expected);
    }

    [Fact]
    public void RecoverRateLimited_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;

        Action act = () => _ = source.Recover((RateLimited e) => { calls++; return 9; });

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public void Recover_Should_Throw_When_TheRecoveryReturnsNull()
    {
        Result<string, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;

        Action act = () => _ = source.Recover((NotFoundError e) => { calls++; return (string)null!; });

        act.Should().Throw<ArgumentNullException>();
        calls.Should().Be(1);
    }
}
