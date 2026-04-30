using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultAdvancedModulesTests
{
    private static readonly Error TestError = Error.Failure("TEST", "Test error");
    private static readonly Error AlternativeError = Error.Failure("ALT", "Alternative error");

    #region BindIf

    [Fact]
    public void BindIf_WithTrueCondition_ShouldExecuteFunction()
    {
        var result = Result.Success();
        bool functionCalled = false;

        Result bound = result.BindIf(true, () =>
        {
            functionCalled = true;
            return Result.Success();
        });

        bound.IsSuccess.Should().BeTrue();
        functionCalled.Should().BeTrue();
    }

    [Fact]
    public void BindIf_WithFalseCondition_ShouldNotExecuteFunction()
    {
        var result = Result.Success();
        bool functionCalled = false;

        Result bound = result.BindIf(false, () =>
        {
            functionCalled = true;
            return Result.Failure(TestError);
        });

        bound.IsSuccess.Should().BeTrue();
        functionCalled.Should().BeFalse();
    }

    [Fact]
    public void BindIf_WithPredicateTrue_ShouldExecuteFunction()
    {
        var result = Result.Success();
        bool functionCalled = false;

        Result bound = result.BindIf(() => true, () =>
        {
            functionCalled = true;
            return Result.Success();
        });

        bound.IsSuccess.Should().BeTrue();
        functionCalled.Should().BeTrue();
    }

    [Fact]
    public void BindIf_WithPredicateFalse_ShouldNotExecuteFunction()
    {
        var result = Result.Success();
        bool functionCalled = false;

        Result bound = result.BindIf(() => false, () =>
        {
            functionCalled = true;
            return Result.Failure(TestError);
        });

        bound.IsSuccess.Should().BeTrue();
        functionCalled.Should().BeFalse();
    }

    [Fact]
    public void BindIf_WithFailedResult_ShouldNotExecuteFunction()
    {
        var result = Result.Failure(TestError);
        bool functionCalled = false;

        Result bound = result.BindIf(true, () =>
        {
            functionCalled = true;
            return Result.Success();
        });

        bound.IsFailure.Should().BeTrue();
        functionCalled.Should().BeFalse();
    }

    #endregion

    #region Else

    [Fact]
    public void Else_WithSuccess_ShouldReturnOriginal()
    {
        var result = Result.Success();

        Result elseResult = result.Else(AlternativeError);

        elseResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Else_WithFailure_ShouldReturnAlternativeError()
    {
        var result = Result.Failure(TestError);

        Result elseResult = result.Else(AlternativeError);

        elseResult.IsFailure.Should().BeTrue();
        elseResult.FirstError.Code.Should().Be("ALT");
    }

    [Fact]
    public void Else_WithFunc_ShouldTransformErrors()
    {
        var result = Result.Failure(TestError);

        Result elseResult = result.Else(errors => Error.Failure("TRANSFORMED", $"Transformed from {errors.Length} errors"));

        elseResult.IsFailure.Should().BeTrue();
        elseResult.FirstError.Code.Should().Be("TRANSFORMED");
    }

    [Fact]
    public void Else_WithIEnumerableFunc_ShouldReturnMultipleErrors()
    {
        var result = Result.Failure(TestError);

        Result elseResult = result.Else(errors => new[]
        {
            Error.Failure("ERR1", "Error 1"),
            Error.Failure("ERR2", "Error 2")
        });

        elseResult.IsFailure.Should().BeTrue();
        elseResult.Errors.Should().HaveCount(2);
    }

    #endregion

    #region Finally

    [Fact]
    public void Finally_WithSuccess_ShouldExecuteFunction()
    {
        var result = Result.Success();

        string finalResult = result.Finally(r => r.IsSuccess ? "Success" : "Failure");

        finalResult.Should().Be("Success");
    }

    [Fact]
    public void Finally_WithFailure_ShouldExecuteFunction()
    {
        var result = Result.Failure(TestError);

        string finalResult = result.Finally(r => r.IsSuccess ? "Success" : "Failure");

        finalResult.Should().Be("Failure");
    }

    [Fact]
    public void Finally_ShouldAlwaysExecute()
    {
        var successResult = Result.Success();
        var failureResult = Result.Failure(TestError);

        int successCount = successResult.Finally(r => r.IsSuccess ? 1 : 0);
        int failureCount = failureResult.Finally(r => r.IsSuccess ? 1 : 0);

        successCount.Should().Be(1);
        failureCount.Should().Be(0);
    }

    #endregion

    #region Switch

    [Fact]
    public void Switch_WithSuccess_ShouldCallSuccessAction()
    {
        var result = Result.Success();
        bool successCalled = false;
        bool failureCalled = false;

        result.Switch(
            () => successCalled = true,
            _ => failureCalled = true);

        successCalled.Should().BeTrue();
        failureCalled.Should().BeFalse();
    }

    [Fact]
    public void Switch_WithFailure_ShouldCallFailureAction()
    {
        var result = Result.Failure(TestError);
        bool successCalled = false;
        bool failureCalled = false;
        Error[]? capturedErrors = null;

        result.Switch(
            () => successCalled = true,
            errors =>
            {
                failureCalled = true;
                capturedErrors = errors;
            });

        successCalled.Should().BeFalse();
        failureCalled.Should().BeTrue();
        capturedErrors.Should().HaveCount(1);
    }

    [Fact]
    public void SwitchFirst_WithFailure_ShouldProvideFirstError()
    {
        var error1 = Error.Failure("ERR1", "First error");
        var error2 = Error.Failure("ERR2", "Second error");
        var result = Result.Failure(error1, error2);
        Error? capturedError = null;

        result.SwitchFirst(
            () => { },
            error => capturedError = error);

        capturedError.Should().NotBeNull();
        capturedError!.Value.Code.Should().Be("ERR1");
    }

    [Fact]
    public void SwitchLast_WithFailure_ShouldProvideLastError()
    {
        var error1 = Error.Failure("ERR1", "First error");
        var error2 = Error.Failure("ERR2", "Second error");
        var result = Result.Failure(error1, error2);
        Error? capturedError = null;

        result.SwitchLast(
            () => { },
            error => capturedError = error);

        capturedError.Should().NotBeNull();
        capturedError!.Value.Code.Should().Be("ERR2");
    }

    #endregion

    #region Then

    [Fact]
    public void Then_WithSuccess_ShouldExecuteFunction()
    {
        var result = Result.Success();
        bool functionCalled = false;

        Result thenResult = result.Then(() =>
        {
            functionCalled = true;
            return Result.Success();
        });

        thenResult.IsSuccess.Should().BeTrue();
        functionCalled.Should().BeTrue();
    }

    [Fact]
    public void Then_WithFailure_ShouldNotExecuteFunction()
    {
        var result = Result.Failure(TestError);
        bool functionCalled = false;

        Result thenResult = result.Then(() =>
        {
            functionCalled = true;
            return Result.Success();
        });

        thenResult.IsFailure.Should().BeTrue();
        functionCalled.Should().BeFalse();
    }

    [Fact]
    public void ThenDo_WithSuccess_ShouldExecuteAction()
    {
        var result = Result.Success();
        bool actionCalled = false;

        Result thenResult = result.ThenDo(() => actionCalled = true);

        thenResult.IsSuccess.Should().BeTrue();
        actionCalled.Should().BeTrue();
    }

    [Fact]
    public void ThenDo_WithFailure_ShouldNotExecuteAction()
    {
        var result = Result.Failure(TestError);
        bool actionCalled = false;

        Result thenResult = result.ThenDo(() => actionCalled = true);

        thenResult.IsFailure.Should().BeTrue();
        actionCalled.Should().BeFalse();
    }

    [Fact]
    public void Then_ChainedCalls_ShouldStopOnFirstFailure()
    {
        int callCount = 0;

        Result result = Result.Success()
            .Then(() => { callCount++; return Result.Success(); })
            .Then(() => { callCount++; return Result.Failure(TestError); })
            .Then(() => { callCount++; return Result.Success(); });

        result.IsFailure.Should().BeTrue();
        callCount.Should().Be(2);
    }

    #endregion
}

