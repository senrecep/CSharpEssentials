// Produced by scripts/generate-typed-results.py; do not edit.
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;

public sealed class ResultTE1E2E3FinallyTests
{
    [Fact]
    public void Finally_Should_ReturnTheFunctionValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;

        bool result = source.Finally(r => { calls++; return r.IsSuccess; });

        calls.Should().Be(1);
        bool expected = true;
        result.Should().Be(expected);
    }

    [Fact]
    public void Finally_Should_ReturnTheFunctionValue_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;

        bool result = source.Finally(r => { calls++; return r.IsSuccess; });

        calls.Should().Be(1);
        bool expected = false;
        result.Should().Be(expected);
    }

    [Fact]
    public void Finally_Should_ReturnTheFunctionValue_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;

        bool result = source.Finally(r => { calls++; return r.IsSuccess; });

        calls.Should().Be(1);
        bool expected = false;
        result.Should().Be(expected);
    }

    [Fact]
    public void Finally_Should_ReturnTheFunctionValue_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;

        bool result = source.Finally(r => { calls++; return r.IsSuccess; });

        calls.Should().Be(1);
        bool expected = false;
        result.Should().Be(expected);
    }

    [Fact]
    public void Finally_Should_PassTheDefaultResult_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;

        bool result = source.Finally(r => { calls++; return r.HasValue; });

        calls.Should().Be(1);
        bool expected = false;
        result.Should().Be(expected);
    }

    [Fact]
    public void FinallyAction_Should_CallTheAction_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;

        Result<int, NotFoundError, ConflictError, RateLimited> result = source.Finally(_ => { calls++; });

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public void FinallyAction_Should_CallTheAction_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;

        Result<int, NotFoundError, ConflictError, RateLimited> result = source.Finally(_ => { calls++; });

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void FinallyAction_Should_CallTheAction_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;

        Result<int, NotFoundError, ConflictError, RateLimited> result = source.Finally(_ => { calls++; });

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public void FinallyAction_Should_CallTheAction_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;

        Result<int, NotFoundError, ConflictError, RateLimited> result = source.Finally(_ => { calls++; });

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public void FinallyAction_Should_CallTheAction_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;

        Result<int, NotFoundError, ConflictError, RateLimited> result = source.Finally(_ => { calls++; });

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError, RateLimited> expected = default;
        result.Should().Be(expected);
    }
}
