// Produced by scripts/generate-typed-results.py; do not edit.
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;

public sealed class ResultTE1E2E3CombineTests
{
    [Fact]
    public void Combine2_Should_ReturnEveryValue_When_EveryResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> first = 1;
        Result<int, NotFoundError, ConflictError, RateLimited> second = 2;

        Result<(int, int), NotFoundError, ConflictError, RateLimited> result = Result<int, NotFoundError, ConflictError, RateLimited>.Combine(first, second);

        Result<(int, int), NotFoundError, ConflictError, RateLimited> expected = (1, 2);
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine8_Should_ReturnEveryValue_When_EveryResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> first = 1;
        Result<int, NotFoundError, ConflictError, RateLimited> second = 2;
        Result<int, NotFoundError, ConflictError, RateLimited> third = 3;
        Result<int, NotFoundError, ConflictError, RateLimited> fourth = 4;
        Result<int, NotFoundError, ConflictError, RateLimited> fifth = 5;
        Result<int, NotFoundError, ConflictError, RateLimited> sixth = 6;
        Result<int, NotFoundError, ConflictError, RateLimited> seventh = 7;
        Result<int, NotFoundError, ConflictError, RateLimited> eighth = 8;

        Result<(int, int, int, int, int, int, int, int), NotFoundError, ConflictError, RateLimited> result = Result<int, NotFoundError, ConflictError, RateLimited>.Combine(first, second, third, fourth, fifth, sixth, seventh, eighth);

        Result<(int, int, int, int, int, int, int, int), NotFoundError, ConflictError, RateLimited> expected = (1, 2, 3, 4, 5, 6, 7, 8);
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine_Should_ReturnTheFirstErrorWithoutLookingFurther_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> first = 1;
        Result<int, NotFoundError, ConflictError, RateLimited> second = TypedFixture.NotFound;
        Result<int, NotFoundError, ConflictError, RateLimited> third = default;

        Result<(int, int, int), NotFoundError, ConflictError, RateLimited> result = Result<int, NotFoundError, ConflictError, RateLimited>.Combine(first, second, third);

        Result<(int, int, int), NotFoundError, ConflictError, RateLimited> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine_Should_ReturnTheFirstErrorWithoutLookingFurther_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> first = 1;
        Result<int, NotFoundError, ConflictError, RateLimited> second = TypedFixture.Conflict;
        Result<int, NotFoundError, ConflictError, RateLimited> third = default;

        Result<(int, int, int), NotFoundError, ConflictError, RateLimited> result = Result<int, NotFoundError, ConflictError, RateLimited>.Combine(first, second, third);

        Result<(int, int, int), NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine_Should_ReturnTheFirstErrorWithoutLookingFurther_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> first = 1;
        Result<int, NotFoundError, ConflictError, RateLimited> second = TypedFixture.Limited;
        Result<int, NotFoundError, ConflictError, RateLimited> third = default;

        Result<(int, int, int), NotFoundError, ConflictError, RateLimited> result = Result<int, NotFoundError, ConflictError, RateLimited>.Combine(first, second, third);

        Result<(int, int, int), NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine_Should_Throw_When_AResultBeforeTheFirstErrorIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> first = default;
        Result<int, NotFoundError, ConflictError, RateLimited> second = TypedFixture.NotFound;

        Action act = () => _ = Result<int, NotFoundError, ConflictError, RateLimited>.Combine(first, second);

        act.Should().Throw<InvalidOperationException>();
    }
}
