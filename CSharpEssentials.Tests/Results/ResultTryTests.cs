using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultTryTests
{
    private static readonly Error TestError = Error.Validation("Test.Code", "Test message");

    #region Result.Try

    [Fact]
    public void Result_Try_Action_Success_ShouldReturnSuccess()
    {
        bool called = false;

        Result result = Result.Try(() => { called = true; }, ex => TestError);

        result.IsSuccess.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public void Result_Try_Action_Exception_ShouldReturnFailure()
    {
        Result result = Result.Try(
            () => throw new InvalidOperationException("boom"),
            ex =>
            {
                ex.Message.Should().Be("boom");
                return TestError;
            });

        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(TestError);
    }

    #endregion

    #region Result.Try<TValue>

    [Fact]
    public void Result_TryT_Func_Success_ShouldReturnSuccessWithValue()
    {
        Result<int> result = Result.Try(() => 42, ex => TestError);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Result_TryT_Func_Exception_ShouldReturnFailure()
    {
        Result<int> result = Result.Try(
            (Func<Result<int>>)(() => throw new InvalidOperationException("boom")),
            ex => TestError);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(TestError);
    }

    [Fact]
    public void Result_TryT_ResultFunc_Success_ShouldReturnSuccessWithValue()
    {
        Result<int> result = Result.Try(() => Result<int>.Success(42), ex => TestError);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Result_TryT_ResultFunc_Exception_ShouldReturnFailure()
    {
        Result<int> result = Result.Try<int>(
            () => throw new InvalidOperationException("boom"),
            ex => TestError);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(TestError);
    }

    #endregion

    #region Result.TryAsync

    [Fact]
    public async Task Result_TryAsync_Action_Success_ShouldReturnSuccess()
    {
        bool called = false;

        Result result = await Result.TryAsync(
            () => { called = true; return Task.CompletedTask; },
            ex => TestError);

        result.IsSuccess.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public async Task Result_TryAsync_Action_Exception_ShouldReturnFailure()
    {
        Result result = await Result.TryAsync(
            () => Task.FromException(new InvalidOperationException("boom")),
            ex => TestError);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(TestError);
    }

    #endregion

    #region Result.TryAsync<TValue>

    [Fact]
    public async Task Result_TryAsyncT_Func_Success_ShouldReturnSuccessWithValue()
    {
        Result<int> result = await Result.TryAsync(() => Task.FromResult(42), ex => TestError);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task Result_TryAsyncT_Func_Exception_ShouldReturnFailure()
    {
        Result<int> result = await Result.TryAsync(
            () => Task.FromException<int>(new InvalidOperationException("boom")),
            ex => TestError);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task Result_TryAsyncT_ResultFunc_Success_ShouldReturnSuccessWithValue()
    {
        Result<int> result = await Result.TryAsync(
            () => Task.FromResult(Result<int>.Success(42)),
            ex => TestError);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task Result_TryAsyncT_ResultFunc_Exception_ShouldReturnFailure()
    {
        Result<int> result = await Result.TryAsync(
            () => Task.FromException<Result<int>>(new InvalidOperationException("boom")),
            ex => TestError);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(TestError);
    }

    #endregion
}
