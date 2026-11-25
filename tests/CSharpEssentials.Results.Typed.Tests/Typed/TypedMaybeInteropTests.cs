using CSharpEssentials.Errors;
using CSharpEssentials.Maybe;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed;

public sealed class TypedMaybeInteropTests
{
    [Fact]
    public void AsMaybe_Should_ReturnTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError> source = 5;

        Maybe<int> maybe = source.AsMaybe();

        maybe.Should().Be(Maybe<int>.From(5));
    }

    [Fact]
    public void AsMaybe_Should_ReturnNone_When_TheResultHoldsAnError()
    {
        Result<int, NotFoundError, ConflictError, RateLimited> source = TypedFixture.Limited;

        Maybe<int> maybe = source.AsMaybe();

        maybe.HasNoValue.Should().BeTrue();
    }

    [Fact]
    public void AsMaybe_Should_ReturnNone_When_TheTwoSlotResultHoldsAnError()
    {
        Result<int, NotFoundError, ConflictError> source = TypedFixture.Conflict;

        Maybe<int> maybe = source.AsMaybe();

        maybe.HasNoValue.Should().BeTrue();
    }

    [Fact]
    public void AsMaybe_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> source = default;

        Action act = () => source.AsMaybe();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task AsMaybeAsync_Should_ReturnTheValue_When_ATaskSourceHoldsAValue()
    {
        Task<Result<int, NotFoundError>> source = Task.FromResult<Result<int, NotFoundError>>(5);

        Maybe<int> maybe = await source.AsMaybeAsync();

        maybe.Should().Be(Maybe<int>.From(5));
    }

    [Fact]
    public async Task AsMaybeAsync_Should_ReturnNone_When_AValueTaskSourceHoldsAnError()
    {
        ValueTask<Result<int, NotFoundError, ConflictError, RateLimited>> source = new(TypedFixture.NotFound);

        Maybe<int> maybe = await source.AsMaybeAsync();

        maybe.HasNoValue.Should().BeTrue();
    }

    [Fact]
    public async Task AsMaybeAsync_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError>> pending = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();

        Func<Task> act = async () => await pending.Task.AsMaybeAsync(cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public void ToResult_Should_HoldTheValue_When_TheMaybeHasAValue()
    {
        Maybe<int> maybe = 5;

        Result<int, NotFoundError> result = maybe.ToResult(TypedFixture.NotFound);

        result.Should().Be((Result<int, NotFoundError>)5);
    }

    [Fact]
    public void ToResult_Should_HoldTheTypedError_When_TheMaybeHasNoValue()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Result<int, NotFoundError> result = maybe.ToResult(TypedFixture.NotFound);

        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public void ToResult_Should_KeepReturningTheClassicResult_When_TheErrorIsAnError()
    {
        Maybe<int> maybe = Maybe<int>.None;
        Error error = Error.NotFound("Item.Missing", "missing");

        Result<int> result = maybe.ToResult(error);

        result.FirstError.Should().Be(error);
    }

    [Fact]
    public void ToResult_Should_KeepReturningTheClassicResult_When_TheErrorIsNull()
    {
        Maybe<int> maybe = 5;

        Result<int> result = maybe.ToResult(null);

        result.Value.Should().Be(5);
    }

    [Fact]
    public void ToResult_Should_UseTheFallbackError_When_TheErrorIsDefault()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Result<int> result = maybe.ToResult(default);

        result.FirstError.Should().Be(maybe.ToResult(null).FirstError);
    }
}
