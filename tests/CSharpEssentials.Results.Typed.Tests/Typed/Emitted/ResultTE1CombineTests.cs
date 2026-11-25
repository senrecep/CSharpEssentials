// Produced by scripts/generate-typed-results.py; do not edit.
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;

public sealed class ResultTE1CombineTests
{
    [Fact]
    public void Combine2_Should_ReturnEveryValue_When_EveryResultHoldsAValue()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;

        Result<(int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second);

        Result<(int, int), NotFoundError> expected = (1, 2);
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine8_Should_ReturnEveryValue_When_EveryResultHoldsAValue()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = 3;
        Result<int, NotFoundError> fourth = 4;
        Result<int, NotFoundError> fifth = 5;
        Result<int, NotFoundError> sixth = 6;
        Result<int, NotFoundError> seventh = 7;
        Result<int, NotFoundError> eighth = 8;

        Result<(int, int, int, int, int, int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth, seventh, eighth);

        Result<(int, int, int, int, int, int, int, int), NotFoundError> expected = (1, 2, 3, 4, 5, 6, 7, 8);
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine_Should_ReturnTheFirstErrorWithoutLookingFurther_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = TypedFixture.NotFound;
        Result<int, NotFoundError> third = default;

        Result<(int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third);

        Result<(int, int, int), NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine_Should_Throw_When_AResultBeforeTheFirstErrorIsDefault()
    {
        Result<int, NotFoundError> first = default;
        Result<int, NotFoundError> second = TypedFixture.NotFound;

        Action act = () => _ = Result<int, NotFoundError>.Combine(first, second);

        act.Should().Throw<InvalidOperationException>();
    }
}
