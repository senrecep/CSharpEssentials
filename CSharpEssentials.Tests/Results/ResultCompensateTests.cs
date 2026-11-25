using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.ResultPattern.Testing;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultCompensateTests
{
    private static readonly Error TestError = Error.Validation("Test.Code", "Test message");
    private static readonly Error AnotherError = Error.Failure("Another.Code", "Another message");

    #region Result.Compensate

    [Fact]
    public void Result_Compensate_ErrorsFunc_WithSuccess_ShouldReturnOriginal()
    {
        var result = Result.Success();

        Result compensated = result.Compensate((Error[] errors) => Result.Failure(AnotherError));

        compensated.ShouldBeSuccess();
    }

    [Fact]
    public void Result_Compensate_ErrorsFunc_WithFailure_ShouldCompensate()
    {
        var result = Result.Failure(TestError);

        Result compensated = result.Compensate((Error[] errors) => Result.Success());

        compensated.ShouldBeSuccess();
    }

    [Fact]
    public void Result_Compensate_FirstErrorFunc_WithSuccess_ShouldReturnOriginal()
    {
        var result = Result.Success();

        Result compensated = result.CompensateFirst(error => Result.Failure(AnotherError));

        compensated.ShouldBeSuccess();
    }

    [Fact]
    public void Result_Compensate_FirstErrorFunc_WithFailure_ShouldCompensate()
    {
        var result = Result.Failure(TestError);

        Result compensated = result.CompensateFirst(error => Result.Success());

        compensated.ShouldBeSuccess();
    }

    #endregion

    #region Result<T>.Compensate

    [Fact]
    public void ResultT_Compensate_ErrorsFunc_WithSuccess_ShouldReturnOriginal()
    {
        var result = 42.ToResult();

        Result<int> compensated = result.Compensate((Error[] errors) => Result<int>.Failure(AnotherError));

        compensated.ShouldBeSuccess().Should().Be(42);
    }

    [Fact]
    public void ResultT_Compensate_ErrorsFunc_WithFailure_ShouldCompensate()
    {
        var result = Result<int>.Failure(TestError);

        Result<int> compensated = result.Compensate((Error[] errors) => 99.ToResult());

        compensated.ShouldBeSuccess().Should().Be(99);
    }

    [Fact]
    public void ResultT_Compensate_FirstErrorFunc_WithSuccess_ShouldReturnOriginal()
    {
        var result = 42.ToResult();

        Result<int> compensated = result.CompensateFirst(error => Result<int>.Failure(AnotherError));

        compensated.ShouldBeSuccess().Should().Be(42);
    }

    [Fact]
    public void ResultT_Compensate_FirstErrorFunc_WithFailure_ShouldCompensate()
    {
        var result = Result<int>.Failure(TestError);

        Result<int> compensated = result.CompensateFirst(error => 99.ToResult());

        compensated.ShouldBeSuccess().Should().Be(99);
    }

    #endregion

    #region Result.CompensateAsync

    [Fact]
    public async Task Result_CompensateAsync_ErrorsFunc_WithSuccess_ShouldReturnOriginal()
    {
        var result = Result.Success();

        Result compensated = await result.CompensateAsync((Error[] errors) => Task.FromResult(Result.Failure(AnotherError)));

        compensated.ShouldBeSuccess();
    }

    [Fact]
    public async Task Result_CompensateAsync_ErrorsFunc_WithFailure_ShouldCompensate()
    {
        var result = Result.Failure(TestError);

        Result compensated = await result.CompensateAsync((Error[] errors) => Task.FromResult(Result.Success()));

        compensated.ShouldBeSuccess();
    }

    [Fact]
    public async Task Result_CompensateAsync_FirstErrorFunc_WithSuccess_ShouldReturnOriginal()
    {
        var result = Result.Success();

        Result compensated = await result.CompensateFirstAsync(error => Task.FromResult(Result.Failure(AnotherError)));

        compensated.ShouldBeSuccess();
    }

    [Fact]
    public async Task Result_CompensateAsync_FirstErrorFunc_WithFailure_ShouldCompensate()
    {
        var result = Result.Failure(TestError);

        Result compensated = await result.CompensateFirstAsync(error => Task.FromResult(Result.Success()));

        compensated.ShouldBeSuccess();
    }

    #endregion

    #region Result<T>.CompensateAsync

    [Fact]
    public async Task ResultT_CompensateAsync_ErrorsFunc_WithSuccess_ShouldReturnOriginal()
    {
        var result = 42.ToResult();

        Result<int> compensated = await result.CompensateAsync((Error[] errors) => Task.FromResult(Result<int>.Failure(AnotherError)));

        compensated.ShouldBeSuccess().Should().Be(42);
    }

    [Fact]
    public async Task ResultT_CompensateAsync_ErrorsFunc_WithFailure_ShouldCompensate()
    {
        var result = Result<int>.Failure(TestError);

        Result<int> compensated = await result.CompensateAsync((Error[] errors) => Task.FromResult(99.ToResult()));

        compensated.ShouldBeSuccess().Should().Be(99);
    }

    [Fact]
    public async Task ResultT_CompensateAsync_FirstErrorFunc_WithSuccess_ShouldReturnOriginal()
    {
        var result = 42.ToResult();

        Result<int> compensated = await result.CompensateFirstAsync(error => Task.FromResult(Result<int>.Failure(AnotherError)));

        compensated.ShouldBeSuccess().Should().Be(42);
    }

    [Fact]
    public async Task ResultT_CompensateAsync_FirstErrorFunc_WithFailure_ShouldCompensate()
    {
        var result = Result<int>.Failure(TestError);

        Result<int> compensated = await result.CompensateFirstAsync(error => Task.FromResult(99.ToResult()));

        compensated.ShouldBeSuccess().Should().Be(99);
    }

    #endregion

    #region CompensateAsync Extensions

    [Fact]
    public async Task Result_CompensateAsync_Task_ErrorsFunc_WithFailure_ShouldCompensate()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Failure(TestError));
