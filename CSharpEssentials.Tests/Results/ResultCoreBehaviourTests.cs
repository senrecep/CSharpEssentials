using System.Collections;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.ResultPattern.Interfaces;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultCoreBehaviourTests
{
    private static readonly Error First = Error.Failure("E1", "First error");
    private static readonly Error Second = Error.Failure("E2", "Second error");
    private static readonly Error NotFound = Error.NotFound("NF", "Not found");

    private sealed class ReadOnlyOnly<T>(params T[] items) : IReadOnlyCollection<T>
    {
        public int Count => items.Length;

        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)items).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    #region Result

    [Fact]
    public void Result_Errors_OnSuccess_ReturnsNoErrorsSentinel()
    {
        Result.Success().Errors.Should().ContainSingle().Which.Should().Be(Error.NoErrors);
    }

    [Fact]
    public void Result_ToString_WithManyErrors_NamesFirstAndLast()
    {
        Result result = new[] { First, Second };

        string text = result.ToString();

        text.Should().StartWith("Failure: 2 errors");
        text.Should().Contain("E1").And.Contain("E2");
    }

    [Fact]
    public void Result_Equals_ComparesStateAndErrors()
    {
        Result failure = new[] { First, Second };

        failure.Equals(failure).Should().BeTrue();
        failure.Equals(Result.Success()).Should().BeFalse();
        failure.Equals(Result.Failure(First)).Should().BeFalse();
        failure.Equals(Result.Failure(First, Second)).Should().BeTrue();
        failure.Equals(Result.Failure(First, NotFound)).Should().BeFalse();
    }

    [Fact]
    public void Result_GetHashCode_EqualFailuresShareHash()
    {
        Result left = new[] { First, Second };
        Result right = new[] { First, Second };

        left.GetHashCode().Should().Be(right.GetHashCode());
        Result.Success().GetHashCode().Should().Be(Result.Success().GetHashCode());
    }

    [Fact]
    public void Result_ImplicitFromHashSet_CreatesFailure()
    {
        Result result = new HashSet<Error> { First, Second };

        result.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void Result_FailureT_FromEnumerable_CreatesFailureOrRejectsEmpty()
    {
        Result<int> result = Result.Failure<int>(new List<Error> { First, Second });
        Action empty = () => Result.Failure<int>(new List<Error>());

        result.Errors.Select(e => e.Code).Should().Equal("E1", "E2");
        empty.Should().Throw<ArgumentException>();
    }

    #endregion

    #region Result<T>

    [Fact]
    public void ResultT_ToResult_DropsValueAndKeepsErrors()
    {
        Result<int> success = 5;
        Result<int> failure = First;

        success.ToResult().IsSuccess.Should().BeTrue();
        failure.ToResult().Errors.Should().ContainSingle().Which.Code.Should().Be("E1");
    }

    [Fact]
    public void ResultT_ToString_DescribesEachState()
    {
        Result<int> success = 5;
        Result<int> single = First;
        Result<int> many = new[] { First, Second };

        success.ToString().Should().Be("Success: 5");
        single.ToString().Should().StartWith("Failure: 1 error,").And.Contain("E1");
        many.ToString().Should().StartWith("Failure: 2 errors").And.Contain("E2");
    }

    [Fact]
    public void ResultT_Equals_ComparesValuesAndErrors()
    {
        Result<int> failure = new[] { First, Second };

        Result<int>.Success(5).Equals(Result<int>.Success(5)).Should().BeTrue();
        Result<int>.Success(5).Equals(Result<int>.Success(6)).Should().BeFalse();
        failure.Equals(Result<int>.Failure(First, Second)).Should().BeTrue();
        failure.Equals(Result<int>.Failure(First)).Should().BeFalse();
        failure.Equals(Result<int>.Success(5)).Should().BeFalse();
    }

    [Fact]
    public void ResultT_GetHashCode_EqualResultsShareHash()
    {
        Result<int> left = new[] { First, Second };
        Result<int> right = new[] { First, Second };

        left.GetHashCode().Should().Be(right.GetHashCode());
        Result<string>.Success("a").GetHashCode().Should().Be(Result<string>.Success("a").GetHashCode());
    }

    [Fact]
    public void ResultT_PlusOperator_CollectsValuesOrErrors()
    {
        Result<int> one = 1;
        Result<int> two = 2;
        Result<int> failed = First;

        (one + two).Value.Should().Equal(1, 2);
        (one + failed).Errors.Should().ContainSingle().Which.Code.Should().Be("E1");
    }

    [Fact]
    public void ResultT_ImplicitFromHashSet_CreatesFailure()
    {
        Result<int> result = new HashSet<Error> { First };

        result.FirstError.Code.Should().Be("E1");
    }

    [Fact]
    public void ResultT_From_EmptyEnumerable_Throws()
    {
        Action act = () => Result<int>.From(Enumerable.Empty<Error>());

        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region Recover

    [Fact]
    public void Recover_ErrorTypeWithResultRecovery_HandlesMatchNoMatchAndSuccess()
    {
        Result<int> failure = new[] { First, NotFound };
        Func<Error, Result<int>> recovery = e => e.Code.Length;

        failure.Recover(ErrorType.NotFound, recovery).Value.Should().Be(2);
        failure.Recover(ErrorType.Conflict, recovery).Errors.Should().HaveCount(2);
        Result<int>.Success(9).Recover(ErrorType.NotFound, recovery).Value.Should().Be(9);
    }

    [Fact]
    public void Recover_ErrorTypeWithValueRecovery_LeavesUnmatchedFailure()
    {
        Result<int> failure = First;
        Func<Error, int> recovery = _ => 1;

        failure.Recover(ErrorType.NotFound, recovery).FirstError.Code.Should().Be("E1");
    }

    [Fact]
    public void RecoverFirst_ResultRecovery_OnlyLooksAtFirstError()
    {
        Result<int> failure = new[] { First, NotFound };
        Result<int> notFoundFirst = new[] { NotFound, First };
        Func<Error, Result<int>> recovery = _ => 7;

        failure.RecoverFirst(ErrorType.NotFound, recovery).Errors.Should().HaveCount(2);
        notFoundFirst.RecoverFirst(ErrorType.NotFound, recovery).Value.Should().Be(7);
        Result<int>.Success(1).RecoverFirst(ErrorType.NotFound, recovery).Value.Should().Be(1);
    }

    [Fact]
    public void RecoverFirst_ValueRecovery_OnlyLooksAtFirstError()
    {
        Result<int> failure = new[] { First, NotFound };
        Func<Error, int> recovery = _ => 7;

        failure.RecoverFirst(ErrorType.NotFound, recovery).FirstError.Code.Should().Be("E1");
        Result<int>.Success(1).RecoverFirst(ErrorType.NotFound, recovery).Value.Should().Be(1);
    }

    [Fact]
    public void Recover_PredicateWithResultRecovery_HandlesMatchNoMatchAndSuccess()
    {
        Result<int> failure = new[] { First, NotFound };
        Func<Error, Result<int>> recovery = _ => 3;

        failure.Recover(e => e.Code == "NF", recovery).Value.Should().Be(3);
        failure.Recover(e => e.Code == "XX", recovery).Errors.Should().HaveCount(2);
        Result<int>.Success(8).Recover(_ => true, recovery).Value.Should().Be(8);
    }

    [Fact]
    public void Recover_PredicateWithValueRecovery_HandlesMatchNoMatchAndSuccess()
    {
        Result<int> failure = new[] { First, NotFound };
        Func<Error, int> recovery = e => e.Code.Length;

        failure.Recover(e => e.Type == ErrorType.NotFound, recovery).Value.Should().Be(2);
        failure.Recover(_ => false, recovery).Errors.Should().HaveCount(2);
        Result<int>.Success(8).Recover(_ => true, recovery).Value.Should().Be(8);
    }

    #endregion

    #region Extensions and collections

    [Fact]
    public void ToResult_FromErrorAndErrors_CreatesFailures()
    {
        First.ToResult().FirstError.Code.Should().Be("E1");
        First.ToResult<int>().FirstError.Code.Should().Be("E1");
        new List<Error> { First, Second }.ToResult<int>().Errors.Should().HaveCount(2);
    }

    [Fact]
    public void Sequence_InterfaceResults_CollectsAllErrors()
    {
        IEnumerable<IResult<int>> source = [Result<int>.Success(1), Result<int>.Failure(First), Result<int>.Failure(Second)];

        source.Sequence().Errors.Select(e => e.Code).Should().Equal("E1", "E2");
    }

    [Fact]
    public void FirstFailureOrSuccesses_Results_StopsAtFirstFailure()
    {
        int visited = 0;
        IEnumerable<Result> failing = new[] { Result.Success(), Result.Failure(First), Result.Failure(Second) }.Select(r => { visited++; return r; });
        IEnumerable<Result> passing = [Result.Success(), Result.Success()];

        failing.FirstFailureOrSuccesses().Errors.Should().ContainSingle().Which.Code.Should().Be("E1");
        visited.Should().Be(2);
        passing.FirstFailureOrSuccesses().IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void FirstFailureOrSuccesses_ResultBases_StopsAtFirstFailure()
    {
        int visited = 0;
        IEnumerable<IResultBase> failing = new IResultBase[] { Result.Success(), Result<int>.Failure(Second), Result.Failure(First) }.Select(r => { visited++; return r; });
        IEnumerable<IResultBase> passing = [Result.Success(), Result<int>.Success(1)];

        failing.FirstFailureOrSuccesses().Errors.Should().ContainSingle().Which.Code.Should().Be("E2");
        visited.Should().Be(2);
        passing.FirstFailureOrSuccesses().IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void FirstFailureOrSuccesses_InterfaceResults_ReturnsValuesOrFirstFailure()
    {
        IEnumerable<IResult<int>> failing = [Result<int>.Success(1), Result<int>.Failure(First), Result<int>.Failure(Second)];
        IEnumerable<IResult<int>> passing = [Result<int>.Success(1), Result<int>.Success(2)];

        failing.FirstFailureOrSuccesses().Errors.Should().ContainSingle().Which.Code.Should().Be("E1");
        passing.FirstFailureOrSuccesses().Value.Should().Equal(1, 2);
    }

    [Fact]
    public async Task TraverseAsync_ReadOnlyCollectionSource_ReturnsAllValues()
    {
        var source = new ReadOnlyOnly<int>(1, 2, 3);
        Func<int, Task<Result<int>>> selector = v => Task.FromResult(Result<int>.Success(v * 10));

        Result<int[]> result = await source.TraverseAsync(selector);

        result.Value.Should().Equal(10, 20, 30);
    }

    [Fact]
    public void InterfaceComparers_CompareByStateAndErrors()
    {
        IResultBase.Comparer.Equals(Result.Failure(First), Result<int>.Failure(First)).Should().BeTrue();
        IResult.Comparer.Equals(Result.Success(), Result.Failure(First)).Should().BeFalse();
        IResult<int>.Comparer.Equals(Result<int>.Success(1), Result<int>.Success(1)).Should().BeTrue();
    }

    [Fact]
    public void ResultUnwrapException_MessageConstructor_KeepsMessageAndErrors()
    {
        var exception = new ResultUnwrapException("custom", [First, Second]);

        exception.Message.Should().Be("custom");
        exception.Errors.Should().Equal(First, Second);
    }

    #endregion
}
