using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultTAdvancedModulesTests
{
    private static readonly Error TestError = Error.Failure("TEST", "Test error");
    private static readonly Error AlternativeError = Error.Failure("ALT", "Alternative error");

    #region FailIf

    [Fact]
    public void FailIf_WithConditionTrue_ShouldReturnFailure()
    {
        var result = Result<int>.Success(10);

        Result<int> failIfResult = result.FailIf(x => x > 5, TestError);

        failIfResult.IsFailure.Should().BeTrue();
        failIfResult.FirstError.Code.Should().Be("TEST");
    }

    [Fact]
    public void FailIf_WithConditionFalse_ShouldReturnOriginal()
    {
        var result = Result<int>.Success(3);

        Result<int> failIfResult = result.FailIf(x => x > 5, TestError);

        failIfResult.IsSuccess.Should().BeTrue();
        failIfResult.Value.Should().Be(3);
    }

    [Fact]
    public void FailIf_WithAlreadyFailed_ShouldReturnOriginalFailure()
    {
        var result = Result<int>.Failure(TestError);

        Result<int> failIfResult = result.FailIf(x => x > 5, AlternativeError);

        failIfResult.IsFailure.Should().BeTrue();
        failIfResult.FirstError.Code.Should().Be("TEST");
    }

    [Fact]
    public void FailIf_WithErrorFunc_ShouldCreateErrorBasedOnValue()
    {
        var result = Result<int>.Success(10);

        Result<int> failIfResult = result.FailIf(
            x => x > 5,
            x => Error.Failure("VALUE_ERROR", $"Value {x} is too large"));

        failIfResult.IsFailure.Should().BeTrue();
        failIfResult.FirstError.Description.Should().Contain("10");
    }

    #endregion

    #region Else

    [Fact]
    public void Else_WithSuccess_ShouldReturnOriginal()
    {
        var result = Result<int>.Success(42);

        Result<int> elseResult = result.Else(AlternativeError);

        elseResult.IsSuccess.Should().BeTrue();
        elseResult.Value.Should().Be(42);
    }

    [Fact]
    public void Else_WithFailure_ShouldReturnAlternativeError()
    {
        var result = Result<int>.Failure(TestError);

        Result<int> elseResult = result.Else(AlternativeError);

        elseResult.IsFailure.Should().BeTrue();
        elseResult.FirstError.Code.Should().Be("ALT");
    }

    [Fact]
    public void Else_WithValue_ShouldReturnDefaultValue()
    {
        var result = Result<int>.Failure(TestError);

        Result<int> elseResult = result.Else(99);

        elseResult.IsSuccess.Should().BeTrue();
        elseResult.Value.Should().Be(99);
    }

    [Fact]
    public void Else_WithValueFunc_ShouldComputeDefaultValue()
    {
        var result = Result<int>.Failure(TestError);

        Result<int> elseResult = result.Else(errors => errors.Length * 10);

        elseResult.IsSuccess.Should().BeTrue();
        elseResult.Value.Should().Be(10);
    }

    [Fact]
    public void Else_WithErrorFunc_ShouldTransformErrors()
    {
        var result = Result<int>.Failure(TestError);

        Result<int> elseResult = result.Else(errors => Error.Failure("TRANSFORMED", $"From {errors.Length} errors"));

        elseResult.IsFailure.Should().BeTrue();
        elseResult.FirstError.Code.Should().Be("TRANSFORMED");
    }

    #endregion

    #region Finally

    [Fact]
    public void Finally_WithSuccess_ShouldExecuteFunction()
    {
        var result = Result<int>.Success(42);

        string finalResult = result.Finally(r => r.IsSuccess ? $"Value: {r.Value}" : "Failed");

        finalResult.Should().Be("Value: 42");
    }

    [Fact]
    public void Finally_WithFailure_ShouldExecuteFunction()
    {
        var result = Result<int>.Failure(TestError);

        string finalResult = result.Finally(r => r.IsSuccess ? "Success" : $"Failed with {r.Errors.Length} errors");

        finalResult.Should().Be("Failed with 1 errors");
    }

    [Fact]
    public void Finally_ShouldTransformResultToAnyType()
    {
        var result = Result<string>.Success("Hello");

        int length = result.Finally(r => r.IsSuccess ? r.Value.Length : 0);

        length.Should().Be(5);
    }

    #endregion

    #region Switch

    [Fact]
    public void Switch_WithSuccess_ShouldCallSuccessAction()
    {
        var result = Result<int>.Success(42);
        int? capturedValue = null;
        bool failureCalled = false;

        result.Switch(
            value => capturedValue = value,
            _ => failureCalled = true);

        capturedValue.Should().Be(42);
        failureCalled.Should().BeFalse();
    }

    [Fact]
    public void Switch_WithFailure_ShouldCallFailureAction()
    {
        var result = Result<int>.Failure(TestError);
        bool successCalled = false;
        Error[]? capturedErrors = null;

        result.Switch(
            _ => successCalled = true,
            errors => capturedErrors = errors);

        successCalled.Should().BeFalse();
        capturedErrors.Should().HaveCount(1);
    }

    [Fact]
    public void SwitchFirst_WithMultipleErrors_ShouldProvideFirstError()
    {
        var error1 = Error.Failure("ERR1", "First");
        var error2 = Error.Failure("ERR2", "Second");
        var result = Result<int>.Failure(error1, error2);
        Error? capturedError = null;

        result.SwitchFirst(
            _ => { },
            error => capturedError = error);

        capturedError.Should().NotBeNull();
        capturedError!.Value.Code.Should().Be("ERR1");
    }

    [Fact]
    public void SwitchLast_WithMultipleErrors_ShouldProvideLastError()
    {
        var error1 = Error.Failure("ERR1", "First");
        var error2 = Error.Failure("ERR2", "Second");
        var result = Result<int>.Failure(error1, error2);
        Error? capturedError = null;

        result.SwitchLast(
            _ => { },
            error => capturedError = error);

        capturedError.Should().NotBeNull();
        capturedError!.Value.Code.Should().Be("ERR2");
    }

    #endregion

    #region Then

    [Fact]
    public void Then_WithSuccess_ShouldExecuteFunction()
    {
        var result = Result<int>.Success(10);

        Result<string> thenResult = result.Then(value => Result<string>.Success(value.ToString(System.Globalization.CultureInfo.InvariantCulture)));

        thenResult.IsSuccess.Should().BeTrue();
        thenResult.Value.Should().Be("10");
    }

    [Fact]
    public void Then_WithFailure_ShouldNotExecuteFunction()
    {
        var result = Result<int>.Failure(TestError);
        bool functionCalled = false;

        Result<string> thenResult = result.Then(value =>
        {
            functionCalled = true;
            return Result<string>.Success(value.ToString(System.Globalization.CultureInfo.InvariantCulture));
        });

        thenResult.IsFailure.Should().BeTrue();
        functionCalled.Should().BeFalse();
    }

    [Fact]
    public void ThenDo_WithSuccess_ShouldExecuteAction()
    {
        var result = Result<int>.Success(42);
        int capturedValue = 0;

        Result<int> thenResult = result.ThenDo(value => capturedValue = value);

        thenResult.IsSuccess.Should().BeTrue();
        capturedValue.Should().Be(42);
    }

    [Fact]
    public void ThenDo_WithFailure_ShouldNotExecuteAction()
    {
        var result = Result<int>.Failure(TestError);
        bool actionCalled = false;

        Result<int> thenResult = result.ThenDo(_ => actionCalled = true);

        thenResult.IsFailure.Should().BeTrue();
        actionCalled.Should().BeFalse();
    }

    #endregion

    #region BindIf

    [Fact]
    public void BindIf_WithTrueCondition_ShouldExecuteFunction()
    {
        var result = Result<int>.Success(10);

        Result<int> bound = result.BindIf(true, x => Result<int>.Success(x * 2));

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be(20);
    }

    [Fact]
    public void BindIf_WithFalseCondition_ShouldReturnOriginal()
    {
        var result = Result<int>.Success(10);

        Result<int> bound = result.BindIf(false, x => Result<int>.Success(x * 2));

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be(10);
    }

    [Fact]
    public void BindIf_WithFailedResult_ShouldReturnOriginalFailure()
    {
        var result = Result<int>.Failure(TestError);

        Result<int> bound = result.BindIf(true, x => Result<int>.Success(x * 2));

        bound.IsFailure.Should().BeTrue();
    }

    #endregion
}

