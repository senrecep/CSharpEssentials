using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultFailureIfTests
{
    private static readonly Error TestError = Error.Validation("Test.Code", "Test message");

    #region Result.FailureIf(bool)

    [Fact]
    public void Result_FailureIf_Bool_True_ShouldReturnFailure()
    {
        Result result = Result.FailureIf(true, TestError);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(TestError);
    }

    [Fact]
    public void Result_FailureIf_Bool_False_ShouldReturnSuccess()
    {
        Result result = Result.FailureIf(false, TestError);

        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Result.FailureIf(Func<bool>)

    [Fact]
    public void Result_FailureIf_Func_True_ShouldReturnFailure()
    {
        Result result = Result.FailureIf(() => true, TestError);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(TestError);
    }

    [Fact]
    public void Result_FailureIf_Func_False_ShouldReturnSuccess()
    {
        Result result = Result.FailureIf(() => false, TestError);

        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Result.FailureIf<TValue>

    [Fact]
    public void Result_FailureIfT_Bool_True_ShouldReturnFailure()
    {
        Result<int> result = Result.FailureIf<int>(true, TestError);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(TestError);
    }

    [Fact]
    public void Result_FailureIfT_Bool_False_ShouldReturnSuccessWithDefaultValue()
    {
        Result<int> result = Result.FailureIf<int>(false, TestError);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
    }

    #endregion
}
