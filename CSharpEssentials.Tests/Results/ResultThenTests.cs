using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultThenTests
{
    private static readonly Error TestError = Error.Validation("Test.Code", "Test message");

    #region Result.Then

    [Fact]
    public void Result_Then_WithSuccess_ShouldExecuteFunction()
    {
        var result = Result.Success();
        bool called = false;

        Result thenResult = result.Then(() => { called = true; return Result.Success(); });

        thenResult.IsSuccess.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public void Result_Then_WithFailure_ShouldNotExecuteFunction()
    {
        var result = Result.Failure(TestError);
        bool called = false;

        Result thenResult = result.Then(() => { called = true; return Result.Success(); });

        thenResult.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void Result_Then_Chained_ShouldStopOnFirstFailure()
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
    public void Result_ThenDo_WithSuccess_ShouldExecuteAction()
    {
        var result = Result.Success();
        bool called = false;

        Result thenResult = result.ThenDo(() => called = true);

        thenResult.IsSuccess.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public void Result_ThenDo_WithFailure_ShouldNotExecuteAction()
    {
        var result = Result.Failure(TestError);
        bool called = false;

        Result thenResult = result.ThenDo(() => called = true);

        thenResult.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task Result_ThenAsync_WithSuccess_ShouldExecuteFunction()
    {
        var result = Result.Success();
        bool called = false;

        Result thenResult = await result.ThenAsync(() => { called = true; return Task.FromResult(Result.Success()); });

        thenResult.IsSuccess.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public async Task Result_ThenAsync_WithFailure_ShouldNotExecuteFunction()
    {
        var result = Result.Failure(TestError);
        bool called = false;

        Result thenResult = await result.ThenAsync(() => { called = true; return Task.FromResult(Result.Success()); });

        thenResult.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task Result_ThenDoAsync_WithSuccess_ShouldExecuteAction()
    {
        var result = Result.Success();
        bool called = false;

        Result thenResult = await result.ThenDoAsync(() => { called = true; return Task.CompletedTask; });

        thenResult.IsSuccess.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public async Task Result_ThenDoAsync_WithFailure_ShouldNotExecuteAction()
    {
        var result = Result.Failure(TestError);
        bool called = false;

        Result thenResult = await result.ThenDoAsync(() => { called = true; return Task.CompletedTask; });

        thenResult.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    #endregion

    #region Task<Result>.Then

    [Fact]
    public async Task TaskResult_Then_WithSuccess_ShouldExecuteFunction()
    {
        Task<Result> task = Task.FromResult(Result.Success());
        bool called = false;

        Result thenResult = await task.Then(() => { called = true; return Result.Success(); }, CancellationToken.None);

        thenResult.IsSuccess.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public async Task TaskResult_Then_WithFailure_ShouldNotExecuteFunction()
    {
        Task<Result> task = Task.FromResult(Result.Failure(TestError));
        bool called = false;

        Result thenResult = await task.Then(() => { called = true; return Result.Success(); }, CancellationToken.None);

        thenResult.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task TaskResult_ThenDo_WithSuccess_ShouldExecuteAction()
    {
        Task<Result> task = Task.FromResult(Result.Success());
        bool called = false;

        Result thenResult = await task.ThenDo(() => called = true, CancellationToken.None);

        thenResult.IsSuccess.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public async Task TaskResult_ThenDo_WithFailure_ShouldNotExecuteAction()
    {
        Task<Result> task = Task.FromResult(Result.Failure(TestError));
        bool called = false;

        Result thenResult = await task.ThenDo(() => called = true, CancellationToken.None);

        thenResult.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task TaskResult_ThenAsync_WithSuccess_ShouldExecuteFunction()
    {
        Task<Result> task = Task.FromResult(Result.Success());
        bool called = false;

        Result thenResult = await task.ThenAsync(() => { called = true; return Task.FromResult(Result.Success()); }, CancellationToken.None);

        thenResult.IsSuccess.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public async Task TaskResult_ThenAsync_WithFailure_ShouldNotExecuteFunction()
    {
        Task<Result> task = Task.FromResult(Result.Failure(TestError));
        bool called = false;

        Result thenResult = await task.ThenAsync(() => { called = true; return Task.FromResult(Result.Success()); }, CancellationToken.None);

        thenResult.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task TaskResult_ThenDoAsync_WithSuccess_ShouldExecuteAction()
    {
        Task<Result> task = Task.FromResult(Result.Success());
        bool called = false;

        Result thenResult = await task.ThenDoAsync(() => { called = true; return Task.CompletedTask; }, CancellationToken.None);

        thenResult.IsSuccess.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public async Task TaskResult_ThenDoAsync_WithFailure_ShouldNotExecuteAction()
    {
        Task<Result> task = Task.FromResult(Result.Failure(TestError));
        bool called = false;

        Result thenResult = await task.ThenDoAsync(() => { called = true; return Task.CompletedTask; }, CancellationToken.None);

        thenResult.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    #endregion

    #region Result<T>.Then

    [Fact]
    public void ResultT_Then_ToResult_WithSuccess_ShouldExecuteFunction()
    {
        var result = Result<int>.Success(10);

        Result<string> thenResult = result.Then(value => Result<string>.Success(value.ToString(System.Globalization.CultureInfo.InvariantCulture)));

        thenResult.IsSuccess.Should().BeTrue();
        thenResult.Value.Should().Be("10");
    }

    [Fact]
    public void ResultT_Then_ToResult_WithFailure_ShouldNotExecuteFunction()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<string> thenResult = result.Then(value =>
        {
            called = true;
            return Result<string>.Success(value.ToString(System.Globalization.CultureInfo.InvariantCulture));
        });

        thenResult.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void ResultT_Then_ToValue_WithSuccess_ShouldExecuteFunction()
    {
        var result = Result<int>.Success(10);

        Result<string> thenResult = result.Then(value => value.ToString(System.Globalization.CultureInfo.InvariantCulture));

        thenResult.IsSuccess.Should().BeTrue();
        thenResult.Value.Should().Be("10");
    }

    [Fact]
    public void ResultT_Then_ToValue_WithFailure_ShouldNotExecuteFunction()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<string> thenResult = result.Then(value =>
        {
            called = true;
            return value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        });

        thenResult.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void ResultT_ThenDo_WithSuccess_ShouldExecuteAction()
    {
        var result = Result<int>.Success(42);
        int captured = 0;

        Result<int> thenResult = result.ThenDo(value => captured = value);

        thenResult.IsSuccess.Should().BeTrue();
        captured.Should().Be(42);
    }

    [Fact]
    public void ResultT_ThenDo_WithFailure_ShouldNotExecuteAction()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<int> thenResult = result.ThenDo(_ => called = true);

        thenResult.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public void ResultT_Then_Chained_ShouldTransformThroughChain()
    {
        Result<string> result = Result<int>.Success(10)
            .Then(v => v * 2)
            .Then(v => v.ToString(System.Globalization.CultureInfo.InvariantCulture));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("20");
    }

    [Fact]
    public void ResultT_Then_Chained_ShouldStopOnFailure()
    {
        int callCount = 0;

        Result<string> result = Result<int>.Success(10)
            .Then(v => { callCount++; return v * 2; })
            .Then(_ => Result<int>.Failure(TestError))
            .Then(v => { callCount++; return v.ToString(System.Globalization.CultureInfo.InvariantCulture); });

        result.IsFailure.Should().BeTrue();
        callCount.Should().Be(1);
    }

    [Fact]
    public async Task ResultT_ThenAsync_ToResult_WithSuccess_ShouldExecuteFunction()
    {
        var result = Result<int>.Success(10);

        Result<string> thenResult = await result.ThenAsync(value =>
            Task.FromResult(Result<string>.Success(value.ToString(System.Globalization.CultureInfo.InvariantCulture))));

        thenResult.IsSuccess.Should().BeTrue();
        thenResult.Value.Should().Be("10");
    }

    [Fact]
    public async Task ResultT_ThenAsync_ToResult_WithFailure_ShouldNotExecuteFunction()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<string> thenResult = await result.ThenAsync(value =>
        {
            called = true;
            return Task.FromResult(Result<string>.Success(value.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        });

        thenResult.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task ResultT_ThenAsync_ToValue_WithSuccess_ShouldExecuteFunction()
    {
        var result = Result<int>.Success(10);

        Result<string> thenResult = await result.ThenAsync(value =>
            Task.FromResult(value.ToString(System.Globalization.CultureInfo.InvariantCulture)));

        thenResult.IsSuccess.Should().BeTrue();
        thenResult.Value.Should().Be("10");
    }

    [Fact]
    public async Task ResultT_ThenAsync_ToValue_WithFailure_ShouldNotExecuteFunction()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<string> thenResult = await result.ThenAsync(value =>
        {
            called = true;
            return Task.FromResult(value.ToString(System.Globalization.CultureInfo.InvariantCulture));
        });

        thenResult.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task ResultT_ThenDoAsync_WithSuccess_ShouldExecuteAction()
    {
        var result = Result<int>.Success(42);
        int captured = 0;

        Result<int> thenResult = await result.ThenDoAsync(value =>
        {
            captured = value;
            return Task.CompletedTask;
        });

        thenResult.IsSuccess.Should().BeTrue();
        captured.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_ThenDoAsync_WithFailure_ShouldNotExecuteAction()
    {
        var result = Result<int>.Failure(TestError);
        bool called = false;

        Result<int> thenResult = await result.ThenDoAsync(_ =>
        {
            called = true;
            return Task.CompletedTask;
        });

        thenResult.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    #endregion

    #region Task<Result<T>>.Then

    [Fact]
    public async Task TaskResultT_Then_ToResult_WithSuccess_ShouldExecuteFunction()
    {
        Task<Result<int>> task = Task.FromResult(Result<int>.Success(10));

        Result<string> thenResult = await task.Then(
            value => Result<string>.Success(value.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            CancellationToken.None);

        thenResult.IsSuccess.Should().BeTrue();
        thenResult.Value.Should().Be("10");
    }

    [Fact]
    public async Task TaskResultT_Then_ToResult_WithFailure_ShouldNotExecuteFunction()
    {
        Task<Result<int>> task = Task.FromResult(Result<int>.Failure(TestError));
        bool called = false;

        Result<string> thenResult = await task.Then(
            value =>
            {
                called = true;
                return Result<string>.Success(value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            },
            CancellationToken.None);

        thenResult.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task TaskResultT_Then_ToValue_WithSuccess_ShouldExecuteFunction()
    {
        Task<Result<int>> task = Task.FromResult(Result<int>.Success(10));

        Result<string> thenResult = await task.Then(
            value => value.ToString(System.Globalization.CultureInfo.InvariantCulture),
            CancellationToken.None);

        thenResult.IsSuccess.Should().BeTrue();
        thenResult.Value.Should().Be("10");
    }

    [Fact]
    public async Task TaskResultT_Then_ToValue_WithFailure_ShouldNotExecuteFunction()
    {
        Task<Result<int>> task = Task.FromResult(Result<int>.Failure(TestError));
        bool called = false;

        Result<string> thenResult = await task.Then(
            value =>
            {
                called = true;
                return value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            },
            CancellationToken.None);

        thenResult.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task TaskResultT_ThenDo_WithSuccess_ShouldExecuteAction()
    {
        Task<Result<int>> task = Task.FromResult(Result<int>.Success(42));
        int captured = 0;

        Result<int> thenResult = await task.ThenDo(value => captured = value, CancellationToken.None);

        thenResult.IsSuccess.Should().BeTrue();
        captured.Should().Be(42);
    }

    [Fact]
    public async Task TaskResultT_ThenDo_WithFailure_ShouldNotExecuteAction()
    {
        Task<Result<int>> task = Task.FromResult(Result<int>.Failure(TestError));
        bool called = false;

        Result<int> thenResult = await task.ThenDo(_ => called = true, CancellationToken.None);

        thenResult.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task TaskResultT_ThenAsync_ToResult_WithSuccess_ShouldExecuteFunction()
    {
        Task<Result<int>> task = Task.FromResult(Result<int>.Success(10));

        Result<string> thenResult = await task.ThenAsync(
            value => Task.FromResult(Result<string>.Success(value.ToString(System.Globalization.CultureInfo.InvariantCulture))),
            CancellationToken.None);

        thenResult.IsSuccess.Should().BeTrue();
        thenResult.Value.Should().Be("10");
    }

    [Fact]
    public async Task TaskResultT_ThenAsync_ToResult_WithFailure_ShouldNotExecuteFunction()
    {
        Task<Result<int>> task = Task.FromResult(Result<int>.Failure(TestError));
        bool called = false;

        Result<string> thenResult = await task.ThenAsync(
            value =>
            {
                called = true;
                return Task.FromResult(Result<string>.Success(value.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            },
            CancellationToken.None);

        thenResult.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task TaskResultT_ThenAsync_ToValue_WithSuccess_ShouldExecuteFunction()
    {
        Task<Result<int>> task = Task.FromResult(Result<int>.Success(10));

        Result<string> thenResult = await task.ThenAsync(
            value => Task.FromResult(value.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            CancellationToken.None);

        thenResult.IsSuccess.Should().BeTrue();
        thenResult.Value.Should().Be("10");
    }

    [Fact]
    public async Task TaskResultT_ThenAsync_ToValue_WithFailure_ShouldNotExecuteFunction()
    {
        Task<Result<int>> task = Task.FromResult(Result<int>.Failure(TestError));
        bool called = false;

        Result<string> thenResult = await task.ThenAsync(
            value =>
            {
                called = true;
                return Task.FromResult(value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            },
            CancellationToken.None);

        thenResult.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task TaskResultT_ThenDoAsync_WithSuccess_ShouldExecuteAction()
    {
        Task<Result<int>> task = Task.FromResult(Result<int>.Success(42));
        int captured = 0;

        Result<int> thenResult = await task.ThenDoAsync(
            value =>
            {
                captured = value;
                return Task.CompletedTask;
            },
            CancellationToken.None);

        thenResult.IsSuccess.Should().BeTrue();
        captured.Should().Be(42);
    }

    [Fact]
    public async Task TaskResultT_ThenDoAsync_WithFailure_ShouldNotExecuteAction()
    {
        Task<Result<int>> task = Task.FromResult(Result<int>.Failure(TestError));
        bool called = false;

        Result<int> thenResult = await task.ThenDoAsync(
            _ =>
            {
                called = true;
                return Task.CompletedTask;
            },
            CancellationToken.None);

        thenResult.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    #endregion
}
