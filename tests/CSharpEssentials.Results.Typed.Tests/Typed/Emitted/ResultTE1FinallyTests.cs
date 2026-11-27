// Produced by scripts/generate-typed-results.py; do not edit.
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;

public sealed class ResultTE1FinallyTests
{
    [Fact]
    public void Finally_Should_ReturnTheFunctionValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;

        bool result = source.Finally(r => { calls++; return r.IsSuccess; });

        calls.Should().Be(1);
        bool expected = true;
        result.Should().Be(expected);
    }

    [Fact]
    public void Finally_Should_ReturnTheFunctionValue_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;

        bool result = source.Finally(r => { calls++; return r.IsSuccess; });

        calls.Should().Be(1);
        bool expected = false;
        result.Should().Be(expected);
    }

    [Fact]
    public void Finally_Should_PassTheDefaultResult_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;

        bool result = source.Finally(r => { calls++; return r.HasValue; });

        calls.Should().Be(1);
        bool expected = false;
        result.Should().Be(expected);
    }

    [Fact]
    public void FinallyAction_Should_CallTheAction_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;

        Result<int, NotFoundError> result = source.Finally(_ => { calls++; });

        calls.Should().Be(1);
        Result<int, NotFoundError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public void FinallyAction_Should_CallTheAction_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;

        Result<int, NotFoundError> result = source.Finally(_ => { calls++; });

        calls.Should().Be(1);
        Result<int, NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void FinallyAction_Should_CallTheAction_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;

        Result<int, NotFoundError> result = source.Finally(_ => { calls++; });

        calls.Should().Be(1);
        Result<int, NotFoundError> expected = default;
        result.Should().Be(expected);
    }
}
