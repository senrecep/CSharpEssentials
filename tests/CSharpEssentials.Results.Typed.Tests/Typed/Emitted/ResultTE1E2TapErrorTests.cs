// Produced by scripts/generate-typed-results.py; do not edit.
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;

public sealed class ResultTE1E2TapErrorTests
{
    [Fact]
    public void TapErrorNotFoundError_Should_SkipTheAction_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;

        Result<int, NotFoundError, ConflictError> result = source.TapError((NotFoundError _) => calls++);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public void TapErrorNotFoundError_Should_CallTheAction_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.NotFound;
        int calls = 0;

        Result<int, NotFoundError, ConflictError> result = source.TapError((NotFoundError _) => calls++);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void TapErrorNotFoundError_Should_PassTheErrorThrough_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;

        Result<int, NotFoundError, ConflictError> result = source.TapError((NotFoundError _) => calls++);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public void TapErrorNotFoundError_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;

        Action act = () => _ = source.TapError((NotFoundError _) => calls++);

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public void TapErrorConflictError_Should_SkipTheAction_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;

        Result<int, NotFoundError, ConflictError> result = source.TapError((ConflictError _) => calls++);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public void TapErrorConflictError_Should_PassTheErrorThrough_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.NotFound;
        int calls = 0;

        Result<int, NotFoundError, ConflictError> result = source.TapError((ConflictError _) => calls++);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void TapErrorConflictError_Should_CallTheAction_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        int calls = 0;

        Result<int, NotFoundError, ConflictError> result = source.TapError((ConflictError _) => calls++);

        calls.Should().Be(1);
        Result<int, NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public void TapErrorConflictError_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;

        Action act = () => _ = source.TapError((ConflictError _) => calls++);

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public void TapErrorAny_Should_SkipTheAction_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        int calls = 0;

        Result<int, NotFoundError, ConflictError> result = source.TapError((Error _) => calls++);

        calls.Should().Be(0);
        Result<int, NotFoundError, ConflictError> expected = 5;
        result.Should().Be(expected);
    }

    [Fact]
    public void TapErrorAny_Should_PassTheConvertedError_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.NotFound;
        string? code = null;

        Result<int, NotFoundError, ConflictError> result = source.TapError((Error error) => code = error.Code);

        code.Should().Be(TypedFixture.NotFound.Code);
        result.Should().Be(source);
    }

    [Fact]
    public void TapErrorAny_Should_PassTheConvertedError_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        string? code = null;

        Result<int, NotFoundError, ConflictError> result = source.TapError((Error error) => code = error.Code);

        code.Should().Be(TypedFixture.Conflict.Code);
        result.Should().Be(source);
    }

    [Fact]
    public void TapErrorAny_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;
        int calls = 0;

        Action act = () => _ = source.TapError((Error _) => calls++);

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task TapErrorAnyAsync_InstanceTaskHandler_Should_SkipTheAction_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        string? code = null;

        Result<int, NotFoundError, ConflictError> result = await source.TapErrorAsync((Error error) => { code = error.Code; return Task.CompletedTask; });

        code.Should().Be(null);
        result.Should().Be(source);
    }

    [Fact]
    public async Task TapErrorAnyAsync_InstanceTaskHandler_Should_PassTheConvertedError_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        string? code = null;

        Result<int, NotFoundError, ConflictError> result = await source.TapErrorAsync((Error error) => { code = error.Code; return Task.CompletedTask; });

        code.Should().Be(TypedFixture.Conflict.Code);
        result.Should().Be(source);
    }

    [Fact]
    public async Task TapErrorAnyAsync_TaskSourceTaskHandler_Should_SkipTheAction_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        string? code = null;

        Result<int, NotFoundError, ConflictError> result = await Task.FromResult(source).TapErrorAsync((Error error) => { code = error.Code; return Task.CompletedTask; });

        code.Should().Be(null);
        result.Should().Be(source);
    }

    [Fact]
    public async Task TapErrorAnyAsync_TaskSourceTaskHandler_Should_PassTheConvertedError_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        string? code = null;

        Result<int, NotFoundError, ConflictError> result = await Task.FromResult(source).TapErrorAsync((Error error) => { code = error.Code; return Task.CompletedTask; });

        code.Should().Be(TypedFixture.Conflict.Code);
        result.Should().Be(source);
    }

    [Fact]
    public async Task TapErrorAnyAsync_ValueTaskSourceTaskHandler_Should_SkipTheAction_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        string? code = null;

        Result<int, NotFoundError, ConflictError> result = await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).TapErrorAsync((Error error) => { code = error.Code; return Task.CompletedTask; });

        code.Should().Be(null);
        result.Should().Be(source);
    }

    [Fact]
    public async Task TapErrorAnyAsync_ValueTaskSourceTaskHandler_Should_PassTheConvertedError_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        string? code = null;

        Result<int, NotFoundError, ConflictError> result = await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).TapErrorAsync((Error error) => { code = error.Code; return Task.CompletedTask; });

        code.Should().Be(TypedFixture.Conflict.Code);
        result.Should().Be(source);
    }

#if !TYPED_NETSTANDARD_ASSET
    [Fact]
    public async Task TapErrorAnyAsync_InstanceValueTaskHandler_Should_SkipTheAction_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        string? code = null;

        Result<int, NotFoundError, ConflictError> result = await source.TapErrorAsync((Error error) => { code = error.Code; return ValueTask.CompletedTask; });

        code.Should().Be(null);
        result.Should().Be(source);
    }

    [Fact]
    public async Task TapErrorAnyAsync_InstanceValueTaskHandler_Should_PassTheConvertedError_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        string? code = null;

        Result<int, NotFoundError, ConflictError> result = await source.TapErrorAsync((Error error) => { code = error.Code; return ValueTask.CompletedTask; });

        code.Should().Be(TypedFixture.Conflict.Code);
        result.Should().Be(source);
    }

    [Fact]
    public async Task TapErrorAnyAsync_ValueTaskSourceValueTaskHandler_Should_SkipTheAction_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError, ConflictError> source = 5;
        string? code = null;

        Result<int, NotFoundError, ConflictError> result = await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).TapErrorAsync((Error error) => { code = error.Code; return ValueTask.CompletedTask; });

        code.Should().Be(null);
        result.Should().Be(source);
    }

    [Fact]
    public async Task TapErrorAnyAsync_ValueTaskSourceValueTaskHandler_Should_PassTheConvertedError_When_TheResultHoldsConflictError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;
        string? code = null;

        Result<int, NotFoundError, ConflictError> result = await new ValueTask<Result<int, NotFoundError, ConflictError>>(source).TapErrorAsync((Error error) => { code = error.Code; return ValueTask.CompletedTask; });

        code.Should().Be(TypedFixture.Conflict.Code);
        result.Should().Be(source);
    }
#endif
}
