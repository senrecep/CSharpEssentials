using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed;

/// <summary>
/// Overload resolution of the async cells with untyped lambdas, the way callers write them. The emitted tests type every handler,
/// so these are the only place where <c>[OverloadResolutionPriority]</c> decides the call. The netstandard2.1 sibling project
/// compiles the same calls against the internal priority polyfill of that asset.
/// </summary>
public sealed class TypedAsyncResolutionTests
{
    private static async Task<int> Later(int value)
    {
        await Task.Yield();
        return value;
    }

    [Fact]
    public async Task BindAsync_Should_BindTheTaskOverload_When_TheLambdaIsUntypedAsync()
    {
        Result<int, NotFoundError> source = 5;

        Task<Result<string, NotFoundError>> pending = source.BindAsync<string>(async v => TypedFixture.Text(await Later(v)));
        Result<string, NotFoundError> result = await pending;

        result.Should().Be((Result<string, NotFoundError>)TypedFixture.Text(5));
    }

    [Fact]
    public async Task TapIfAsync_Should_AwaitTheTaskHandler_When_ALambdaReturnsATaskOnAValueTaskSource()
    {
        ValueTask<Result<int, NotFoundError>> source = new(5);
        var handler = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        ValueTask<Result<int, NotFoundError>> pending = source.TapIfAsync(v => v > 0, _ => handler.Task);
        bool completedBeforeHandler = pending.IsCompleted;
        handler.SetResult(true);
        Result<int, NotFoundError> result = await pending;

        (completedBeforeHandler, result.Value).Should().Be((false, 5));
    }

    [Fact]
    public async Task TapIfAsync_Should_BindTheTaskOverload_When_TheLambdaIsUntypedAsyncOnAValueTaskSource()
    {
        ValueTask<Result<int, NotFoundError>> source = new(5);
        int seen = 0;

        Result<int, NotFoundError> result = await source.TapIfAsync(v => v > 0, async v => seen = await Later(v));

        (result.Value, seen).Should().Be((5, 5));
    }

    [Fact]
    public async Task MapErrorAsync_Should_BindTheTaskOverload_When_TheLambdaIsUntypedAsync()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;

        Task<Result<int, ValidationError>> pending = source.MapErrorAsync(async (NotFoundError _) =>
        {
            await Task.Yield();
            return TypedFixture.Invalid;
        });
        Result<int, ValidationError> result = await pending;

        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task EnsureAsync_Should_KeepTheErrorTypes_When_AnUntypedAsyncLambdaGetsAnErrorOfASlotTypeOnAValueTaskSource()
    {
        ValueTask<Result<int, NotFoundError, ConflictError>> source = new(-5);

        Result<int, NotFoundError, ConflictError> result = await source.EnsureAsync(async v => await Later(v) > 0, TypedFixture.Conflict);

        result.Value.Should().BeSameAs(TypedFixture.Conflict);
    }

    [Fact]
    public async Task FailIfAsync_Should_KeepTheErrorTypes_When_ASyncLambdaGetsAnErrorOfASlotTypeOnATaskSource()
    {
        Task<Result<int, NotFoundError>> source = Task.FromResult<Result<int, NotFoundError>>(-5);

        Result<int, NotFoundError> result = await source.FailIfAsync(v => v < 0, TypedFixture.NotFound);

        result.Value.Should().BeSameAs(TypedFixture.NotFound);
    }

    [Fact]
    public async Task EnsureAsync_Should_AddASlot_When_TheErrorTypeIsNotASlotType()
    {
        Task<Result<int, NotFoundError>> source = Task.FromResult<Result<int, NotFoundError>>(-5);

        Result<int, NotFoundError, ValidationError> result = await source.EnsureAsync(async v => await Later(v) > 0, TypedFixture.Invalid);

        result.Value.Should().BeSameAs(TypedFixture.Invalid);
    }

    [Fact]
    public async Task EnsureAsync_Should_KeepTheBaseSlot_When_ASourceGetsADerivedError()
    {
        UserErrors.Locked locked = new(7);
        Task<Result<int, ConflictError>> source = Task.FromResult<Result<int, ConflictError>>(-5);

        Result<int, ConflictError> result = await source.EnsureAsync(v => v > 0, locked);

        result.Value.Should().BeSameAs(locked);
    }

    [Fact]
    public async Task EnsureAsync_Should_AddASlot_When_AnInstanceGetsADerivedError()
    {
        UserErrors.Locked locked = new(7);
        Result<int, ConflictError> source = -5;

        Result<int, ConflictError, UserErrors.Locked> result = await source.EnsureAsync(async v => await Later(v) > 0, locked);

        result.Value.Should().BeSameAs(locked);
    }
}
