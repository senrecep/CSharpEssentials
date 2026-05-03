using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultDeconstructTests
{
    private static readonly Error TestError = Error.Validation("Test.Code", "Test message");

    #region Result.Deconstruct

    [Fact]
    public void Result_Deconstruct_WithSuccess_ShouldReturnIsSuccessTrueAndEmptyErrors()
    {
        var result = Result.Success();

        result.Deconstruct(out bool isSuccess, out Error[] errors);

        isSuccess.Should().BeTrue();
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Result_Deconstruct_WithFailure_ShouldReturnIsSuccessFalseAndErrors()
    {
        var result = Result.Failure(TestError);

        result.Deconstruct(out bool isSuccess, out Error[] errors);

        isSuccess.Should().BeFalse();
        errors.Should().ContainSingle().Which.Should().Be(TestError);
    }

    #endregion

    #region Result<T>.Deconstruct

    [Fact]
    public void ResultT_Deconstruct_WithSuccess_ShouldReturnIsSuccessTrueValueAndEmptyErrors()
    {
        var result = Result<int>.Success(42);

        result.Deconstruct(out bool isSuccess, out int value, out Error[] errors);

        isSuccess.Should().BeTrue();
        value.Should().Be(42);
        errors.Should().BeEmpty();
    }

    [Fact]
    public void ResultT_Deconstruct_WithFailure_ShouldReturnIsSuccessFalseDefaultValueAndErrors()
    {
        var result = Result<int>.Failure(TestError);

        result.Deconstruct(out bool isSuccess, out int value, out Error[] errors);

        isSuccess.Should().BeFalse();
        value.Should().Be(0);
        errors.Should().ContainSingle().Which.Should().Be(TestError);
    }

    #endregion
}
