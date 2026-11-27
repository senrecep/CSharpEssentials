// Produced by scripts/generate-typed-results.py; do not edit.
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;

public sealed class ResultTE1E2E3SelectTests
{
    [Fact]
    public void Select_Should_MapTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;

        Result<string, NotFoundError, ConflictError, RateLimited> result = source.Select(v => { calls++; return TypedFixture.Text(v); });

        calls.Should().Be(1);
        Result<string, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Text(5);
        result.Should().Be(expected);
    }

    [Fact]
    public void Select_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;

        Result<string, NotFoundError, ConflictError, RateLimited> result = source.Select(v => { calls++; return TypedFixture.Text(v); });

        calls.Should().Be(0);
        Result<string, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void Select_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;

        Result<string, NotFoundError, ConflictError, RateLimited> result = source.Select(v => { calls++; return TypedFixture.Text(v); });

        calls.Should().Be(0);
        Result<string, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public void Select_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;

        Result<string, NotFoundError, ConflictError, RateLimited> result = source.Select(v => { calls++; return TypedFixture.Text(v); });

        calls.Should().Be(0);
        Result<string, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public void Select_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;

        Action act = () => _ = source.Select(v => { calls++; return TypedFixture.Text(v); });

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public void Select_Should_Throw_When_TheSelectorReturnsNull()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;

        Action act = () => _ = source.Select<string>(v => { calls++; return null!; });

        act.Should().Throw<ArgumentNullException>();
        calls.Should().Be(1);
    }

    [Fact]
    public void SelectMany_Should_ProjectBothValues_When_BothResultsHoldAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        Result<int, NotFoundError, ConflictError, RateLimited> other = 7;

        Result<string, NotFoundError, ConflictError, RateLimited> result = from a in source from b in other select TypedFixture.Text(a + b);

        Result<string, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Text(12);
        result.Should().Be(expected);
    }

    [Fact]
    public void SelectMany_Should_ReturnTheInnerError_When_TheInnerResultFails()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        Result<int, NotFoundError, ConflictError, RateLimited> other = TypedFixture.Limited;

        Result<string, NotFoundError, ConflictError, RateLimited> result = from a in source from b in other select TypedFixture.Text(a + b);

        Result<string, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public void SelectMany_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        Result<int, NotFoundError, ConflictError, RateLimited> other = 7;

        Result<string, NotFoundError, ConflictError, RateLimited> result = from a in source from b in other select TypedFixture.Text(a + b);

        Result<string, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void SelectMany_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        Result<int, NotFoundError, ConflictError, RateLimited> other = 7;

        Result<string, NotFoundError, ConflictError, RateLimited> result = from a in source from b in other select TypedFixture.Text(a + b);

        Result<string, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public void SelectMany_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        Result<int, NotFoundError, ConflictError, RateLimited> other = 7;

        Result<string, NotFoundError, ConflictError, RateLimited> result = from a in source from b in other select TypedFixture.Text(a + b);

        Result<string, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public void SelectMany_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        Result<int, NotFoundError, ConflictError, RateLimited> other = 7;

        Action act = () => _ = from a in source from b in other select TypedFixture.Text(a + b);

        act.Should().Throw<InvalidOperationException>();
    }
}
