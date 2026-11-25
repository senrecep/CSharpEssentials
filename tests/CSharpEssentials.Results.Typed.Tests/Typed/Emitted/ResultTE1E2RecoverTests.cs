// Produced by scripts/generate-typed-results.py; do not edit.
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;

public sealed class ResultTE1E2RecoverTests
{
    [Fact]
    public void RecoverNotFoundError_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;

        Result<int, ConflictError> result = source.Recover((NotFoundError e) => { calls++; return 9; });

        calls.Should().Be(0);
        Result<int, ConflictError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public void RecoverNotFoundError_Should_ReturnTheRecoveredValue_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.NotFound;
        int calls = 0;

        Result<int, ConflictError> result = source.Recover((NotFoundError e) => { calls++; return 9; });

        calls.Should().Be(1);
        Result<int, ConflictError> expected = 9;
        result.Should().Be(expected);
    }

    [Fact]
    public void RecoverNotFoundError_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;

        Result<int, ConflictError> result = source.Recover((NotFoundError e) => { calls++; return 9; });

        calls.Should().Be(0);
        Result<int, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public void RecoverNotFoundError_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;

        Action act = () => _ = source.Recover((NotFoundError e) => { calls++; return 9; });

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public void RecoverConflictError_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;

        Result<int, NotFoundError> result = source.Recover((ConflictError e) => { calls++; return 9; });

        calls.Should().Be(0);
        Result<int, NotFoundError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public void RecoverConflictError_Should_MoveTheErrorToItsNewSlot_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.NotFound;
        int calls = 0;

        Result<int, NotFoundError> result = source.Recover((ConflictError e) => { calls++; return 9; });

        calls.Should().Be(0);
        Result<int, NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void RecoverConflictError_Should_ReturnTheRecoveredValue_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;

        Result<int, NotFoundError> result = source.Recover((ConflictError e) => { calls++; return 9; });

        calls.Should().Be(1);
        Result<int, NotFoundError> expected = 9;
        result.Should().Be(expected);
    }

    [Fact]
    public void RecoverConflictError_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;

        Action act = () => _ = source.Recover((ConflictError e) => { calls++; return 9; });

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public void Recover_Should_Throw_When_TheRecoveryReturnsNull()
    {
        Result<string, NotFoundError, ConflictError> source = TypedFixture.NotFound;
        int calls = 0;

        Action act = () => _ = source.Recover((NotFoundError e) => { calls++; return (string)null!; });

        act.Should().Throw<ArgumentNullException>();
        calls.Should().Be(1);
    }
}
