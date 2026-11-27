// Produced by scripts/generate-typed-results.py; do not edit.
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;

public sealed class ResultTE1E2GetValueTests
{
    [Fact]
    public void GetValueOrDefault_Should_ReturnTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;

        int result = source.GetValueOrDefault();

        calls.Should().Be(0);
        result.Should().Be(5);
    }

    [Fact]
    public void GetValueOrDefault_Should_ReturnDefault_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;

        int result = source.GetValueOrDefault();

        calls.Should().Be(0);
        result.Should().Be(0);
    }

    [Fact]
    public void GetValueOr_Should_ReturnTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;

        int result = source.GetValueOr(9);

        calls.Should().Be(0);
        result.Should().Be(5);
    }

    [Fact]
    public void GetValueOr_Should_ReturnTheFallback_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;

        int result = source.GetValueOr(9);

        calls.Should().Be(0);
        result.Should().Be(9);
    }

    [Fact]
    public void GetValueOrThrow_Should_ReturnTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;

        int result = source.GetValueOrThrow();

        calls.Should().Be(0);
        result.Should().Be(5);
    }

    [Fact]
    public void GetValueOrThrow_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;

        Action act = () => _ = source.GetValueOrThrow();

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public void GetValueOrAny_Should_ReturnTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;

        int result = source.GetValueOr((Error e) => { calls++; return e.Code.Length; });

        calls.Should().Be(0);
        result.Should().Be(5);
    }

    [Fact]
    public void GetValueOrAny_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;

        Action act = () => _ = source.GetValueOr((Error e) => { calls++; return e.Code.Length; });

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public void GetValueOrDefault_Should_ReturnDefault_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.NotFound;
        int calls = 0;

        int result = source.GetValueOrDefault();

        calls.Should().Be(0);
        result.Should().Be(0);
    }

    [Fact]
    public void GetValueOr_Should_ReturnTheFallback_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.NotFound;
        int calls = 0;

        int result = source.GetValueOr(9);

        calls.Should().Be(0);
        result.Should().Be(9);
    }

    [Fact]
    public void GetValueOrThrow_Should_Throw_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.NotFound;
        int calls = 0;

        Action act = () => _ = source.GetValueOrThrow();

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public void GetValueOrAny_Should_CallTheFallbackWithTheConvertedError_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.NotFound;
        int calls = 0;

        int result = source.GetValueOr((Error e) => { calls++; return e.Code.Length; });

        calls.Should().Be(1);
        result.Should().Be(TypedFixture.NotFound.Code.Length);
    }

    [Fact]
    public void GetValueOrDefault_Should_ReturnDefault_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;

        int result = source.GetValueOrDefault();

        calls.Should().Be(0);
        result.Should().Be(0);
    }

    [Fact]
    public void GetValueOr_Should_ReturnTheFallback_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;

        int result = source.GetValueOr(9);

        calls.Should().Be(0);
        result.Should().Be(9);
    }

    [Fact]
    public void GetValueOrThrow_Should_Throw_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;

        Action act = () => _ = source.GetValueOrThrow();

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public void GetValueOrAny_Should_CallTheFallbackWithTheConvertedError_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;

        int result = source.GetValueOr((Error e) => { calls++; return e.Code.Length; });

        calls.Should().Be(1);
        result.Should().Be(TypedFixture.Conflict.Code.Length);
    }

    [Fact]
    public async Task GetValueOrDefaultAsync_TaskSource_Should_ReturnTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;

        int result = await Task.FromResult(source).GetValueOrDefaultAsync();

        result.Should().Be(5);
    }

    [Fact]
    public async Task GetValueOrDefaultAsync_TaskSource_Should_ReturnDefault_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;

        int result = await Task.FromResult(source).GetValueOrDefaultAsync();

        result.Should().Be(0);
    }

    [Fact]
    public async Task GetValueOrAsync_TaskSource_Should_ReturnTheFallback_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;

        int result = await Task.FromResult(source).GetValueOrAsync(9);

        result.Should().Be(9);
    }

    [Fact]
    public async Task GetValueOrAnyAsync_TaskSource_Should_CallTheFallbackWithTheConvertedError_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;

        int result = await Task.FromResult(source).GetValueOrAsync((Error e) => e.Code.Length);

        result.Should().Be(TypedFixture.Conflict.Code.Length);
    }

    [Fact]
    public async Task GetValueOrDefaultAsync_ValueTaskSource_Should_ReturnTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;

        int result = await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).GetValueOrDefaultAsync();

        result.Should().Be(5);
    }

    [Fact]
    public async Task GetValueOrDefaultAsync_ValueTaskSource_Should_ReturnDefault_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;

        int result = await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).GetValueOrDefaultAsync();

        result.Should().Be(0);
    }

    [Fact]
    public async Task GetValueOrAsync_ValueTaskSource_Should_ReturnTheFallback_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;

        int result = await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).GetValueOrAsync(9);

        result.Should().Be(9);
    }

    [Fact]
    public async Task GetValueOrAnyAsync_ValueTaskSource_Should_CallTheFallbackWithTheConvertedError_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;

        int result = await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).GetValueOrAsync((Error e) => e.Code.Length);

        result.Should().Be(TypedFixture.Conflict.Code.Length);
    }
}
