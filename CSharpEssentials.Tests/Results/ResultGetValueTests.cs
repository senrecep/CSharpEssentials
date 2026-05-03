using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultGetValueTests
{
    private static readonly Error TestError = Error.Validation("Test.Code", "Test message");

    #region GetValueOrDefault

    [Fact]
    public void ResultT_GetValueOrDefault_WithSuccess_ShouldReturnValue()
    {
        var result = Result<int>.Success(42);

        int value = result.GetValueOrDefault();

        value.Should().Be(42);
    }

    [Fact]
    public void ResultT_GetValueOrDefault_WithFailure_ShouldReturnDefault()
    {
        var result = Result<int>.Failure(TestError);

        int value = result.GetValueOrDefault();

        value.Should().Be(0);
    }

    [Fact]
    public void ResultT_GetValueOrDefault_ReferenceType_WithFailure_ShouldReturnNull()
    {
        var result = Result<string>.Failure(TestError);

        string? value = result.GetValueOrDefault();

        value.Should().BeNull();
    }

    [Fact]
    public void ResultT_GetValueOrDefault_WithDefaultValue_Success_ShouldReturnValue()
    {
        var result = Result<int>.Success(42);

        int value = result.GetValueOrDefault(99);

        value.Should().Be(42);
    }

    [Fact]
    public void ResultT_GetValueOrDefault_WithDefaultValue_Failure_ShouldReturnDefaultValue()
    {
        var result = Result<int>.Failure(TestError);

        int value = result.GetValueOrDefault(99);

        value.Should().Be(99);
    }

    [Fact]
    public void ResultT_GetValueOrDefault_WithDefaultValue_ReferenceType_Failure_ShouldReturnDefaultValue()
    {
        var result = Result<string>.Failure(TestError);

        string value = result.GetValueOrDefault("fallback");

        value.Should().Be("fallback");
    }

    #endregion

    #region GetValueOrThrow

    [Fact]
    public void ResultT_GetValueOrThrow_WithSuccess_ShouldReturnValue()
    {
        var result = Result<int>.Success(42);

        int value = result.GetValueOrThrow();

        value.Should().Be(42);
    }

    [Fact]
    public void ResultT_GetValueOrThrow_WithFailure_ShouldThrowInvalidOperationException()
    {
        var result = Result<int>.Failure(TestError);

        Action action = () => result.GetValueOrThrow();

        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ResultT_GetValueOrThrow_WithFailureAndCustomMessage_ShouldThrowWithMessage()
    {
        var result = Result<int>.Failure(TestError);

        Action action = () => result.GetValueOrThrow("Custom error message");

        action.Should().Throw<InvalidOperationException>().WithMessage("Custom error message");
    }

    [Fact]
    public void ResultT_GetValueOrThrow_WithFailureAndException_ShouldThrowThatException()
    {
        var result = Result<int>.Failure(TestError);
        var customException = new ApplicationException("My exception");

        Action action = () => result.GetValueOrThrow(customException);

        action.Should().Throw<ApplicationException>().WithMessage("My exception");
    }

    [Fact]
    public void ResultT_GetValueOrThrow_Chained_ShouldThrowWhenFailed()
    {
        var result = Result<int>.Failure(TestError);

        Action action = () => result.Then(v => v * 2).GetValueOrThrow();

        action.Should().Throw<InvalidOperationException>();
    }

    #endregion
}
