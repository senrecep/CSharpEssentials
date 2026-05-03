using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultFinallyTests
{
    private static readonly Error TestError = Error.Validation("Test.Code", "Test message");

    #region Result.Finally

    [Fact]
    public void Result_Finally_WithSuccess_ShouldExecuteFunction()
    {
        var result = Result.Success();

        string final = result.Finally(r => r.IsSuccess ? "ok" : "fail");

        final.Should().Be("ok");
    }

    [Fact]
    public void Result_Finally_WithFailure_ShouldExecuteFunction()
    {
        var result = Result.Failure(TestError);

        string final = result.Finally(r => r.IsSuccess ? "ok" : "fail");

        final.Should().Be("fail");
    }

    [Fact]
    public void Result_Finally_ShouldAlwaysExecuteRegardlessOfState()
    {
        var success = Result.Success();
        var failure = Result.Failure(TestError);

        int s = success.Finally(r => r.IsSuccess ? 1 : 0);
        int f = failure.Finally(r => r.IsSuccess ? 1 : 0);

        s.Should().Be(1);
        f.Should().Be(0);
    }

    [Fact]
    public void Result_Finally_Chained_ShouldTransformToAnyType()
    {
        string final = Result.Success()
            .Then(() => Result.Success())
            .Finally(r => r.IsSuccess ? "chained-ok" : "chained-fail");

        final.Should().Be("chained-ok");
    }

    #endregion

    #region Result<T>.Finally

    [Fact]
    public void ResultT_Finally_WithSuccess_ShouldExecuteFunction()
    {
        var result = Result<int>.Success(42);

        string final = result.Finally(r => r.IsSuccess ? $"value:{r.Value}" : "fail");

        final.Should().Be("value:42");
    }

    [Fact]
    public void ResultT_Finally_WithFailure_ShouldExecuteFunction()
    {
        var result = Result<int>.Failure(TestError);

        string final = result.Finally(r => r.IsSuccess ? $"value:{r.Value}" : $"errors:{r.Errors.Length}");

        final.Should().Be("errors:1");
    }

    [Fact]
    public void ResultT_Finally_ShouldTransformToAnyType()
    {
        var result = Result<string>.Success("hello");

        int length = result.Finally(r => r.IsSuccess ? r.Value.Length : 0);

        length.Should().Be(5);
    }

    [Fact]
    public void ResultT_Finally_Chained_ShouldWorkAfterOperations()
    {
        int final = Result<int>.Success(10)
            .Then(v => v * 2)
            .Finally(r => r.IsSuccess ? r.Value : 0);

        final.Should().Be(20);
    }

    #endregion
}
