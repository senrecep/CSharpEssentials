// Produced by scripts/generate-typed-results.py; do not edit.
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;

public sealed class ResultTE1E2CombineTests
{
    [Fact]
    public void Combine2_Should_ReturnEveryValue_When_EveryResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = 2;

        Result<(int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second);

        Result<(int, int), NotFoundError, ConflictError> expected = (1, 2);
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine8_Should_ReturnEveryValue_When_EveryResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = 2;
        Result<int, NotFoundError, ConflictError> third = 3;
        Result<int, NotFoundError, ConflictError> fourth = 4;
        Result<int, NotFoundError, ConflictError> fifth = 5;
        Result<int, NotFoundError, ConflictError> sixth = 6;
        Result<int, NotFoundError, ConflictError> seventh = 7;
        Result<int, NotFoundError, ConflictError> eighth = 8;

        Result<(int, int, int, int, int, int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth, fifth, sixth, seventh, eighth);

        Result<(int, int, int, int, int, int, int, int), NotFoundError, ConflictError> expected = (1, 2, 3, 4, 5, 6, 7, 8);
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine_Should_ReturnTheFirstErrorWithoutLookingFurther_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = TypedFixture.NotFound;
        Result<int, NotFoundError, ConflictError> third = default;

        Result<(int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third);

        Result<(int, int, int), NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine_Should_ReturnTheFirstErrorWithoutLookingFurther_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = TypedFixture.Conflict;
        Result<int, NotFoundError, ConflictError> third = default;

        Result<(int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third);

        Result<(int, int, int), NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine_Should_Throw_When_AResultBeforeTheFirstErrorIsDefault()
    {
        Result<int, NotFoundError, ConflictError> first = default;
        Result<int, NotFoundError, ConflictError> second = TypedFixture.NotFound;

        Action act = () => _ = Result<int, NotFoundError, ConflictError>.Combine(first, second);

        act.Should().Throw<InvalidOperationException>();
    }
}
