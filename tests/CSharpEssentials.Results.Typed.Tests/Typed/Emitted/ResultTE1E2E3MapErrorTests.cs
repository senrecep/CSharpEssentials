// Produced by scripts/generate-typed-results.py; do not edit.
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;

public sealed class ResultTE1E2E3MapErrorTests
{
    [Fact]
    public void MapErrorNotFoundError_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;

        Result<int, ValidationError, ConflictError, RateLimited> result = source.MapError((NotFoundError e) => { calls++; return TypedFixture.Invalid; });

        calls.Should().Be(0);
        Result<int, ValidationError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public void MapErrorNotFoundError_Should_MapTheError_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;

        Result<int, ValidationError, ConflictError, RateLimited> result = source.MapError((NotFoundError e) => { calls++; return TypedFixture.Invalid; });

        calls.Should().Be(1);
        Result<int, ValidationError, ConflictError, RateLimited> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public void MapErrorNotFoundError_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;

        Result<int, ValidationError, ConflictError, RateLimited> result = source.MapError((NotFoundError e) => { calls++; return TypedFixture.Invalid; });

        calls.Should().Be(0);
        Result<int, ValidationError, ConflictError, RateLimited> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public void MapErrorNotFoundError_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;

        Result<int, ValidationError, ConflictError, RateLimited> result = source.MapError((NotFoundError e) => { calls++; return TypedFixture.Invalid; });

        calls.Should().Be(0);
        Result<int, ValidationError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public void MapErrorNotFoundError_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;

        Action act = () => _ = source.MapError((NotFoundError e) => { calls++; return TypedFixture.Invalid; });

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public void MapErrorConflictError_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;

        Result<int, NotFoundError, ValidationError, RateLimited> result = source.MapError((ConflictError e) => { calls++; return TypedFixture.Invalid; });

        calls.Should().Be(0);
        Result<int, NotFoundError, ValidationError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public void MapErrorConflictError_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;

        Result<int, NotFoundError, ValidationError, RateLimited> result = source.MapError((ConflictError e) => { calls++; return TypedFixture.Invalid; });

        calls.Should().Be(0);
        Result<int, NotFoundError, ValidationError, RateLimited> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void MapErrorConflictError_Should_MapTheError_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;

        Result<int, NotFoundError, ValidationError, RateLimited> result = source.MapError((ConflictError e) => { calls++; return TypedFixture.Invalid; });

        calls.Should().Be(1);
        Result<int, NotFoundError, ValidationError, RateLimited> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public void MapErrorConflictError_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;

        Result<int, NotFoundError, ValidationError, RateLimited> result = source.MapError((ConflictError e) => { calls++; return TypedFixture.Invalid; });

        calls.Should().Be(0);
        Result<int, NotFoundError, ValidationError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public void MapErrorConflictError_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;

        Action act = () => _ = source.MapError((ConflictError e) => { calls++; return TypedFixture.Invalid; });

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public void MapErrorRateLimited_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;

        Result<int, NotFoundError, ConflictError, ValidationError> result = source.MapError((RateLimited e) => { calls++; return TypedFixture.Invalid; });

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, ValidationError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public void MapErrorRateLimited_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;

        Result<int, NotFoundError, ConflictError, ValidationError> result = source.MapError((RateLimited e) => { calls++; return TypedFixture.Invalid; });

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, ValidationError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void MapErrorRateLimited_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;

        Result<int, NotFoundError, ConflictError, ValidationError> result = source.MapError((RateLimited e) => { calls++; return TypedFixture.Invalid; });

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError, ValidationError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public void MapErrorRateLimited_Should_MapTheError_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;

        Result<int, NotFoundError, ConflictError, ValidationError> result = source.MapError((RateLimited e) => { calls++; return TypedFixture.Invalid; });

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, ValidationError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public void MapErrorRateLimited_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;

        Action act = () => _ = source.MapError((RateLimited e) => { calls++; return TypedFixture.Invalid; });

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public void MapError_Should_Throw_When_TheMapReturnsNull()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;

        Action act = () => _ = source.MapError((NotFoundError e) => { calls++; return (ValidationError)null!; });

        act.Should().Throw<ArgumentNullException>();
        calls.Should().Be(1);
    }
}
