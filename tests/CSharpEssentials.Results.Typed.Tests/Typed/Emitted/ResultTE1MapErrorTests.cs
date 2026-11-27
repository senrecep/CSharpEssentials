// Produced by scripts/generate-typed-results.py; do not edit.
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;

public sealed class ResultTE1MapErrorTests
{
    [Fact]
    public void MapErrorNotFoundError_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;

        Result<int, ValidationError> result = source.MapError((NotFoundError e) => { calls++; return TypedFixture.Invalid; });

        calls.Should().Be(0);
        Result<int, ValidationError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public void MapErrorNotFoundError_Should_MapTheError_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;

        Result<int, ValidationError> result = source.MapError((NotFoundError e) => { calls++; return TypedFixture.Invalid; });

        calls.Should().Be(1);
        Result<int, ValidationError> expected = TypedFixture.Invalid;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public void MapErrorNotFoundError_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;

        Action act = () => _ = source.MapError((NotFoundError e) => { calls++; return TypedFixture.Invalid; });

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public void MapError_Should_Throw_When_TheMapReturnsNull()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;

        Action act = () => _ = source.MapError((NotFoundError e) => { calls++; return (ValidationError)null!; });

        act.Should().Throw<ArgumentNullException>();
        calls.Should().Be(1);
    }
}
