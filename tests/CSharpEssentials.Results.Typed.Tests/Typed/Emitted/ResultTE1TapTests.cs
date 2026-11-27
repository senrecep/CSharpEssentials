// Produced by scripts/generate-typed-results.py; do not edit.
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;

public sealed class ResultTE1TapTests
{
    [Fact]
    public void Tap_Should_CallTheAction_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;

        Result<int, NotFoundError> result = source.Tap(_ => calls++);

        calls.Should().Be(1);
        Result<int, NotFoundError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public void Tap_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;

        Result<int, NotFoundError> result = source.Tap(_ => calls++);

        calls.Should().Be(0);
        Result<int, NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void Tap_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;

        Action act = () => _ = source.Tap(_ => calls++);

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public void TapIf_Should_CallTheAction_When_ThePredicateAllowsTheValue()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;

        Result<int, NotFoundError> result = source.TapIf(v => { calls++; return v > 0; }, _ => calls++);

        calls.Should().Be(2);
        Result<int, NotFoundError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public void TapIf_Should_SkipTheAction_When_ThePredicateRejectsTheValue()
    {
        Result<int, NotFoundError> source = -5;
        int calls = 0;

        Result<int, NotFoundError> result = source.TapIf(v => { calls++; return v > 0; }, _ => calls++);

        calls.Should().Be(1);
        Result<int, NotFoundError> expected = -5;
        result.Should().Be(expected);
    }

    [Fact]
    public void TapIf_Should_CallNeitherDelegate_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;

        Result<int, NotFoundError> result = source.TapIf(v => { calls++; return v > 0; }, _ => calls++);

        calls.Should().Be(0);
        Result<int, NotFoundError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void TapIf_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;

        Action act = () => _ = source.TapIf(v => { calls++; return v > 0; }, _ => calls++);

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }
}
