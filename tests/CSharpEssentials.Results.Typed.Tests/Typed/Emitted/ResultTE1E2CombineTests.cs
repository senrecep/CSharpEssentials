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
    public void Combine2_Should_ReturnTheErrorOfTheFirstResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> first = TypedFixture.NotFound;
        Result<int, NotFoundError, ConflictError> second = default;

        Result<(int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second);

        Result<(int, int), NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine2_Should_ReturnTheErrorOfTheSecondResult_When_ItHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = TypedFixture.Conflict;

        Result<(int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second);

        Result<(int, int), NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine2_Should_Throw_When_AResultBeforeTheFirstErrorIsDefault()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = default;

        Action act = () => _ = Result<int, NotFoundError, ConflictError>.Combine(first, second);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine3_Should_ReturnEveryValue_When_EveryResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = 2;
        Result<int, NotFoundError, ConflictError> third = 3;

        Result<(int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third);

        Result<(int, int, int), NotFoundError, ConflictError> expected = (1, 2, 3);
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine3_Should_ReturnTheErrorOfTheFirstResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> first = TypedFixture.NotFound;
        Result<int, NotFoundError, ConflictError> second = default;
        Result<int, NotFoundError, ConflictError> third = default;

        Result<(int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third);

        Result<(int, int, int), NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine3_Should_ReturnTheErrorOfTheSecondResult_When_ItHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = TypedFixture.Conflict;
        Result<int, NotFoundError, ConflictError> third = default;

        Result<(int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third);

        Result<(int, int, int), NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine3_Should_ReturnTheErrorOfTheThirdResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = 2;
        Result<int, NotFoundError, ConflictError> third = TypedFixture.NotFound;

        Result<(int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third);

        Result<(int, int, int), NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine3_Should_Throw_When_AResultBeforeTheFirstErrorIsDefault()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = 2;
        Result<int, NotFoundError, ConflictError> third = default;

        Action act = () => _ = Result<int, NotFoundError, ConflictError>.Combine(first, second, third);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine4_Should_ReturnEveryValue_When_EveryResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = 2;
        Result<int, NotFoundError, ConflictError> third = 3;
        Result<int, NotFoundError, ConflictError> fourth = 4;

        Result<(int, int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth);

        Result<(int, int, int, int), NotFoundError, ConflictError> expected = (1, 2, 3, 4);
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine4_Should_ReturnTheErrorOfTheFirstResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> first = TypedFixture.NotFound;
        Result<int, NotFoundError, ConflictError> second = default;
        Result<int, NotFoundError, ConflictError> third = default;
        Result<int, NotFoundError, ConflictError> fourth = default;

        Result<(int, int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth);

        Result<(int, int, int, int), NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine4_Should_ReturnTheErrorOfTheSecondResult_When_ItHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = TypedFixture.Conflict;
        Result<int, NotFoundError, ConflictError> third = default;
        Result<int, NotFoundError, ConflictError> fourth = default;

        Result<(int, int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth);

        Result<(int, int, int, int), NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine4_Should_ReturnTheErrorOfTheThirdResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = 2;
        Result<int, NotFoundError, ConflictError> third = TypedFixture.NotFound;
        Result<int, NotFoundError, ConflictError> fourth = default;

        Result<(int, int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth);

        Result<(int, int, int, int), NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine4_Should_ReturnTheErrorOfTheFourthResult_When_ItHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = 2;
        Result<int, NotFoundError, ConflictError> third = 3;
        Result<int, NotFoundError, ConflictError> fourth = TypedFixture.Conflict;

        Result<(int, int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth);

        Result<(int, int, int, int), NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine4_Should_Throw_When_AResultBeforeTheFirstErrorIsDefault()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = 2;
        Result<int, NotFoundError, ConflictError> third = 3;
        Result<int, NotFoundError, ConflictError> fourth = default;

        Action act = () => _ = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine5_Should_ReturnEveryValue_When_EveryResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = 2;
        Result<int, NotFoundError, ConflictError> third = 3;
        Result<int, NotFoundError, ConflictError> fourth = 4;
        Result<int, NotFoundError, ConflictError> fifth = 5;

        Result<(int, int, int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth, fifth);

        Result<(int, int, int, int, int), NotFoundError, ConflictError> expected = (1, 2, 3, 4, 5);
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine5_Should_ReturnTheErrorOfTheFirstResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> first = TypedFixture.NotFound;
        Result<int, NotFoundError, ConflictError> second = default;
        Result<int, NotFoundError, ConflictError> third = default;
        Result<int, NotFoundError, ConflictError> fourth = default;
        Result<int, NotFoundError, ConflictError> fifth = default;

        Result<(int, int, int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth, fifth);

        Result<(int, int, int, int, int), NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine5_Should_ReturnTheErrorOfTheSecondResult_When_ItHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = TypedFixture.Conflict;
        Result<int, NotFoundError, ConflictError> third = default;
        Result<int, NotFoundError, ConflictError> fourth = default;
        Result<int, NotFoundError, ConflictError> fifth = default;

        Result<(int, int, int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth, fifth);

        Result<(int, int, int, int, int), NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine5_Should_ReturnTheErrorOfTheThirdResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = 2;
        Result<int, NotFoundError, ConflictError> third = TypedFixture.NotFound;
        Result<int, NotFoundError, ConflictError> fourth = default;
        Result<int, NotFoundError, ConflictError> fifth = default;

        Result<(int, int, int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth, fifth);

        Result<(int, int, int, int, int), NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine5_Should_ReturnTheErrorOfTheFourthResult_When_ItHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = 2;
        Result<int, NotFoundError, ConflictError> third = 3;
        Result<int, NotFoundError, ConflictError> fourth = TypedFixture.Conflict;
        Result<int, NotFoundError, ConflictError> fifth = default;

        Result<(int, int, int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth, fifth);

        Result<(int, int, int, int, int), NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine5_Should_ReturnTheErrorOfTheFifthResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = 2;
        Result<int, NotFoundError, ConflictError> third = 3;
        Result<int, NotFoundError, ConflictError> fourth = 4;
        Result<int, NotFoundError, ConflictError> fifth = TypedFixture.NotFound;

        Result<(int, int, int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth, fifth);

        Result<(int, int, int, int, int), NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine5_Should_Throw_When_AResultBeforeTheFirstErrorIsDefault()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = 2;
        Result<int, NotFoundError, ConflictError> third = 3;
        Result<int, NotFoundError, ConflictError> fourth = 4;
        Result<int, NotFoundError, ConflictError> fifth = default;

        Action act = () => _ = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth, fifth);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine6_Should_ReturnEveryValue_When_EveryResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = 2;
        Result<int, NotFoundError, ConflictError> third = 3;
        Result<int, NotFoundError, ConflictError> fourth = 4;
        Result<int, NotFoundError, ConflictError> fifth = 5;
        Result<int, NotFoundError, ConflictError> sixth = 6;

        Result<(int, int, int, int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth, fifth, sixth);

        Result<(int, int, int, int, int, int), NotFoundError, ConflictError> expected = (1, 2, 3, 4, 5, 6);
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine6_Should_ReturnTheErrorOfTheFirstResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> first = TypedFixture.NotFound;
        Result<int, NotFoundError, ConflictError> second = default;
        Result<int, NotFoundError, ConflictError> third = default;
        Result<int, NotFoundError, ConflictError> fourth = default;
        Result<int, NotFoundError, ConflictError> fifth = default;
        Result<int, NotFoundError, ConflictError> sixth = default;

        Result<(int, int, int, int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth, fifth, sixth);

        Result<(int, int, int, int, int, int), NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine6_Should_ReturnTheErrorOfTheSecondResult_When_ItHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = TypedFixture.Conflict;
        Result<int, NotFoundError, ConflictError> third = default;
        Result<int, NotFoundError, ConflictError> fourth = default;
        Result<int, NotFoundError, ConflictError> fifth = default;
        Result<int, NotFoundError, ConflictError> sixth = default;

        Result<(int, int, int, int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth, fifth, sixth);

        Result<(int, int, int, int, int, int), NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine6_Should_ReturnTheErrorOfTheThirdResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = 2;
        Result<int, NotFoundError, ConflictError> third = TypedFixture.NotFound;
        Result<int, NotFoundError, ConflictError> fourth = default;
        Result<int, NotFoundError, ConflictError> fifth = default;
        Result<int, NotFoundError, ConflictError> sixth = default;

        Result<(int, int, int, int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth, fifth, sixth);

        Result<(int, int, int, int, int, int), NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine6_Should_ReturnTheErrorOfTheFourthResult_When_ItHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = 2;
        Result<int, NotFoundError, ConflictError> third = 3;
        Result<int, NotFoundError, ConflictError> fourth = TypedFixture.Conflict;
        Result<int, NotFoundError, ConflictError> fifth = default;
        Result<int, NotFoundError, ConflictError> sixth = default;

        Result<(int, int, int, int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth, fifth, sixth);

        Result<(int, int, int, int, int, int), NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine6_Should_ReturnTheErrorOfTheFifthResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = 2;
        Result<int, NotFoundError, ConflictError> third = 3;
        Result<int, NotFoundError, ConflictError> fourth = 4;
        Result<int, NotFoundError, ConflictError> fifth = TypedFixture.NotFound;
        Result<int, NotFoundError, ConflictError> sixth = default;

        Result<(int, int, int, int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth, fifth, sixth);

        Result<(int, int, int, int, int, int), NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine6_Should_ReturnTheErrorOfTheSixthResult_When_ItHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = 2;
        Result<int, NotFoundError, ConflictError> third = 3;
        Result<int, NotFoundError, ConflictError> fourth = 4;
        Result<int, NotFoundError, ConflictError> fifth = 5;
        Result<int, NotFoundError, ConflictError> sixth = TypedFixture.Conflict;

        Result<(int, int, int, int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth, fifth, sixth);

        Result<(int, int, int, int, int, int), NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine6_Should_Throw_When_AResultBeforeTheFirstErrorIsDefault()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = 2;
        Result<int, NotFoundError, ConflictError> third = 3;
        Result<int, NotFoundError, ConflictError> fourth = 4;
        Result<int, NotFoundError, ConflictError> fifth = 5;
        Result<int, NotFoundError, ConflictError> sixth = default;

        Action act = () => _ = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth, fifth, sixth);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Combine7_Should_ReturnEveryValue_When_EveryResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = 2;
        Result<int, NotFoundError, ConflictError> third = 3;
        Result<int, NotFoundError, ConflictError> fourth = 4;
        Result<int, NotFoundError, ConflictError> fifth = 5;
        Result<int, NotFoundError, ConflictError> sixth = 6;
        Result<int, NotFoundError, ConflictError> seventh = 7;

        Result<(int, int, int, int, int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth, fifth, sixth, seventh);

        Result<(int, int, int, int, int, int, int), NotFoundError, ConflictError> expected = (1, 2, 3, 4, 5, 6, 7);
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine7_Should_ReturnTheErrorOfTheFirstResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> first = TypedFixture.NotFound;
        Result<int, NotFoundError, ConflictError> second = default;
        Result<int, NotFoundError, ConflictError> third = default;
        Result<int, NotFoundError, ConflictError> fourth = default;
        Result<int, NotFoundError, ConflictError> fifth = default;
        Result<int, NotFoundError, ConflictError> sixth = default;
        Result<int, NotFoundError, ConflictError> seventh = default;

        Result<(int, int, int, int, int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth, fifth, sixth, seventh);

        Result<(int, int, int, int, int, int, int), NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine7_Should_ReturnTheErrorOfTheSecondResult_When_ItHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = TypedFixture.Conflict;
        Result<int, NotFoundError, ConflictError> third = default;
        Result<int, NotFoundError, ConflictError> fourth = default;
        Result<int, NotFoundError, ConflictError> fifth = default;
        Result<int, NotFoundError, ConflictError> sixth = default;
        Result<int, NotFoundError, ConflictError> seventh = default;

        Result<(int, int, int, int, int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth, fifth, sixth, seventh);

        Result<(int, int, int, int, int, int, int), NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine7_Should_ReturnTheErrorOfTheThirdResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = 2;
        Result<int, NotFoundError, ConflictError> third = TypedFixture.NotFound;
        Result<int, NotFoundError, ConflictError> fourth = default;
        Result<int, NotFoundError, ConflictError> fifth = default;
        Result<int, NotFoundError, ConflictError> sixth = default;
        Result<int, NotFoundError, ConflictError> seventh = default;

        Result<(int, int, int, int, int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth, fifth, sixth, seventh);

        Result<(int, int, int, int, int, int, int), NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine7_Should_ReturnTheErrorOfTheFourthResult_When_ItHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = 2;
        Result<int, NotFoundError, ConflictError> third = 3;
        Result<int, NotFoundError, ConflictError> fourth = TypedFixture.Conflict;
        Result<int, NotFoundError, ConflictError> fifth = default;
        Result<int, NotFoundError, ConflictError> sixth = default;
        Result<int, NotFoundError, ConflictError> seventh = default;

        Result<(int, int, int, int, int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth, fifth, sixth, seventh);

        Result<(int, int, int, int, int, int, int), NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine7_Should_ReturnTheErrorOfTheFifthResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = 2;
        Result<int, NotFoundError, ConflictError> third = 3;
        Result<int, NotFoundError, ConflictError> fourth = 4;
        Result<int, NotFoundError, ConflictError> fifth = TypedFixture.NotFound;
        Result<int, NotFoundError, ConflictError> sixth = default;
        Result<int, NotFoundError, ConflictError> seventh = default;

        Result<(int, int, int, int, int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth, fifth, sixth, seventh);

        Result<(int, int, int, int, int, int, int), NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine7_Should_ReturnTheErrorOfTheSixthResult_When_ItHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = 2;
        Result<int, NotFoundError, ConflictError> third = 3;
        Result<int, NotFoundError, ConflictError> fourth = 4;
        Result<int, NotFoundError, ConflictError> fifth = 5;
        Result<int, NotFoundError, ConflictError> sixth = TypedFixture.Conflict;
        Result<int, NotFoundError, ConflictError> seventh = default;

        Result<(int, int, int, int, int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth, fifth, sixth, seventh);

        Result<(int, int, int, int, int, int, int), NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine7_Should_ReturnTheErrorOfTheSeventhResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = 2;
        Result<int, NotFoundError, ConflictError> third = 3;
        Result<int, NotFoundError, ConflictError> fourth = 4;
        Result<int, NotFoundError, ConflictError> fifth = 5;
        Result<int, NotFoundError, ConflictError> sixth = 6;
        Result<int, NotFoundError, ConflictError> seventh = TypedFixture.NotFound;

        Result<(int, int, int, int, int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth, fifth, sixth, seventh);

        Result<(int, int, int, int, int, int, int), NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine7_Should_Throw_When_AResultBeforeTheFirstErrorIsDefault()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = 2;
        Result<int, NotFoundError, ConflictError> third = 3;
        Result<int, NotFoundError, ConflictError> fourth = 4;
        Result<int, NotFoundError, ConflictError> fifth = 5;
        Result<int, NotFoundError, ConflictError> sixth = 6;
        Result<int, NotFoundError, ConflictError> seventh = default;

        Action act = () => _ = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth, fifth, sixth, seventh);

        act.Should().Throw<InvalidOperationException>();
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
    public void Combine8_Should_ReturnTheErrorOfTheFirstResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> first = TypedFixture.NotFound;
        Result<int, NotFoundError, ConflictError> second = default;
        Result<int, NotFoundError, ConflictError> third = default;
        Result<int, NotFoundError, ConflictError> fourth = default;
        Result<int, NotFoundError, ConflictError> fifth = default;
        Result<int, NotFoundError, ConflictError> sixth = default;
        Result<int, NotFoundError, ConflictError> seventh = default;
        Result<int, NotFoundError, ConflictError> eighth = default;

        Result<(int, int, int, int, int, int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth, fifth, sixth, seventh, eighth);

        Result<(int, int, int, int, int, int, int, int), NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine8_Should_ReturnTheErrorOfTheSecondResult_When_ItHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = TypedFixture.Conflict;
        Result<int, NotFoundError, ConflictError> third = default;
        Result<int, NotFoundError, ConflictError> fourth = default;
        Result<int, NotFoundError, ConflictError> fifth = default;
        Result<int, NotFoundError, ConflictError> sixth = default;
        Result<int, NotFoundError, ConflictError> seventh = default;
        Result<int, NotFoundError, ConflictError> eighth = default;

        Result<(int, int, int, int, int, int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth, fifth, sixth, seventh, eighth);

        Result<(int, int, int, int, int, int, int, int), NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine8_Should_ReturnTheErrorOfTheThirdResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = 2;
        Result<int, NotFoundError, ConflictError> third = TypedFixture.NotFound;
        Result<int, NotFoundError, ConflictError> fourth = default;
        Result<int, NotFoundError, ConflictError> fifth = default;
        Result<int, NotFoundError, ConflictError> sixth = default;
        Result<int, NotFoundError, ConflictError> seventh = default;
        Result<int, NotFoundError, ConflictError> eighth = default;

        Result<(int, int, int, int, int, int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth, fifth, sixth, seventh, eighth);

        Result<(int, int, int, int, int, int, int, int), NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine8_Should_ReturnTheErrorOfTheFourthResult_When_ItHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = 2;
        Result<int, NotFoundError, ConflictError> third = 3;
        Result<int, NotFoundError, ConflictError> fourth = TypedFixture.Conflict;
        Result<int, NotFoundError, ConflictError> fifth = default;
        Result<int, NotFoundError, ConflictError> sixth = default;
        Result<int, NotFoundError, ConflictError> seventh = default;
        Result<int, NotFoundError, ConflictError> eighth = default;

        Result<(int, int, int, int, int, int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth, fifth, sixth, seventh, eighth);

        Result<(int, int, int, int, int, int, int, int), NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine8_Should_ReturnTheErrorOfTheFifthResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = 2;
        Result<int, NotFoundError, ConflictError> third = 3;
        Result<int, NotFoundError, ConflictError> fourth = 4;
        Result<int, NotFoundError, ConflictError> fifth = TypedFixture.NotFound;
        Result<int, NotFoundError, ConflictError> sixth = default;
        Result<int, NotFoundError, ConflictError> seventh = default;
        Result<int, NotFoundError, ConflictError> eighth = default;

        Result<(int, int, int, int, int, int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth, fifth, sixth, seventh, eighth);

        Result<(int, int, int, int, int, int, int, int), NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine8_Should_ReturnTheErrorOfTheSixthResult_When_ItHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = 2;
        Result<int, NotFoundError, ConflictError> third = 3;
        Result<int, NotFoundError, ConflictError> fourth = 4;
        Result<int, NotFoundError, ConflictError> fifth = 5;
        Result<int, NotFoundError, ConflictError> sixth = TypedFixture.Conflict;
        Result<int, NotFoundError, ConflictError> seventh = default;
        Result<int, NotFoundError, ConflictError> eighth = default;

        Result<(int, int, int, int, int, int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth, fifth, sixth, seventh, eighth);

        Result<(int, int, int, int, int, int, int, int), NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine8_Should_ReturnTheErrorOfTheSeventhResult_When_ItHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = 2;
        Result<int, NotFoundError, ConflictError> third = 3;
        Result<int, NotFoundError, ConflictError> fourth = 4;
        Result<int, NotFoundError, ConflictError> fifth = 5;
        Result<int, NotFoundError, ConflictError> sixth = 6;
        Result<int, NotFoundError, ConflictError> seventh = TypedFixture.NotFound;
        Result<int, NotFoundError, ConflictError> eighth = default;

        Result<(int, int, int, int, int, int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth, fifth, sixth, seventh, eighth);

        Result<(int, int, int, int, int, int, int, int), NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine8_Should_ReturnTheErrorOfTheEighthResult_When_ItHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = 2;
        Result<int, NotFoundError, ConflictError> third = 3;
        Result<int, NotFoundError, ConflictError> fourth = 4;
        Result<int, NotFoundError, ConflictError> fifth = 5;
        Result<int, NotFoundError, ConflictError> sixth = 6;
        Result<int, NotFoundError, ConflictError> seventh = 7;
        Result<int, NotFoundError, ConflictError> eighth = TypedFixture.Conflict;

        Result<(int, int, int, int, int, int, int, int), NotFoundError, ConflictError> result = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth, fifth, sixth, seventh, eighth);

        Result<(int, int, int, int, int, int, int, int), NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
    }

    [Fact]
    public void Combine8_Should_Throw_When_AResultBeforeTheFirstErrorIsDefault()
    {
        Result<int, NotFoundError, ConflictError> first = 1;
        Result<int, NotFoundError, ConflictError> second = 2;
        Result<int, NotFoundError, ConflictError> third = 3;
        Result<int, NotFoundError, ConflictError> fourth = 4;
        Result<int, NotFoundError, ConflictError> fifth = 5;
        Result<int, NotFoundError, ConflictError> sixth = 6;
        Result<int, NotFoundError, ConflictError> seventh = 7;
        Result<int, NotFoundError, ConflictError> eighth = default;

        Action act = () => _ = Result<int, NotFoundError, ConflictError>.Combine(first, second, third, fourth, fifth, sixth, seventh, eighth);

        act.Should().Throw<InvalidOperationException>();
    }
}
