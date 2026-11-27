// Produced by scripts/generate-typed-results.py; do not edit.
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;

public sealed class ResultTE1E2TryCatchTests
{
    [Fact]
    public void TryCatch_Should_ReturnTheValue_When_TheFunctionReturns()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;

        Result<string, NotFoundError, ConflictError, FormatException> result = source.TryCatch<string, FormatException>(v => { calls++; return TypedFixture.Text(v); });

        calls.Should().Be(1);
        Result<string, NotFoundError, ConflictError, FormatException> expected = TypedFixture.Text(5);
        result.Should().Be(expected);
    }

    [Fact]
    public void TryCatch_Should_ReturnTheException_When_TheFunctionThrowsTheCaughtType()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;

        Result<string, NotFoundError, ConflictError, FormatException> result = source.TryCatch<string, FormatException>(v => { calls++; throw TypedFixture.Format; });

        calls.Should().Be(1);
        Result<string, NotFoundError, ConflictError, FormatException> expected = TypedFixture.Format;
        result.Should().Be(expected);
    }

    [Fact]
    public void TryCatch_Should_Throw_When_TheFunctionThrowsAnotherType()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;

        Action act = () => _ = source.TryCatch<string, FormatException>(v => { calls++; throw new InvalidOperationException(); });

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(1);
    }

    [Fact]
    public void TryCatch_Should_Throw_When_TheFunctionReturnsNull()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;

        Action act = () => _ = source.TryCatch<string, FormatException>(v => { calls++; return null!; });

        act.Should().Throw<ArgumentNullException>();
        calls.Should().Be(1);
    }

    [Fact]
    public void TryCatch_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.NotFound;
        int calls = 0;

        Result<string, NotFoundError, ConflictError, FormatException> result = source.TryCatch<string, FormatException>(v => { calls++; return TypedFixture.Text(v); });

        calls.Should().Be(0);
        Result<string, NotFoundError, ConflictError, FormatException> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void TryCatch_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;

        Result<string, NotFoundError, ConflictError, FormatException> result = source.TryCatch<string, FormatException>(v => { calls++; return TypedFixture.Text(v); });

        calls.Should().Be(0);
        Result<string, NotFoundError, ConflictError, FormatException> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public void TryCatch_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;

        Action act = () => _ = source.TryCatch<string, FormatException>(v => { calls++; return TypedFixture.Text(v); });

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }
}
