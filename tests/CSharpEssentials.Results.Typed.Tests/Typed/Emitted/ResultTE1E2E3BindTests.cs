// Produced by scripts/generate-typed-results.py; do not edit.
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;

public sealed class ResultTE1E2E3BindTests
{
    [Fact]
    public void Bind_Should_ReturnTheInnerResult_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;

        Result<string, NotFoundError, ConflictError, RateLimited> result = source.Bind<string>(v => { calls++; return TypedFixture.Text(v); });

        calls.Should().Be(1);
        Result<string, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Text(5);
        result.Should().Be(expected);
    }

    [Fact]
    public void Bind_Should_ReturnTheInnerError_When_TheInnerResultFails()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = 5;
        int calls = 0;

        Result<string, NotFoundError, ConflictError, RateLimited> result = source.Bind<string>(v => { calls++; return TypedFixture.Limited; });

        calls.Should().Be(1);
        Result<string, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public void Bind_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.NotFound;
        int calls = 0;

        Result<string, NotFoundError, ConflictError, RateLimited> result = source.Bind<string>(v => { calls++; return TypedFixture.Text(v); });

        calls.Should().Be(0);
        Result<string, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void Bind_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Conflict;
        int calls = 0;

        Result<string, NotFoundError, ConflictError, RateLimited> result = source.Bind<string>(v => { calls++; return TypedFixture.Text(v); });

        calls.Should().Be(0);
        Result<string, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public void Bind_Should_PassTheErrorThrough_When_TheResultHoldsRateLimited()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;
        int calls = 0;

        Result<string, NotFoundError, ConflictError, RateLimited> result = source.Bind<string>(v => { calls++; return TypedFixture.Text(v); });

        calls.Should().Be(0);
        Result<string, NotFoundError, ConflictError, RateLimited> expected = TypedFixture.Limited;
        result.Should().Be(expected);
        result.Value.Should().Be(TypedFixture.Limited);
    }

    [Fact]
    public void Bind_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = default;
        int calls = 0;

        Action act = () => _ = source.Bind<string>(v => { calls++; return TypedFixture.Text(v); });

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }
}
