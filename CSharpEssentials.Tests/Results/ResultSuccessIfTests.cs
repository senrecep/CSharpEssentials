using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultSuccessIfTests
{
    private static readonly Error TestError = Error.Validation("Test.Code", "Test message");

    #region Result.SuccessIf(bool)

    [Fact]
    public void Result_SuccessIf_Bool_True_ShouldReturnSuccess()
    {
        var result = Result.SuccessIf(true, TestError);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Result_SuccessIf_Bool_False_ShouldReturnFailure()
    {
        var result = Result.SuccessIf(false, TestError);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(TestError);
    }

    #endregion

    #region Result.SuccessIf(Func<bool>)

    [Fact]
    public void Result_SuccessIf_Func_True_ShouldReturnSuccess()
    {
        var result = Result.SuccessIf(() => true, TestError);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Result_SuccessIf_Func_False_ShouldReturnFailure()
    {
        var result = Result.SuccessIf(() => false, TestError);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(TestError);
    }

    #endregion

    #region Result.SuccessIf<TValue>

    [Fact]
    public void Result_SuccessIfT_Bool_True_ShouldReturnSuccessWithValue()
    {
        var result = Result.SuccessIf(true, 42, TestError);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Result_SuccessIfT_Bool_False_ShouldReturnFailure()
    {
        var result = Result.SuccessIf(false, 42, TestError);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(TestError);
    }

    #endregion
}
