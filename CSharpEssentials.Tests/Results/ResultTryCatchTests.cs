using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultTryCatchTests
{
    private static readonly Error TestError = Error.Validation("Test.Code", "Test message");
    private static readonly Error CustomCatchError = Error.Validation("Catch.Code", "Catch error");

    #region Result.TryCatch

    [Fact]
    public void Result_TryCatch_WithSuccess_ShouldExecuteFunction()
    {
        var result = Result.Success();

        Result caught = result.TryCatch(() => Result.Success());

        caught.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Result_TryCatch_WithFailure_ShouldNotExecuteFunction()
    {
        var result = Result.Failure(TestError);
        bool called = false;

        Result caught = result.TryCatch(() => { called = true; return Result.Success(); });

        caught.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void Result_TryCatch_WithException_ShouldReturnFailure()
    {
        var result = Result.Success();

        Result caught = result.TryCatch(() => throw new InvalidOperationException("boom"));

        caught.IsFailure.Should().BeTrue();
        caught.FirstError.Code.Should().Be("InvalidOperationException");
    }

    [Fact]
    public void Result_TryCatch_WithExceptionAndCustomError_ShouldReturnCustomError()
    {
        var result = Result.Success();

        Result caught = result.TryCatch(() => throw new InvalidOperationException("boom"), CustomCatchError);

        caught.IsFailure.Should().BeTrue();
        caught.FirstError.Should().Be(CustomCatchError);
    }

    [Fact]
    public void Result_TryCatch_Generic_WithSuccess_ShouldExecuteFunction()
    {
        var result = Result.Success();

        Result<int> caught = result.TryCatch(() => 42.ToResult());

        caught.IsSuccess.Should().BeTrue();
        caught.Value.Should().Be(42);
    }

    [Fact]
    public void Result_TryCatch_Generic_WithFailure_ShouldNotExecuteFunction()
    {
        var result = Result.Failure(TestError);
        bool called = false;

        Result<int> caught = result.TryCatch(() => { called = true; return 42.ToResult(); });

        caught.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void Result_TryCatch_Generic_WithException_ShouldReturnFailure()
    {
        var result = Result.Success();

        Result<int> caught = result.TryCatch<int>(() => throw new InvalidOperationException("boom"));

        caught.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Result_TryCatch_Generic_WithExceptionAndCustomError_ShouldReturnCustomError()
    {
        var result = Result.Success();

        Result<int> caught = result.TryCatch<int>(() => throw new InvalidOperationException("boom"), CustomCatchError);

        caught.IsFailure.Should().BeTrue();
        caught.FirstError.Should().Be(CustomCatchError);
    }

    [Fact]
    public void Result_TryCatch_Chained_ShouldContinueAfterSuccess()
    {
        Result result = Result.Success()
            .TryCatch(() => Result.Success())
            .TryCatch(() => Result.Failure(TestError))
            .TryCatch(() => Result.Success());

        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(TestError);
    }

    #endregion

    #region Result<T>.TryCatch

    [Fact]
    public void ResultT_TryCatch_ToResult_WithSuccess_ShouldExecuteFunction()
    {
        var result = 10.ToResult();

        Result caught = result.TryCatch(value => value > 5 ? Result.Success() : Result.Failure(TestError));

        caught.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ResultT_TryCatch_ToResult_WithFailure_ShouldNotExecuteFunction()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result caught = result.TryCatch(value => { called = true; return Result.Success(); });

        caught.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void ResultT_TryCatch_ToResult_WithException_ShouldReturnFailure()
    {
        var result = 10.ToResult();

        Result caught = result.TryCatch(_ => throw new InvalidOperationException("boom"));

        caught.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ResultT_TryCatch_ToResult_WithExceptionAndCustomError_ShouldReturnCustomError()
    {
        var result = 10.ToResult();

        Result caught = result.TryCatch(_ => throw new InvalidOperationException("boom"), CustomCatchError);

        caught.IsFailure.Should().BeTrue();
        caught.FirstError.Should().Be(CustomCatchError);
    }

    [Fact]
    public void ResultT_TryCatch_Generic_WithSuccess_ShouldExecuteFunction()
    {
        var result = 10.ToResult();

        Result<string> caught = result.TryCatch(value => value.ToString(System.Globalization.CultureInfo.InvariantCulture).ToResult());

        caught.IsSuccess.Should().BeTrue();
        caught.Value.Should().Be("10");
    }

    [Fact]
    public void ResultT_TryCatch_Generic_WithFailure_ShouldNotExecuteFunction()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<string> caught = result.TryCatch(value =>
        {
            called = true;
            return value.ToString(System.Globalization.CultureInfo.InvariantCulture).ToResult();
        });

        caught.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void ResultT_TryCatch_Generic_WithException_ShouldReturnFailure()
    {
        var result = 10.ToResult();

        Result<string> caught = result.TryCatch<string>(_ => throw new InvalidOperationException("boom"));

        caught.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ResultT_TryCatch_Generic_WithExceptionAndCustomError_ShouldReturnCustomError()
    {
        var result = 10.ToResult();

        Result<string> caught = result.TryCatch<string>(_ => throw new InvalidOperationException("boom"), CustomCatchError);

        caught.IsFailure.Should().BeTrue();
        caught.FirstError.Should().Be(CustomCatchError);
    }

    [Fact]
    public void ResultT_TryCatch_Chained_ShouldCatchInChain()
    {
        Result<string> result = 10.ToResult()
            .TryCatch(v => (v * 2).ToResult())
            .TryCatch(v => v.ToString(System.Globalization.CultureInfo.InvariantCulture).ToResult());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("20");
    }

    #endregion
}
