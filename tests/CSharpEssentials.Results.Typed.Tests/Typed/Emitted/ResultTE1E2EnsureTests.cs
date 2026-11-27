// Produced by scripts/generate-typed-results.py; do not edit.
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;

public sealed class ResultTE1E2EnsureTests
{
    [Fact]
    public void EnsureNotFoundError_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;

        Result<int, NotFoundError, ConflictError> result = source.Ensure(v => { calls++; return v > 0; }, TypedFixture.NotFound);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public void EnsureNotFoundError_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError> source = -5;
        int calls = 0;

        Result<int, NotFoundError, ConflictError> result = source.Ensure(v => { calls++; return v > 0; }, TypedFixture.NotFound);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void EnsureNotFoundError_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.NotFound;
        int calls = 0;

        Result<int, NotFoundError, ConflictError> result = source.Ensure(v => { calls++; return v > 0; }, TypedFixture.NotFound);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void EnsureNotFoundError_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;

        Result<int, NotFoundError, ConflictError> result = source.Ensure(v => { calls++; return v > 0; }, TypedFixture.NotFound);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public void EnsureNotFoundError_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;

        Action act = () => _ = source.Ensure(v => { calls++; return v > 0; }, TypedFixture.NotFound);

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public void EnsureNotFoundError_Should_Throw_When_TheErrorIsNullAndThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError> source = -5;
        int calls = 0;

        Action act = () => _ = source.Ensure(v => { calls++; return v > 0; }, (NotFoundError)null!);

