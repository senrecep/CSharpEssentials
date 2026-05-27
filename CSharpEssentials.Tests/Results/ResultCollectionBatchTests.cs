using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.ResultPattern.Interfaces;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultCollectionBatchTests
{
    [Fact]
    public void Sequence_AllSuccesses_ShouldReturnValues()
    {
        Result<int>[] source = [1, 2, 3];

        Result<int[]> result = source.Sequence();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Equal(1, 2, 3);
    }

    [Fact]
    public void Sequence_WithFailures_ShouldAggregateErrors()
    {
        Result<int>[] source =
        [
            1,
            Error.Validation("First.Error", "First"),
            Error.Validation("Second.Error", "Second")
        ];

        Result<int[]> result = source.Sequence();

        result.IsFailure.Should().BeTrue();
        result.Errors.Select(x => x.Code).Should().Equal("First.Error", "Second.Error");
    }

    [Fact]
    public void Traverse_ShouldProjectAndSequence()
    {
        int[] source = [1, 2, 3];

        Result<string[]> result = source.Traverse(value => $"item-{value}".ToResult());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Equal("item-1", "item-2", "item-3");
    }

    [Fact]
    public void Partition_ShouldReturnSuccessesAndFlattenedErrors()
    {
        Result<int>[] source =
        [
            10,
            Error.Validation("First.Error", "First"),
            20,
            Error.Validation("Second.Error", "Second")
        ];

        (int[] successes, Error[] errors) = source.Partition();

        successes.Should().Equal(10, 20);
        errors.Select(x => x.Code).Should().Equal("First.Error", "Second.Error");
    }

    [Fact]
    public void CombineAll_NonGeneric_ShouldAggregateErrors()
    {
        Result[] source =
        [
            Result.Success(),
            Error.Validation("First.Error", "First"),
            Error.Validation("Second.Error", "Second")
        ];

        Result result = source.CombineAll();

        result.IsFailure.Should().BeTrue();
        result.Errors.Select(x => x.Code).Should().Equal("First.Error", "Second.Error");
    }

    [Fact]
    public void CombineAll_IResultBase_ShouldAggregateErrors()
    {
        IResultBase[] source =
        [
            Result.Success(),
            Result.Failure(Error.Validation("First.Error", "First")),
            Result.Failure(Error.Validation("Second.Error", "Second"))
        ];

        Result result = source.CombineAll();

        result.IsFailure.Should().BeTrue();
        result.Errors.Select(x => x.Code).Should().Equal("First.Error", "Second.Error");
    }

    [Fact]
    public void CombineAll_WithEmptyCollections_ShouldReturnSuccess()
    {
        Array.Empty<Result>().CombineAll().IsSuccess.Should().BeTrue();
        Array.Empty<IResultBase>().CombineAll().IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void FirstFailureOrSuccesses_WithSuccesses_ShouldReturnValues()
    {
        Result<int>[] source = [1, 2, 3];

        Result<int[]> result = source.FirstFailureOrSuccesses();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Equal(1, 2, 3);
    }

    [Fact]
    public void Sequence_IResultOverload_ShouldReturnValues()
    {
        IResult<int>[] source = [Result.Success(1), Result.Success(2), Result.Success(3)];

        Result<int[]> result = source.Sequence();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Equal(1, 2, 3);
    }

    [Fact]
    public void Partition_IResultOverload_ShouldReturnSuccessesAndFlattenedErrors()
    {
        IResult<int>[] source =
        [
            Result.Success(10),
            Result.Failure<int>(Error.Validation("First.Error", "First")),
            Result.Success(20),
            Result.Failure<int>(Error.Validation("Second.Error", "Second"))
        ];

        (int[] successes, Error[] errors) = source.Partition();

        successes.Should().Equal(10, 20);
        errors.Select(x => x.Code).Should().Equal("First.Error", "Second.Error");
    }

    [Fact]
    public void FirstFailureOrSuccesses_IResultOverload_WithSuccesses_ShouldReturnValues()
    {
        IResult<int>[] source = [Result.Success(1), Result.Success(2), Result.Success(3)];

        Result<int[]> result = source.FirstFailureOrSuccesses();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Equal(1, 2, 3);
    }

    [Fact]
    public void FirstFailureOrSuccesses_ShouldShortCircuitOnFirstFailure()
    {
        static IEnumerable<Result<int>> Source()
        {
            yield return 1;
            yield return Error.Validation("Stop.Error", "Stop");
            throw new InvalidOperationException("Enumeration should stop at first failure.");
        }

        Result<int[]> result = Source().FirstFailureOrSuccesses();

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(x => x.Code == "Stop.Error");
    }

    [Fact]
    public void ResultCollectionExtensions_WithEmptyCollections_ShouldReturnEmptySuccesses()
    {
        Array.Empty<Result<int>>().Sequence().Value.Should().BeEmpty();
        Array.Empty<IResult<int>>().Sequence().Value.Should().BeEmpty();
        Array.Empty<Result<int>>().FirstFailureOrSuccesses().Value.Should().BeEmpty();
        Array.Empty<IResult<int>>().FirstFailureOrSuccesses().Value.Should().BeEmpty();

        (int[] structValues, Error[] structErrors) = Array.Empty<Result<int>>().Partition();
        (int[] interfaceValues, Error[] interfaceErrors) = Array.Empty<IResult<int>>().Partition();

        structValues.Should().BeEmpty();
        structErrors.Should().BeEmpty();
        interfaceValues.Should().BeEmpty();
        interfaceErrors.Should().BeEmpty();
    }

    [Fact]
    public void ResultCollectionExtensions_WithNullSource_ShouldThrowArgumentNullException()
    {
        IEnumerable<Result> results = null!;
        IEnumerable<IResultBase> resultBases = null!;
        IEnumerable<Result<int>> genericResults = null!;
        IEnumerable<IResult<int>> genericInterfaces = null!;
        IEnumerable<int> values = null!;

        Action[] actions =
        [
            () => results.CombineAll(),
            () => resultBases.CombineAll(),
            () => genericResults.Sequence(),
            () => genericInterfaces.Sequence(),
            () => genericResults.Partition(),
            () => genericInterfaces.Partition(),
            () => results.FirstFailureOrSuccesses(),
            () => resultBases.FirstFailureOrSuccesses(),
            () => genericResults.FirstFailureOrSuccesses(),
            () => genericInterfaces.FirstFailureOrSuccesses(),
            () => values.Traverse(value => value.ToResult())
        ];

        foreach (Action action in actions)
            action.Should().Throw<ArgumentNullException>().WithParameterName("source");
    }
}
