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
    public void Combine2_Should_ReturnTheErrorOfTheFirstResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError> first = TypedFixture.NotFound;
        Result<int, NotFoundError> second = default;

        Result<(int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second);

        Result<(int, int), NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine2_Should_ReturnTheErrorOfTheSecondResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = TypedFixture.NotFound;

        Result<(int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second);

        Result<(int, int), NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine2_Should_Throw_When_TheFirstResultIsDefaultBeforeAnError()
    {
        Result<int, NotFoundError> first = default;
        Result<int, NotFoundError> second = TypedFixture.NotFound;

        Action act = () => _ = Result<int, NotFoundError>.Combine(first, second);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine2_Should_Throw_When_TheLastResultIsDefault()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = default;

        Action act = () => _ = Result<int, NotFoundError>.Combine(first, second);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine3_Should_ReturnEveryValue_When_EveryResultHoldsAValue()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = 3;

        Result<(int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third);

        Result<(int, int, int), NotFoundError> expected = (1, 2, 3);
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine3_Should_ReturnTheErrorOfTheFirstResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError> first = TypedFixture.NotFound;
        Result<int, NotFoundError> second = default;
        Result<int, NotFoundError> third = default;

        Result<(int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third);

        Result<(int, int, int), NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine3_Should_ReturnTheErrorOfTheSecondResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = TypedFixture.NotFound;
        Result<int, NotFoundError> third = default;

        Result<(int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third);

        Result<(int, int, int), NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine3_Should_ReturnTheErrorOfTheThirdResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = TypedFixture.NotFound;

        Result<(int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third);

        Result<(int, int, int), NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine3_Should_Throw_When_TheFirstResultIsDefaultBeforeAnError()
    {
        Result<int, NotFoundError> first = default;
        Result<int, NotFoundError> second = TypedFixture.NotFound;
        Result<int, NotFoundError> third = default;

        Action act = () => _ = Result<int, NotFoundError>.Combine(first, second, third);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine3_Should_Throw_When_TheSecondResultIsDefaultBeforeAnError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = default;
        Result<int, NotFoundError> third = TypedFixture.NotFound;

        Action act = () => _ = Result<int, NotFoundError>.Combine(first, second, third);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine3_Should_Throw_When_TheLastResultIsDefault()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = default;

        Action act = () => _ = Result<int, NotFoundError>.Combine(first, second, third);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine4_Should_ReturnEveryValue_When_EveryResultHoldsAValue()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = 3;
        Result<int, NotFoundError> fourth = 4;

        Result<(int, int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third, fourth);

        Result<(int, int, int, int), NotFoundError> expected = (1, 2, 3, 4);
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine4_Should_ReturnTheErrorOfTheFirstResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError> first = TypedFixture.NotFound;
        Result<int, NotFoundError> second = default;
        Result<int, NotFoundError> third = default;
        Result<int, NotFoundError> fourth = default;

        Result<(int, int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third, fourth);

        Result<(int, int, int, int), NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine4_Should_ReturnTheErrorOfTheSecondResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = TypedFixture.NotFound;
        Result<int, NotFoundError> third = default;
        Result<int, NotFoundError> fourth = default;

        Result<(int, int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third, fourth);

        Result<(int, int, int, int), NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine4_Should_ReturnTheErrorOfTheThirdResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = TypedFixture.NotFound;
        Result<int, NotFoundError> fourth = default;

        Result<(int, int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third, fourth);

        Result<(int, int, int, int), NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine4_Should_ReturnTheErrorOfTheFourthResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = 3;
        Result<int, NotFoundError> fourth = TypedFixture.NotFound;

        Result<(int, int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third, fourth);

        Result<(int, int, int, int), NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine4_Should_Throw_When_TheFirstResultIsDefaultBeforeAnError()
    {
        Result<int, NotFoundError> first = default;
        Result<int, NotFoundError> second = TypedFixture.NotFound;
        Result<int, NotFoundError> third = default;
        Result<int, NotFoundError> fourth = default;

        Action act = () => _ = Result<int, NotFoundError>.Combine(first, second, third, fourth);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine4_Should_Throw_When_TheSecondResultIsDefaultBeforeAnError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = default;
        Result<int, NotFoundError> third = TypedFixture.NotFound;
        Result<int, NotFoundError> fourth = default;

        Action act = () => _ = Result<int, NotFoundError>.Combine(first, second, third, fourth);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine4_Should_Throw_When_TheThirdResultIsDefaultBeforeAnError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = default;
        Result<int, NotFoundError> fourth = TypedFixture.NotFound;

        Action act = () => _ = Result<int, NotFoundError>.Combine(first, second, third, fourth);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine4_Should_Throw_When_TheLastResultIsDefault()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = 3;
        Result<int, NotFoundError> fourth = default;

        Action act = () => _ = Result<int, NotFoundError>.Combine(first, second, third, fourth);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine5_Should_ReturnEveryValue_When_EveryResultHoldsAValue()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = 3;
        Result<int, NotFoundError> fourth = 4;
        Result<int, NotFoundError> fifth = 5;

        Result<(int, int, int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth);

        Result<(int, int, int, int, int), NotFoundError> expected = (1, 2, 3, 4, 5);
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine5_Should_ReturnTheErrorOfTheFirstResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError> first = TypedFixture.NotFound;
        Result<int, NotFoundError> second = default;
        Result<int, NotFoundError> third = default;
        Result<int, NotFoundError> fourth = default;
        Result<int, NotFoundError> fifth = default;

        Result<(int, int, int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth);

        Result<(int, int, int, int, int), NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine5_Should_ReturnTheErrorOfTheSecondResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = TypedFixture.NotFound;
        Result<int, NotFoundError> third = default;
        Result<int, NotFoundError> fourth = default;
        Result<int, NotFoundError> fifth = default;

        Result<(int, int, int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth);

        Result<(int, int, int, int, int), NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine5_Should_ReturnTheErrorOfTheThirdResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = TypedFixture.NotFound;
        Result<int, NotFoundError> fourth = default;
        Result<int, NotFoundError> fifth = default;

        Result<(int, int, int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth);

        Result<(int, int, int, int, int), NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine5_Should_ReturnTheErrorOfTheFourthResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = 3;
        Result<int, NotFoundError> fourth = TypedFixture.NotFound;
        Result<int, NotFoundError> fifth = default;

        Result<(int, int, int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth);

        Result<(int, int, int, int, int), NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine5_Should_ReturnTheErrorOfTheFifthResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = 3;
        Result<int, NotFoundError> fourth = 4;
        Result<int, NotFoundError> fifth = TypedFixture.NotFound;

        Result<(int, int, int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth);

        Result<(int, int, int, int, int), NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine5_Should_Throw_When_TheFirstResultIsDefaultBeforeAnError()
    {
        Result<int, NotFoundError> first = default;
        Result<int, NotFoundError> second = TypedFixture.NotFound;
        Result<int, NotFoundError> third = default;
        Result<int, NotFoundError> fourth = default;
        Result<int, NotFoundError> fifth = default;

        Action act = () => _ = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine5_Should_Throw_When_TheSecondResultIsDefaultBeforeAnError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = default;
        Result<int, NotFoundError> third = TypedFixture.NotFound;
        Result<int, NotFoundError> fourth = default;
        Result<int, NotFoundError> fifth = default;

        Action act = () => _ = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine5_Should_Throw_When_TheThirdResultIsDefaultBeforeAnError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = default;
        Result<int, NotFoundError> fourth = TypedFixture.NotFound;
        Result<int, NotFoundError> fifth = default;

        Action act = () => _ = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine5_Should_Throw_When_TheFourthResultIsDefaultBeforeAnError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = 3;
        Result<int, NotFoundError> fourth = default;
        Result<int, NotFoundError> fifth = TypedFixture.NotFound;

        Action act = () => _ = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine5_Should_Throw_When_TheLastResultIsDefault()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = 3;
        Result<int, NotFoundError> fourth = 4;
        Result<int, NotFoundError> fifth = default;

        Action act = () => _ = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine6_Should_ReturnEveryValue_When_EveryResultHoldsAValue()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = 3;
        Result<int, NotFoundError> fourth = 4;
        Result<int, NotFoundError> fifth = 5;
        Result<int, NotFoundError> sixth = 6;

        Result<(int, int, int, int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth);

        Result<(int, int, int, int, int, int), NotFoundError> expected = (1, 2, 3, 4, 5, 6);
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine6_Should_ReturnTheErrorOfTheFirstResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError> first = TypedFixture.NotFound;
        Result<int, NotFoundError> second = default;
        Result<int, NotFoundError> third = default;
        Result<int, NotFoundError> fourth = default;
        Result<int, NotFoundError> fifth = default;
        Result<int, NotFoundError> sixth = default;

        Result<(int, int, int, int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth);

        Result<(int, int, int, int, int, int), NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine6_Should_ReturnTheErrorOfTheSecondResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = TypedFixture.NotFound;
        Result<int, NotFoundError> third = default;
        Result<int, NotFoundError> fourth = default;
        Result<int, NotFoundError> fifth = default;
        Result<int, NotFoundError> sixth = default;

        Result<(int, int, int, int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth);

        Result<(int, int, int, int, int, int), NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine6_Should_ReturnTheErrorOfTheThirdResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = TypedFixture.NotFound;
        Result<int, NotFoundError> fourth = default;
        Result<int, NotFoundError> fifth = default;
        Result<int, NotFoundError> sixth = default;

        Result<(int, int, int, int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth);

        Result<(int, int, int, int, int, int), NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine6_Should_ReturnTheErrorOfTheFourthResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = 3;
        Result<int, NotFoundError> fourth = TypedFixture.NotFound;
        Result<int, NotFoundError> fifth = default;
        Result<int, NotFoundError> sixth = default;

        Result<(int, int, int, int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth);

        Result<(int, int, int, int, int, int), NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine6_Should_ReturnTheErrorOfTheFifthResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = 3;
        Result<int, NotFoundError> fourth = 4;
        Result<int, NotFoundError> fifth = TypedFixture.NotFound;
        Result<int, NotFoundError> sixth = default;

        Result<(int, int, int, int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth);

        Result<(int, int, int, int, int, int), NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine6_Should_ReturnTheErrorOfTheSixthResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = 3;
        Result<int, NotFoundError> fourth = 4;
        Result<int, NotFoundError> fifth = 5;
        Result<int, NotFoundError> sixth = TypedFixture.NotFound;

        Result<(int, int, int, int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth);

        Result<(int, int, int, int, int, int), NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine6_Should_Throw_When_TheFirstResultIsDefaultBeforeAnError()
    {
        Result<int, NotFoundError> first = default;
        Result<int, NotFoundError> second = TypedFixture.NotFound;
        Result<int, NotFoundError> third = default;
        Result<int, NotFoundError> fourth = default;
        Result<int, NotFoundError> fifth = default;
        Result<int, NotFoundError> sixth = default;

        Action act = () => _ = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine6_Should_Throw_When_TheSecondResultIsDefaultBeforeAnError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = default;
        Result<int, NotFoundError> third = TypedFixture.NotFound;
        Result<int, NotFoundError> fourth = default;
        Result<int, NotFoundError> fifth = default;
        Result<int, NotFoundError> sixth = default;

        Action act = () => _ = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine6_Should_Throw_When_TheThirdResultIsDefaultBeforeAnError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = default;
        Result<int, NotFoundError> fourth = TypedFixture.NotFound;
        Result<int, NotFoundError> fifth = default;
        Result<int, NotFoundError> sixth = default;

        Action act = () => _ = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine6_Should_Throw_When_TheFourthResultIsDefaultBeforeAnError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = 3;
        Result<int, NotFoundError> fourth = default;
        Result<int, NotFoundError> fifth = TypedFixture.NotFound;
        Result<int, NotFoundError> sixth = default;

        Action act = () => _ = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine6_Should_Throw_When_TheFifthResultIsDefaultBeforeAnError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = 3;
        Result<int, NotFoundError> fourth = 4;
        Result<int, NotFoundError> fifth = default;
        Result<int, NotFoundError> sixth = TypedFixture.NotFound;

        Action act = () => _ = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine6_Should_Throw_When_TheLastResultIsDefault()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = 3;
        Result<int, NotFoundError> fourth = 4;
        Result<int, NotFoundError> fifth = 5;
        Result<int, NotFoundError> sixth = default;

        Action act = () => _ = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine7_Should_ReturnEveryValue_When_EveryResultHoldsAValue()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = 3;
        Result<int, NotFoundError> fourth = 4;
        Result<int, NotFoundError> fifth = 5;
        Result<int, NotFoundError> sixth = 6;
        Result<int, NotFoundError> seventh = 7;

        Result<(int, int, int, int, int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth, seventh);

        Result<(int, int, int, int, int, int, int), NotFoundError> expected = (1, 2, 3, 4, 5, 6, 7);
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine7_Should_ReturnTheErrorOfTheFirstResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError> first = TypedFixture.NotFound;
        Result<int, NotFoundError> second = default;
        Result<int, NotFoundError> third = default;
        Result<int, NotFoundError> fourth = default;
        Result<int, NotFoundError> fifth = default;
        Result<int, NotFoundError> sixth = default;
        Result<int, NotFoundError> seventh = default;

        Result<(int, int, int, int, int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth, seventh);

        Result<(int, int, int, int, int, int, int), NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine7_Should_ReturnTheErrorOfTheSecondResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = TypedFixture.NotFound;
        Result<int, NotFoundError> third = default;
        Result<int, NotFoundError> fourth = default;
        Result<int, NotFoundError> fifth = default;
        Result<int, NotFoundError> sixth = default;
        Result<int, NotFoundError> seventh = default;

        Result<(int, int, int, int, int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth, seventh);

        Result<(int, int, int, int, int, int, int), NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine7_Should_ReturnTheErrorOfTheThirdResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = TypedFixture.NotFound;
        Result<int, NotFoundError> fourth = default;
        Result<int, NotFoundError> fifth = default;
        Result<int, NotFoundError> sixth = default;
        Result<int, NotFoundError> seventh = default;

        Result<(int, int, int, int, int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth, seventh);

        Result<(int, int, int, int, int, int, int), NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine7_Should_ReturnTheErrorOfTheFourthResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = 3;
        Result<int, NotFoundError> fourth = TypedFixture.NotFound;
        Result<int, NotFoundError> fifth = default;
        Result<int, NotFoundError> sixth = default;
        Result<int, NotFoundError> seventh = default;

        Result<(int, int, int, int, int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth, seventh);

        Result<(int, int, int, int, int, int, int), NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine7_Should_ReturnTheErrorOfTheFifthResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = 3;
        Result<int, NotFoundError> fourth = 4;
        Result<int, NotFoundError> fifth = TypedFixture.NotFound;
        Result<int, NotFoundError> sixth = default;
        Result<int, NotFoundError> seventh = default;

        Result<(int, int, int, int, int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth, seventh);

        Result<(int, int, int, int, int, int, int), NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine7_Should_ReturnTheErrorOfTheSixthResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = 3;
        Result<int, NotFoundError> fourth = 4;
        Result<int, NotFoundError> fifth = 5;
        Result<int, NotFoundError> sixth = TypedFixture.NotFound;
        Result<int, NotFoundError> seventh = default;

        Result<(int, int, int, int, int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth, seventh);

        Result<(int, int, int, int, int, int, int), NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine7_Should_ReturnTheErrorOfTheSeventhResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = 3;
        Result<int, NotFoundError> fourth = 4;
        Result<int, NotFoundError> fifth = 5;
        Result<int, NotFoundError> sixth = 6;
        Result<int, NotFoundError> seventh = TypedFixture.NotFound;

        Result<(int, int, int, int, int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth, seventh);

        Result<(int, int, int, int, int, int, int), NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine7_Should_Throw_When_TheFirstResultIsDefaultBeforeAnError()
    {
        Result<int, NotFoundError> first = default;
        Result<int, NotFoundError> second = TypedFixture.NotFound;
        Result<int, NotFoundError> third = default;
        Result<int, NotFoundError> fourth = default;
        Result<int, NotFoundError> fifth = default;
        Result<int, NotFoundError> sixth = default;
        Result<int, NotFoundError> seventh = default;

        Action act = () => _ = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth, seventh);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine7_Should_Throw_When_TheSecondResultIsDefaultBeforeAnError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = default;
        Result<int, NotFoundError> third = TypedFixture.NotFound;
        Result<int, NotFoundError> fourth = default;
        Result<int, NotFoundError> fifth = default;
        Result<int, NotFoundError> sixth = default;
        Result<int, NotFoundError> seventh = default;

        Action act = () => _ = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth, seventh);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine7_Should_Throw_When_TheThirdResultIsDefaultBeforeAnError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = default;
        Result<int, NotFoundError> fourth = TypedFixture.NotFound;
        Result<int, NotFoundError> fifth = default;
        Result<int, NotFoundError> sixth = default;
        Result<int, NotFoundError> seventh = default;

        Action act = () => _ = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth, seventh);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine7_Should_Throw_When_TheFourthResultIsDefaultBeforeAnError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = 3;
        Result<int, NotFoundError> fourth = default;
        Result<int, NotFoundError> fifth = TypedFixture.NotFound;
        Result<int, NotFoundError> sixth = default;
        Result<int, NotFoundError> seventh = default;

        Action act = () => _ = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth, seventh);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine7_Should_Throw_When_TheFifthResultIsDefaultBeforeAnError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = 3;
        Result<int, NotFoundError> fourth = 4;
        Result<int, NotFoundError> fifth = default;
        Result<int, NotFoundError> sixth = TypedFixture.NotFound;
        Result<int, NotFoundError> seventh = default;

        Action act = () => _ = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth, seventh);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine7_Should_Throw_When_TheSixthResultIsDefaultBeforeAnError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = 3;
        Result<int, NotFoundError> fourth = 4;
        Result<int, NotFoundError> fifth = 5;
        Result<int, NotFoundError> sixth = default;
        Result<int, NotFoundError> seventh = TypedFixture.NotFound;

        Action act = () => _ = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth, seventh);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine7_Should_Throw_When_TheLastResultIsDefault()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = 3;
        Result<int, NotFoundError> fourth = 4;
        Result<int, NotFoundError> fifth = 5;
        Result<int, NotFoundError> sixth = 6;
        Result<int, NotFoundError> seventh = default;

        Action act = () => _ = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth, seventh);

        act.Should().Throw<InvalidOperationException>();
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
    public void Combine8_Should_ReturnTheErrorOfTheFirstResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError> first = TypedFixture.NotFound;
        Result<int, NotFoundError> second = default;
        Result<int, NotFoundError> third = default;
        Result<int, NotFoundError> fourth = default;
        Result<int, NotFoundError> fifth = default;
        Result<int, NotFoundError> sixth = default;
        Result<int, NotFoundError> seventh = default;
        Result<int, NotFoundError> eighth = default;

        Result<(int, int, int, int, int, int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth, seventh, eighth);

        Result<(int, int, int, int, int, int, int, int), NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine8_Should_ReturnTheErrorOfTheSecondResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = TypedFixture.NotFound;
        Result<int, NotFoundError> third = default;
        Result<int, NotFoundError> fourth = default;
        Result<int, NotFoundError> fifth = default;
        Result<int, NotFoundError> sixth = default;
        Result<int, NotFoundError> seventh = default;
        Result<int, NotFoundError> eighth = default;

        Result<(int, int, int, int, int, int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth, seventh, eighth);

        Result<(int, int, int, int, int, int, int, int), NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine8_Should_ReturnTheErrorOfTheThirdResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = TypedFixture.NotFound;
        Result<int, NotFoundError> fourth = default;
        Result<int, NotFoundError> fifth = default;
        Result<int, NotFoundError> sixth = default;
        Result<int, NotFoundError> seventh = default;
        Result<int, NotFoundError> eighth = default;

        Result<(int, int, int, int, int, int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth, seventh, eighth);

        Result<(int, int, int, int, int, int, int, int), NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine8_Should_ReturnTheErrorOfTheFourthResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = 3;
        Result<int, NotFoundError> fourth = TypedFixture.NotFound;
        Result<int, NotFoundError> fifth = default;
        Result<int, NotFoundError> sixth = default;
        Result<int, NotFoundError> seventh = default;
        Result<int, NotFoundError> eighth = default;

        Result<(int, int, int, int, int, int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth, seventh, eighth);

        Result<(int, int, int, int, int, int, int, int), NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine8_Should_ReturnTheErrorOfTheFifthResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = 3;
        Result<int, NotFoundError> fourth = 4;
        Result<int, NotFoundError> fifth = TypedFixture.NotFound;
        Result<int, NotFoundError> sixth = default;
        Result<int, NotFoundError> seventh = default;
        Result<int, NotFoundError> eighth = default;

        Result<(int, int, int, int, int, int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth, seventh, eighth);

        Result<(int, int, int, int, int, int, int, int), NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine8_Should_ReturnTheErrorOfTheSixthResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = 3;
        Result<int, NotFoundError> fourth = 4;
        Result<int, NotFoundError> fifth = 5;
        Result<int, NotFoundError> sixth = TypedFixture.NotFound;
        Result<int, NotFoundError> seventh = default;
        Result<int, NotFoundError> eighth = default;

        Result<(int, int, int, int, int, int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth, seventh, eighth);

        Result<(int, int, int, int, int, int, int, int), NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine8_Should_ReturnTheErrorOfTheSeventhResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = 3;
        Result<int, NotFoundError> fourth = 4;
        Result<int, NotFoundError> fifth = 5;
        Result<int, NotFoundError> sixth = 6;
        Result<int, NotFoundError> seventh = TypedFixture.NotFound;
        Result<int, NotFoundError> eighth = default;

        Result<(int, int, int, int, int, int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth, seventh, eighth);

        Result<(int, int, int, int, int, int, int, int), NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine8_Should_ReturnTheErrorOfTheEighthResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = 3;
        Result<int, NotFoundError> fourth = 4;
        Result<int, NotFoundError> fifth = 5;
        Result<int, NotFoundError> sixth = 6;
        Result<int, NotFoundError> seventh = 7;
        Result<int, NotFoundError> eighth = TypedFixture.NotFound;

        Result<(int, int, int, int, int, int, int, int), NotFoundError> result = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth, seventh, eighth);

        Result<(int, int, int, int, int, int, int, int), NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine8_Should_Throw_When_TheFirstResultIsDefaultBeforeAnError()
    {
        Result<int, NotFoundError> first = default;
        Result<int, NotFoundError> second = TypedFixture.NotFound;
        Result<int, NotFoundError> third = default;
        Result<int, NotFoundError> fourth = default;
        Result<int, NotFoundError> fifth = default;
        Result<int, NotFoundError> sixth = default;
        Result<int, NotFoundError> seventh = default;
        Result<int, NotFoundError> eighth = default;

        Action act = () => _ = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth, seventh, eighth);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine8_Should_Throw_When_TheSecondResultIsDefaultBeforeAnError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = default;
        Result<int, NotFoundError> third = TypedFixture.NotFound;
        Result<int, NotFoundError> fourth = default;
        Result<int, NotFoundError> fifth = default;
        Result<int, NotFoundError> sixth = default;
        Result<int, NotFoundError> seventh = default;
        Result<int, NotFoundError> eighth = default;

        Action act = () => _ = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth, seventh, eighth);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine8_Should_Throw_When_TheThirdResultIsDefaultBeforeAnError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = default;
        Result<int, NotFoundError> fourth = TypedFixture.NotFound;
        Result<int, NotFoundError> fifth = default;
        Result<int, NotFoundError> sixth = default;
        Result<int, NotFoundError> seventh = default;
        Result<int, NotFoundError> eighth = default;

        Action act = () => _ = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth, seventh, eighth);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine8_Should_Throw_When_TheFourthResultIsDefaultBeforeAnError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = 3;
        Result<int, NotFoundError> fourth = default;
        Result<int, NotFoundError> fifth = TypedFixture.NotFound;
        Result<int, NotFoundError> sixth = default;
        Result<int, NotFoundError> seventh = default;
        Result<int, NotFoundError> eighth = default;

        Action act = () => _ = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth, seventh, eighth);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine8_Should_Throw_When_TheFifthResultIsDefaultBeforeAnError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = 3;
        Result<int, NotFoundError> fourth = 4;
        Result<int, NotFoundError> fifth = default;
        Result<int, NotFoundError> sixth = TypedFixture.NotFound;
        Result<int, NotFoundError> seventh = default;
        Result<int, NotFoundError> eighth = default;

        Action act = () => _ = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth, seventh, eighth);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine8_Should_Throw_When_TheSixthResultIsDefaultBeforeAnError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = 3;
        Result<int, NotFoundError> fourth = 4;
        Result<int, NotFoundError> fifth = 5;
        Result<int, NotFoundError> sixth = default;
        Result<int, NotFoundError> seventh = TypedFixture.NotFound;
        Result<int, NotFoundError> eighth = default;

        Action act = () => _ = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth, seventh, eighth);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine8_Should_Throw_When_TheSeventhResultIsDefaultBeforeAnError()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = 3;
        Result<int, NotFoundError> fourth = 4;
        Result<int, NotFoundError> fifth = 5;
        Result<int, NotFoundError> sixth = 6;
        Result<int, NotFoundError> seventh = default;
        Result<int, NotFoundError> eighth = TypedFixture.NotFound;

        Action act = () => _ = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth, seventh, eighth);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine8_Should_Throw_When_TheLastResultIsDefault()
    {
        Result<int, NotFoundError> first = 1;
        Result<int, NotFoundError> second = 2;
        Result<int, NotFoundError> third = 3;
        Result<int, NotFoundError> fourth = 4;
        Result<int, NotFoundError> fifth = 5;
        Result<int, NotFoundError> sixth = 6;
        Result<int, NotFoundError> seventh = 7;
        Result<int, NotFoundError> eighth = default;

        Action act = () => _ = Result<int, NotFoundError>.Combine(first, second, third, fourth, fifth, sixth, seventh, eighth);

        act.Should().Throw<InvalidOperationException>();
    }
}
