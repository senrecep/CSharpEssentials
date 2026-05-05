using System.Globalization;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultChainTests
{
    private static readonly Error TestError = Error.Validation("Test.Code", "Test message");
    private static readonly Error AnotherError = Error.Validation("Another.Code", "Another message");

    #region Bind Chain

    [Fact]
    public void Result_Bind_Chained_ShouldFlowThroughSuccesses()
    {
        Result result = Result.Success()
            .Bind(() => Result.Success())
            .Bind(() => Result.Success());

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Result_Bind_Chained_ShouldStopOnFailure()
    {
        int callCount = 0;

        Result result = Result.Success()
            .Bind(() => { callCount++; return Result.Success(); })
            .Bind(() => { callCount++; return Result.Failure(TestError); })
            .Bind(() => { callCount++; return Result.Success(); });

        result.IsFailure.Should().BeTrue();
        callCount.Should().Be(2);
    }

    [Fact]
    public void ResultT_Bind_Chained_ShouldTransformThroughChain()
    {
        Result<string> result = 10.ToResult()
            .Bind(v => (v * 2).ToResult())
            .Bind(v => v.ToString(System.Globalization.CultureInfo.InvariantCulture).ToResult());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("20");
    }

    [Fact]
    public void ResultT_Bind_Chained_ShouldStopOnFailureAndPreserveError()
    {
        Result<string> result = 10.ToResult()
            .Bind(v => (v * 2).ToResult())
            .Bind(_ => Result<int>.Failure(TestError))
            .Bind(v => v.ToString(System.Globalization.CultureInfo.InvariantCulture).ToResult());

        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(TestError);
    }

    #endregion

    #region Map Chain

    [Fact]
    public void Result_Map_Chained_ShouldTransformThroughChain()
    {
        Result<string> result = Result.Success()
            .Map(() => 10)
            .Map(v => v * 2)
            .Map(v => v.ToString(System.Globalization.CultureInfo.InvariantCulture));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("20");
    }

    [Fact]
    public void ResultT_Map_Chained_ShouldTransformThroughChain()
    {
        Result<string> result = 5.ToResult()
            .Map(v => v * 2)
            .Map(v => v.ToString(System.Globalization.CultureInfo.InvariantCulture));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("10");
    }

    [Fact]
    public void ResultT_Map_Chained_ShouldStopOnFailure()
    {
        int callCount = 0;

        Result<string> result = 5.ToResult()
            .Map(v => { callCount++; return v * 2; })
            .Map(_ => Result<int>.Failure(TestError))
            .Map(v => { callCount++; return v.ToString(System.Globalization.CultureInfo.InvariantCulture); });

        result.IsFailure.Should().BeTrue();
        callCount.Should().Be(1);
    }

    #endregion

    #region Then Chain

    [Fact]
    public void Result_Then_Chained_ShouldFlowThroughSuccesses()
    {
        Result result = Result.Success()
            .Then(() => Result.Success())
            .Then(() => Result.Success());

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Result_Then_Chained_ShouldStopOnFailure()
    {
        int callCount = 0;

        Result result = Result.Success()
            .Then(() => { callCount++; return Result.Success(); })
            .Then(() => { callCount++; return Result.Failure(TestError); })
            .Then(() => { callCount++; return Result.Success(); });

        result.IsFailure.Should().BeTrue();
        callCount.Should().Be(2);
    }

    [Fact]
    public void ResultT_Then_Chained_ShouldTransformThroughChain()
    {
        Result<string> result = 5.ToResult()
            .Then(v => v * 2)
            .Then(v => v.ToString(System.Globalization.CultureInfo.InvariantCulture));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("10");
    }

    #endregion

    #region Tap Chain

    [Fact]
    public void Result_Tap_Chained_ShouldExecuteAllTaps()
    {
        int tapCount = 0;

        Result result = Result.Success()
            .Tap(() => tapCount++)
            .Tap(() => tapCount++)
            .Tap(() => tapCount++);

        result.IsSuccess.Should().BeTrue();
        tapCount.Should().Be(3);
    }

    [Fact]
    public void Result_Tap_Chained_ShouldNotExecuteAfterFailure()
    {
        int tapCount = 0;

        Result result = Result.Failure(TestError)
            .Tap(() => tapCount++)
            .Tap(() => tapCount++);

        result.IsFailure.Should().BeTrue();
        tapCount.Should().Be(0);
    }

    [Fact]
    public void ResultT_Tap_Chained_ShouldExecuteAllTaps()
    {
        int sum = 0;

        Result<int> result = 10.ToResult()
            .Tap(v => sum += v)
            .Tap(v => sum += v);

        result.IsSuccess.Should().BeTrue();
        sum.Should().Be(20);
    }

    #endregion

    #region Else Chain

    [Fact]
    public void Result_Else_Chained_ShouldNotTriggerOnSuccess()
    {
        Result result = Result.Success()
            .Else(AnotherError)
            .Else(errors => AnotherError);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Result_Else_Chained_ShouldTriggerOnFailure()
    {
        Result result = Result.Failure(TestError)
            .Else(AnotherError)
            .Else(errors => errors[0]);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(AnotherError);
    }

    [Fact]
    public void ResultT_Else_Chained_ShouldNotTriggerOnSuccess()
    {
        Result<int> result = 42.ToResult()
            .Else(99)
            .Else(errors => 99);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void ResultT_Else_Chained_ShouldTriggerOnFailure()
    {
        Result<int> result = Result<int>.Failure(TestError)
            .Else(99)
            .Else(errors => errors.Length * 10);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(99);
    }

    #endregion

    #region Match Chain (via Finally)

    [Fact]
    public void Result_Match_Chained_ShouldTransformBasedOnState()
    {
        string result = Result.Success()
            .Then(() => Result.Success())
            .Finally(r => r.Match(() => "ok", _ => "fail"));

        result.Should().Be("ok");
    }

    [Fact]
    public void ResultT_Match_Chained_ShouldTransformBasedOnState()
    {
        string result = 42.ToResult()
            .Then(v => v * 2)
            .Finally(r => r.Match(v => $"value:{v}", _ => "fail"));

        result.Should().Be("value:84");
    }

    #endregion

    #region Switch Chain (via Tap)

    [Fact]
    public void Result_Switch_Chained_ShouldExecuteSideEffects()
    {
        bool successCalled = false;
        bool failureCalled = false;

        Result.Success()
            .Tap(() => successCalled = true)
            .Switch(() => { }, _ => failureCalled = true);

        successCalled.Should().BeTrue();
        failureCalled.Should().BeFalse();
    }

    [Fact]
    public void ResultT_Switch_Chained_ShouldExecuteSideEffects()
    {
        int captured = 0;
        bool failureCalled = false;

        42.ToResult()
            .Tap(v => captured = v)
            .Switch(v => { }, _ => failureCalled = true);

        captured.Should().Be(42);
        failureCalled.Should().BeFalse();
    }

    #endregion

    #region Mixed Chain

    [Fact]
    public void Mixed_Chain_BindMapThenTap_ShouldWorkTogether()
    {
        int tapValue = 0;

        Result<string> result = 5.ToResult()
            .Bind(v => (v + 5).ToResult())
            .Map(v => v * 2)
            .Then(v => v.ToString(System.Globalization.CultureInfo.InvariantCulture).ToResult())
            .Tap(v => tapValue = int.Parse(v, CultureInfo.InvariantCulture));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("20");
        tapValue.Should().Be(20);
    }

    [Fact]
    public void Mixed_Chain_WithFailure_ShouldShortCircuit()
    {
        int tapCount = 0;

        Result<string> result = 5.ToResult()
            .Bind(v => Result<int>.Failure(TestError))
            .Map(v => v * 2)
            .Then(v => v.ToString(System.Globalization.CultureInfo.InvariantCulture).ToResult())
            .Tap(_ => tapCount++);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(TestError);
        tapCount.Should().Be(0);
    }

    [Fact]
    public void Mixed_Chain_FailIfThenElse_ShouldWorkTogether()
    {
        Result<int> result = 150.ToResult()
            .FailIf(v => v > 100, TestError)
            .Else(99);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(99);
    }

    [Fact]
    public void Mixed_Chain_TryCatchThenBind_ShouldWorkTogether()
    {
        Result<string> result = 10.ToResult()
            .TryCatch(v => (v * 2).ToResult())
            .Bind(v => v.ToString(System.Globalization.CultureInfo.InvariantCulture).ToResult());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("20");
    }

    #endregion
}
