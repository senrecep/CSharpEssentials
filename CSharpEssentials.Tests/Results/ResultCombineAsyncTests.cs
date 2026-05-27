using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultCombineAsyncTests
{
    private static readonly Error ErrorA = Error.Failure("ERR_A", "Error A");
    private static readonly Error ErrorB = Error.Validation("ERR_B", "Error B");
    private static readonly Error ErrorC = Error.NotFound("ERR_C", "Error C");

    #region Combine two async results

    [Fact]
    public async Task CombineAsync_Should_ReturnTuple_When_BothTasksSucceed()
    {
        Result<int> first = await Task.FromResult(42.ToResult());
        Result<string> second = await Task.FromResult("hello".ToResult());

        Result<(int, string)> combined = Result<int>.Combine(first, second);

        combined.IsSuccess.Should().BeTrue();
        combined.Value.Item1.Should().Be(42);
        combined.Value.Item2.Should().Be("hello");
    }

    [Fact]
    public async Task CombineAsync_Should_ReturnFailure_When_FirstTaskFails()
    {
        Result<int> first = await Task.FromResult(Result<int>.Failure(ErrorA));
        Result<string> second = await Task.FromResult("hello".ToResult());

        Result<(int, string)> combined = Result<int>.Combine(first, second);

        combined.IsFailure.Should().BeTrue();
        combined.Errors.Should().ContainSingle(e => e.Code == "ERR_A");
    }

    [Fact]
    public async Task CombineAsync_Should_ReturnFailure_When_SecondTaskFails()
    {
        Result<int> first = await Task.FromResult(42.ToResult());
        Result<string> second = await Task.FromResult(Result<string>.Failure(ErrorB));

        Result<(int, string)> combined = Result<int>.Combine(first, second);

        combined.IsFailure.Should().BeTrue();
        combined.Errors.Should().ContainSingle(e => e.Code == "ERR_B");
    }

    [Fact]
    public async Task CombineAsync_Should_AggregateAllErrors_When_BothTasksFail()
    {
        Result<int> first = await Task.FromResult(Result<int>.Failure(ErrorA));
        Result<string> second = await Task.FromResult(Result<string>.Failure(ErrorB));

        Result<(int, string)> combined = Result<int>.Combine(first, second);

        combined.IsFailure.Should().BeTrue();
        combined.Errors.Should().HaveCount(2);
        combined.Errors.Should().Contain(e => e.Code == "ERR_A");
        combined.Errors.Should().Contain(e => e.Code == "ERR_B");
    }

    #endregion

    #region Combine three async results

    [Fact]
    public async Task CombineAsync_Should_ReturnTuple_When_AllThreeTasksSucceed()
    {
        Result<int> first = await Task.FromResult(1.ToResult());
        Result<string> second = await Task.FromResult("two".ToResult());
        Result<bool> third = await Task.FromResult(true.ToResult());

        Result<(int, string, bool)> combined = Result<int>.Combine(first, second, third);

        combined.IsSuccess.Should().BeTrue();
        combined.Value.Item1.Should().Be(1);
        combined.Value.Item2.Should().Be("two");
        combined.Value.Item3.Should().BeTrue();
    }

    [Fact]
    public async Task CombineAsync_Should_ReturnFailure_When_OneOfThreeTasksFails()
    {
        Result<int> first = await Task.FromResult(1.ToResult());
        Result<string> second = await Task.FromResult(Result<string>.Failure(ErrorB));
        Result<bool> third = await Task.FromResult(true.ToResult());

        Result<(int, string, bool)> combined = Result<int>.Combine(first, second, third);

        combined.IsFailure.Should().BeTrue();
        combined.Errors.Should().ContainSingle(e => e.Code == "ERR_B");
    }

    [Fact]
    public async Task CombineAsync_Should_AggregateAllErrors_When_AllThreeTasksFail()
    {
        Result<int> first = await Task.FromResult(Result<int>.Failure(ErrorA));
        Result<string> second = await Task.FromResult(Result<string>.Failure(ErrorB));
        Result<bool> third = await Task.FromResult(Result<bool>.Failure(ErrorC));

        Result<(int, string, bool)> combined = Result<int>.Combine(first, second, third);

        combined.IsFailure.Should().BeTrue();
        combined.Errors.Should().HaveCount(3);
        combined.Errors.Should().Contain(e => e.Code == "ERR_A");
        combined.Errors.Should().Contain(e => e.Code == "ERR_B");
        combined.Errors.Should().Contain(e => e.Code == "ERR_C");
    }

    #endregion

    #region Combine via Task.WhenAll pattern

    [Fact]
    public async Task CombineAsync_Should_ReturnTuple_When_ParallelTasksSucceed()
    {
        Task<Result<int>> taskA = Task.Run(() => 10.ToResult());
        Task<Result<string>> taskB = Task.Run(() => "world".ToResult());

        Result<int> first = await taskA;
        Result<string> second = await taskB;

        Result<(int, string)> combined = Result<int>.Combine(first, second);

        combined.IsSuccess.Should().BeTrue();
        combined.Value.Should().Be((10, "world"));
    }

    [Fact]
    public async Task CombineAsync_Should_ReturnFailure_When_OneParallelTaskFails()
    {
        Task<Result<int>> taskA = Task.Run(() => Result<int>.Failure(ErrorA));
        Task<Result<string>> taskB = Task.Run(() => "world".ToResult());

        Result<int> first = await taskA;
        Result<string> second = await taskB;

        Result<(int, string)> combined = Result<int>.Combine(first, second);

        combined.IsFailure.Should().BeTrue();
        combined.FirstError.Code.Should().Be("ERR_A");
    }

    #endregion

    #region Combine four async results

    [Fact]
    public async Task CombineAsync_Should_ReturnTuple_When_AllFourTasksSucceed()
    {
        Result<int> first = await Task.FromResult(1.ToResult());
        Result<string> second = await Task.FromResult("two".ToResult());
        Result<bool> third = await Task.FromResult(true.ToResult());
        Result<double> fourth = await Task.FromResult(3.14.ToResult());

        Result<(int, string, bool, double)> combined = Result<int>.Combine(first, second, third, fourth);

        combined.IsSuccess.Should().BeTrue();
        combined.Value.Item1.Should().Be(1);
        combined.Value.Item2.Should().Be("two");
        combined.Value.Item3.Should().BeTrue();
        combined.Value.Item4.Should().Be(3.14);
    }

    [Fact]
    public async Task CombineAsync_Should_ReturnFailure_When_OneOfFourTasksFails()
    {
        Result<int> first = await Task.FromResult(1.ToResult());
        Result<string> second = await Task.FromResult("two".ToResult());
        Result<bool> third = await Task.FromResult(Result<bool>.Failure(ErrorC));
        Result<double> fourth = await Task.FromResult(3.14.ToResult());

        Result<(int, string, bool, double)> combined = Result<int>.Combine(first, second, third, fourth);

        combined.IsFailure.Should().BeTrue();
        combined.Errors.Should().ContainSingle(e => e.Code == "ERR_C");
    }

    #endregion
}
