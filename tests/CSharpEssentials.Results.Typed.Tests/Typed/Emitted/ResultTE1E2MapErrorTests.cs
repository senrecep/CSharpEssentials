// Produced by scripts/generate-typed-results.py; do not edit.
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;

public sealed class ResultTE1E2MapErrorTests
{
    [Fact]
    public void MapErrorNotFoundError_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;

        Result<int, ValidationError, ConflictError> result = source.MapError((NotFoundError e) => { calls++; return TypedFixture.Invalid; });

        calls.Should().Be(0);
        Result<int, ValidationError, ConflictError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public void MapErrorNotFoundError_Should_MapTheError_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.NotFound;
        int calls = 0;

        Result<int, ValidationError, ConflictError> result = source.MapError((NotFoundError e) => { calls++; return TypedFixture.Invalid; });

        calls.Should().Be(1);
        Result<int, ValidationError, ConflictError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public void MapErrorNotFoundError_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;

        Result<int, ValidationError, ConflictError> result = source.MapError((NotFoundError e) => { calls++; return TypedFixture.Invalid; });

        calls.Should().Be(0);
        Result<int, ValidationError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public void MapErrorNotFoundError_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;

        Action act = () => _ = source.MapError((NotFoundError e) => { calls++; return TypedFixture.Invalid; });

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public void MapErrorConflictError_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;

        Result<int, NotFoundError, ValidationError> result = source.MapError((ConflictError e) => { calls++; return TypedFixture.Invalid; });

        calls.Should().Be(0);
        Result<int, NotFoundError, ValidationError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public void MapErrorConflictError_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.NotFound;
        int calls = 0;

        Result<int, NotFoundError, ValidationError> result = source.MapError((ConflictError e) => { calls++; return TypedFixture.Invalid; });

        calls.Should().Be(0);
        Result<int, NotFoundError, ValidationError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void MapErrorConflictError_Should_MapTheError_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;

        Result<int, NotFoundError, ValidationError> result = source.MapError((ConflictError e) => { calls++; return TypedFixture.Invalid; });

        calls.Should().Be(1);
        Result<int, NotFoundError, ValidationError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public void MapErrorConflictError_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;

        Action act = () => _ = source.MapError((ConflictError e) => { calls++; return TypedFixture.Invalid; });

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public void MapError_Should_Throw_When_TheMapReturnsNull()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.NotFound;
        int calls = 0;

        Action act = () => _ = source.MapError((NotFoundError e) => { calls++; return (ValidationError)null!; });

        act.Should().Throw<ArgumentNullException>();
        calls.Should().Be(1);
    }
}