#pragma warning restore IDE0008

        Result compensated = await task.CompensateAsync((Error[] errors) => Task.FromResult(Result.Success()));

        compensated.ShouldBeSuccess();
    }

    [Fact]
    public async Task Result_CompensateAsync_Task_FirstErrorFunc_WithFailure_ShouldCompensate()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Failure(TestError));
#pragma warning restore IDE0008

        Result compensated = await task.CompensateFirstAsync(error => Task.FromResult(Result.Success()));

        compensated.ShouldBeSuccess();
    }

    [Fact]
    public async Task Result_CompensateAsync_ValueTask_ErrorsFunc_WithFailure_ShouldCompensate()
    {
#pragma warning disable IDE0008
        var valueTask = ValueTask.FromResult(Result.Failure(TestError));
#pragma warning restore IDE0008

        Result compensated = await valueTask.CompensateAsync((Error[] errors) => Task.FromResult(Result.Success()));

        compensated.ShouldBeSuccess();
    }

    [Fact]
    public async Task Result_CompensateAsync_ValueTask_FirstErrorFunc_WithFailure_ShouldCompensate()
    {
#pragma warning disable IDE0008
        var valueTask = ValueTask.FromResult(Result.Failure(TestError));
#pragma warning restore IDE0008

        Result compensated = await valueTask.CompensateFirstAsync(error => Task.FromResult(Result.Success()));

        compensated.ShouldBeSuccess();
    }

    [Fact]
    public async Task ResultT_CompensateAsync_Task_ErrorsFunc_WithFailure_ShouldCompensate()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008

        Result<int> compensated = await task.CompensateAsync((Error[] errors) => Task.FromResult(99.ToResult()));

        compensated.ShouldBeSuccess().Should().Be(99);
    }

    [Fact]
    public async Task ResultT_CompensateAsync_Task_FirstErrorFunc_WithFailure_ShouldCompensate()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008

        Result<int> compensated = await task.CompensateFirstAsync(error => Task.FromResult(99.ToResult()));

        compensated.ShouldBeSuccess().Should().Be(99);
    }

    [Fact]
    public async Task ResultT_CompensateAsync_ValueTask_ErrorsFunc_WithFailure_ShouldCompensate()
    {
#pragma warning disable IDE0008
        var valueTask = ValueTask.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008

        Result<int> compensated = await valueTask.CompensateAsync((Error[] errors) => Task.FromResult(99.ToResult()));

        compensated.ShouldBeSuccess().Should().Be(99);
    }

    [Fact]
    public async Task ResultT_CompensateAsync_ValueTask_FirstErrorFunc_WithFailure_ShouldCompensate()
    {
#pragma warning disable IDE0008
        var valueTask = ValueTask.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008

        Result<int> compensated = await valueTask.CompensateFirstAsync(error => Task.FromResult(99.ToResult()));

        compensated.ShouldBeSuccess().Should().Be(99);
    }

    #endregion
}
