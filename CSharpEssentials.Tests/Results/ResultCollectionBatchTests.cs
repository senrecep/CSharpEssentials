using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
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
    public void FirstFailureOrSuccesses_WithSuccesses_ShouldReturnValues()
    {
        Result<int>[] source = [1, 2, 3];

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
}
