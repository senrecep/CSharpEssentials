// Produced by scripts/generate-typed-results.py; do not edit.
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;

public sealed class ResultTE1E2CollectionsTests
{
    [Fact]
    public void Sequence_Should_ReturnEveryValue_When_EveryResultHoldsAValue()
    {
        List<Result<int, NotFoundError, ConflictError>> source = [5, 7];

        Result<int[], NotFoundError, ConflictError> result = source.Sequence();

        result.TryGetValue(out int[] values).Should().BeTrue();
        values.Should().Equal(5, 7);
    }

    [Fact]
    public void Sequence_Should_ReturnTheFirstErrorWithoutLookingFurther_When_TheResultHoldsNotFoundError()
    {
        List<Result<int, NotFoundError, ConflictError>> source = [5, TypedFixture.NotFound, default];

        Result<int[], NotFoundError, ConflictError> result = source.Sequence();

        Result<int[], NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public void Sequence_Should_Throw_When_AResultBeforeTheFirstErrorIsDefault()
    {
        List<Result<int, NotFoundError, ConflictError>> source = [5, default, TypedFixture.NotFound];

        Action act = () => source.Sequence();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Traverse_Should_StopCallingTheSelector_When_ItReturnsConflictError()
    {
        int[] items = [1, 2, 3];
        int calls = 0;

        Result<int[], NotFoundError, ConflictError> result = items.Traverse(v => { calls++; return v == 2 ? (Result<int, NotFoundError, ConflictError>)TypedFixture.Conflict : (Result<int, NotFoundError, ConflictError>)v; });

        calls.Should().Be(2);
        Result<int[], NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
    }

    [Fact]
    public void Traverse_Should_ReturnEveryValue_When_TheSelectorAlwaysSucceeds()
    {
        int[] items = [1, 2];

        Result<int[], NotFoundError, ConflictError> result = items.Traverse(v => (Result<int, NotFoundError, ConflictError>)(v * 10));

        result.TryGetValue(out int[] values).Should().BeTrue();
        values.Should().Equal(10, 20);
    }

    [Fact]
    public void Partition_Should_SplitTheValuesAndTheConvertedErrors_When_TheResultsAreMixed()
    {
        List<Result<int, NotFoundError, ConflictError>> source = [5, TypedFixture.NotFound, TypedFixture.Conflict, 7];

        (int[] successes, Error[] failures) = source.Partition();

        successes.Should().Equal(5, 7);
        failures.Select(e => e.Code).Should().Equal(TypedFixture.NotFound.Code, TypedFixture.Conflict.Code);
    }

    [Fact]
    public void Partition_Should_Throw_When_AResultIsDefault()
    {
        List<Result<int, NotFoundError, ConflictError>> source = [5, default];

        Action act = () => source.Partition();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task SequenceAsync_Task_Should_ReturnEveryValue_When_EveryResultHoldsAValue()
    {
        List<Task<Result<int, NotFoundError, ConflictError>>> source = [Task.FromResult<Result<int, NotFoundError, ConflictError>>(5), Task.FromResult<Result<int, NotFoundError, ConflictError>>(7)];

        Result<int[], NotFoundError, ConflictError> result = await source.SequenceAsync();

        result.TryGetValue(out int[] values).Should().BeTrue();
        values.Should().Equal(5, 7);
    }

    [Fact]
    public async Task SequenceAsync_Task_Should_ReturnTheFirstErrorWithoutAwaitingTheRest_When_TheResultHoldsNotFoundError()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError>> never = new();
        List<Task<Result<int, NotFoundError, ConflictError>>> source = [Task.FromResult<Result<int, NotFoundError, ConflictError>>(TypedFixture.NotFound), never.Task];

        Result<int[], NotFoundError, ConflictError> result = await source.SequenceAsync();

        Result<int[], NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TraverseAsync_Task_Should_StopCallingTheSelector_When_ItReturnsConflictError()
    {
        int[] items = [1, 2, 3];
        int calls = 0;

        Result<int[], NotFoundError, ConflictError> result = await items.TraverseAsync(v => { calls++; return Task.FromResult<Result<int, NotFoundError, ConflictError>>(v == 2 ? (Result<int, NotFoundError, ConflictError>)TypedFixture.Conflict : (Result<int, NotFoundError, ConflictError>)v); });

        calls.Should().Be(2);
        Result<int[], NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TraverseAsync_TaskWithToken_Should_StopCallingTheSelector_When_ItReturnsConflictError()
    {
        int[] items = [1, 2, 3];
        int calls = 0;

        Result<int[], NotFoundError, ConflictError> result = await items.TraverseAsync((v, _) => { calls++; return Task.FromResult<Result<int, NotFoundError, ConflictError>>(v == 2 ? (Result<int, NotFoundError, ConflictError>)TypedFixture.Conflict : (Result<int, NotFoundError, ConflictError>)v); });

        calls.Should().Be(2);
        Result<int[], NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TraverseAsync_Task_Should_Throw_When_TheTokenIsCanceled()
    {
        int[] items = [1];
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();

        Func<Task> act = async () => await items.TraverseAsync(v => Task.FromResult<Result<int, NotFoundError, ConflictError>>((Result<int, NotFoundError, ConflictError>)v), cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task SequenceAsync_ValueTask_Should_ReturnEveryValue_When_EveryResultHoldsAValue()
    {
        List<ValueTask<Result<int, NotFoundError, ConflictError>>> source = [new ValueTask<Result<int, NotFoundError, ConflictError>>(5), new ValueTask<Result<int, NotFoundError, ConflictError>>(7)];

        Result<int[], NotFoundError, ConflictError> result = await source.SequenceAsync();

        result.TryGetValue(out int[] values).Should().BeTrue();
        values.Should().Equal(5, 7);
    }

    [Fact]
    public async Task SequenceAsync_ValueTask_Should_ReturnTheFirstErrorWithoutAwaitingTheRest_When_TheResultHoldsNotFoundError()
    {
        TaskCompletionSource<Result<int, NotFoundError, ConflictError>> never = new();
        List<ValueTask<Result<int, NotFoundError, ConflictError>>> source = [new ValueTask<Result<int, NotFoundError, ConflictError>>(TypedFixture.NotFound), new ValueTask<Result<int, NotFoundError, ConflictError>>(never.Task)];

        Result<int[], NotFoundError, ConflictError> result = await source.SequenceAsync();

        Result<int[], NotFoundError, ConflictError> expected = TypedFixture.NotFound;
        result.Should().Be(expected);
    }

#if !TYPED_NETSTANDARD_ASSET
    [Fact]
    public async Task TraverseAsync_ValueTask_Should_StopCallingTheSelector_When_ItReturnsConflictError()
    {
        int[] items = [1, 2, 3];
        int calls = 0;

        Result<int[], NotFoundError, ConflictError> result = await items.TraverseAsync(v => { calls++; return new ValueTask<Result<int, NotFoundError, ConflictError>>(v == 2 ? (Result<int, NotFoundError, ConflictError>)TypedFixture.Conflict : (Result<int, NotFoundError, ConflictError>)v); });

        calls.Should().Be(2);
        Result<int[], NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TraverseAsync_ValueTaskWithToken_Should_StopCallingTheSelector_When_ItReturnsConflictError()
    {
        int[] items = [1, 2, 3];
        int calls = 0;

        Result<int[], NotFoundError, ConflictError> result = await items.TraverseAsync((v, _) => { calls++; return new ValueTask<Result<int, NotFoundError, ConflictError>>(v == 2 ? (Result<int, NotFoundError, ConflictError>)TypedFixture.Conflict : (Result<int, NotFoundError, ConflictError>)v); });

        calls.Should().Be(2);
        Result<int[], NotFoundError, ConflictError> expected = TypedFixture.Conflict;
        result.Should().Be(expected);
    }

    [Fact]
    public async Task TraverseAsync_ValueTask_Should_Throw_When_TheTokenIsCanceled()
    {
        int[] items = [1];
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();

        Func<Task> act = async () => await items.TraverseAsync(v => new ValueTask<Result<int, NotFoundError, ConflictError>>((Result<int, NotFoundError, ConflictError>)v), cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
#endif
}