        act.Should().Throw<ArgumentNullException>();
        calls.Should().Be(1);
    }

    [Fact]
    public void EnsureConflictError_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;

        Result<int, NotFoundError, ConflictError> result = source.Ensure(v => { calls++; return v > 0; }, TypedFixture.Conflict);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public void EnsureConflictError_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError> source = -5;
        int calls = 0;

        Result<int, NotFoundError, ConflictError> result = source.Ensure(v => { calls++; return v > 0; }, TypedFixture.Conflict);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public void EnsureConflictError_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.NotFound;
        int calls = 0;

        Result<int, NotFoundError, ConflictError> result = source.Ensure(v => { calls++; return v > 0; }, TypedFixture.Conflict);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void EnsureConflictError_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;

        Result<int, NotFoundError, ConflictError> result = source.Ensure(v => { calls++; return v > 0; }, TypedFixture.Conflict);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public void EnsureConflictError_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;

        Action act = () => _ = source.Ensure(v => { calls++; return v > 0; }, TypedFixture.Conflict);

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public void EnsureConflictError_Should_Throw_When_TheErrorIsNullAndThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError> source = -5;
        int calls = 0;

        Action act = () => _ = source.Ensure(v => { calls++; return v > 0; }, (ConflictError)null!);

        act.Should().Throw<ArgumentNullException>();
        calls.Should().Be(1);
    }

    [Fact]
    public void EnsureAdded_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;

        Result<int, NotFoundError, ConflictError, ValidationError> result = source.Ensure(v => { calls++; return v > 0; }, TypedFixture.Invalid);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, ValidationError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public void EnsureAdded_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError> source = -5;
        int calls = 0;

        Result<int, NotFoundError, ConflictError, ValidationError> result = source.Ensure(v => { calls++; return v > 0; }, TypedFixture.Invalid);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, ValidationError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public void EnsureAdded_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.NotFound;
        int calls = 0;

        Result<int, NotFoundError, ConflictError, ValidationError> result = source.Ensure(v => { calls++; return v > 0; }, TypedFixture.Invalid);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, ValidationError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void EnsureAdded_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;

        Result<int, NotFoundError, ConflictError, ValidationError> result = source.Ensure(v => { calls++; return v > 0; }, TypedFixture.Invalid);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, ValidationError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public void EnsureAdded_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;

        Action act = () => _ = source.Ensure(v => { calls++; return v > 0; }, TypedFixture.Invalid);

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public void EnsureAdded_Should_Throw_When_TheErrorIsNullAndThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError> source = -5;
        int calls = 0;

        Action act = () => _ = source.Ensure(v => { calls++; return v > 0; }, (ValidationError)null!);

        act.Should().Throw<ArgumentNullException>();
        calls.Should().Be(1);
    }

    [Fact]
    public void FailIfNotFoundError_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;

        Result<int, NotFoundError, ConflictError> result = source.FailIf(v => { calls++; return v < 0; }, TypedFixture.NotFound);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public void FailIfNotFoundError_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError> source = -5;
        int calls = 0;

        Result<int, NotFoundError, ConflictError> result = source.FailIf(v => { calls++; return v < 0; }, TypedFixture.NotFound);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void FailIfNotFoundError_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.NotFound;
        int calls = 0;

        Result<int, NotFoundError, ConflictError> result = source.FailIf(v => { calls++; return v < 0; }, TypedFixture.NotFound);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void FailIfNotFoundError_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;

        Result<int, NotFoundError, ConflictError> result = source.FailIf(v => { calls++; return v < 0; }, TypedFixture.NotFound);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public void FailIfNotFoundError_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;

        Action act = () => _ = source.FailIf(v => { calls++; return v < 0; }, TypedFixture.NotFound);

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public void FailIfNotFoundError_Should_Throw_When_TheErrorIsNullAndThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError> source = -5;
        int calls = 0;

        Action act = () => _ = source.FailIf(v => { calls++; return v < 0; }, (NotFoundError)null!);

        act.Should().Throw<ArgumentNullException>();
        calls.Should().Be(1);
    }

    [Fact]
    public void FailIfConflictError_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;

        Result<int, NotFoundError, ConflictError> result = source.FailIf(v => { calls++; return v < 0; }, TypedFixture.Conflict);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public void FailIfConflictError_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError> source = -5;
        int calls = 0;

        Result<int, NotFoundError, ConflictError> result = source.FailIf(v => { calls++; return v < 0; }, TypedFixture.Conflict);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public void FailIfConflictError_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.NotFound;
        int calls = 0;

        Result<int, NotFoundError, ConflictError> result = source.FailIf(v => { calls++; return v < 0; }, TypedFixture.Conflict);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void FailIfConflictError_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;

        Result<int, NotFoundError, ConflictError> result = source.FailIf(v => { calls++; return v < 0; }, TypedFixture.Conflict);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public void FailIfConflictError_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;

        Action act = () => _ = source.FailIf(v => { calls++; return v < 0; }, TypedFixture.Conflict);

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public void FailIfConflictError_Should_Throw_When_TheErrorIsNullAndThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError> source = -5;
        int calls = 0;

        Action act = () => _ = source.FailIf(v => { calls++; return v < 0; }, (ConflictError)null!);

        act.Should().Throw<ArgumentNullException>();
        calls.Should().Be(1);
    }

    [Fact]
    public void FailIfAdded_Should_KeepTheValue_When_ThePredicateAllowsIt()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;

        Result<int, NotFoundError, ConflictError, ValidationError> result = source.FailIf(v => { calls++; return v < 0; }, TypedFixture.Invalid);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, ValidationError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public void FailIfAdded_Should_ReturnTheError_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError> source = -5;
        int calls = 0;

        Result<int, NotFoundError, ConflictError, ValidationError> result = source.FailIf(v => { calls++; return v < 0; }, TypedFixture.Invalid);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, ValidationError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public void FailIfAdded_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.NotFound;
        int calls = 0;

        Result<int, NotFoundError, ConflictError, ValidationError> result = source.FailIf(v => { calls++; return v < 0; }, TypedFixture.Invalid);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, ValidationError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void FailIfAdded_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;

        Result<int, NotFoundError, ConflictError, ValidationError> result = source.FailIf(v => { calls++; return v < 0; }, TypedFixture.Invalid);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, ValidationError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public void FailIfAdded_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;

        Action act = () => _ = source.FailIf(v => { calls++; return v < 0; }, TypedFixture.Invalid);

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public void FailIfAdded_Should_Throw_When_TheErrorIsNullAndThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError, ConflictError> source = -5;
        int calls = 0;

        Action act = () => _ = source.FailIf(v => { calls++; return v < 0; }, (ValidationError)null!);

        act.Should().Throw<ArgumentNullException>();
        calls.Should().Be(1);
    }
}
