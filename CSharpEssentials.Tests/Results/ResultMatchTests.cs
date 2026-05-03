using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultMatchTests
{
    private static readonly Error TestError = Error.Failure("TEST", "Test error");
    private static readonly Error AnotherError = Error.Validation("VAL", "Validation error");

    #region Result.Match

    [Fact]
    public void Result_Match_WithSuccess_ShouldCallOnSuccess()
    {
        var result = Result.Success();

        string matched = result.Match(
            () => "success",
            errors => $"failure:{errors.Length}");

        matched.Should().Be("success");
    }

    [Fact]
    public void Result_Match_WithFailure_ShouldCallOnFailure()
    {
        var result = Result.Failure(TestError);

        string matched = result.Match(
            () => "success",
            errors => $"failure:{errors.Length}");

        matched.Should().Be("failure:1");
    }

    [Fact]
    public async Task Result_MatchAsync_WithSuccess_ShouldCallOnSuccess()
    {
        var result = Result.Success();

        string matched = await result.MatchAsync(
            () => Task.FromResult("success"),
            errors => Task.FromResult($"failure:{errors.Length}"));

        matched.Should().Be("success");
    }

    [Fact]
    public async Task Result_MatchAsync_WithFailure_ShouldCallOnFailure()
    {
        var result = Result.Failure(TestError);

        string matched = await result.MatchAsync(
            () => Task.FromResult("success"),
            errors => Task.FromResult($"failure:{errors.Length}"));

        matched.Should().Be("failure:1");
    }

    [Fact]
    public void Result_MatchFirst_WithSuccess_ShouldCallOnSuccess()
    {
        var result = Result.Success();

        string matched = result.MatchFirst(
            () => "success",
            error => $"failure:{error.Code}");

        matched.Should().Be("success");
    }

    [Fact]
    public void Result_MatchFirst_WithFailure_ShouldCallOnFirstError()
    {
        var result = Result.Failure(TestError);

        string matched = result.MatchFirst(
            () => "success",
            error => $"failure:{error.Code}");

        matched.Should().Be("failure:TEST");
    }

    [Fact]
    public void Result_MatchLast_WithSuccess_ShouldCallOnSuccess()
    {
        var result = Result.Success();

        string matched = result.MatchLast(
            () => "success",
            error => $"failure:{error.Code}");

        matched.Should().Be("success");
    }

    [Fact]
    public void Result_MatchLast_WithFailure_ShouldCallOnLastError()
    {
        var result = Result.Failure(TestError, AnotherError);

        string matched = result.MatchLast(
            () => "success",
            error => $"failure:{error.Code}");

        matched.Should().Be("failure:VAL");
    }

    [Fact]
    public async Task Result_MatchFirstAsync_WithSuccess_ShouldCallOnSuccess()
    {
        var result = Result.Success();

        string matched = await result.MatchFirstAsync(
            () => Task.FromResult("success"),
            error => Task.FromResult($"failure:{error.Code}"));

        matched.Should().Be("success");
    }

    [Fact]
    public async Task Result_MatchFirstAsync_WithFailure_ShouldCallOnFirstError()
    {
        var result = Result.Failure(TestError);

        string matched = await result.MatchFirstAsync(
            () => Task.FromResult("success"),
            error => Task.FromResult($"failure:{error.Code}"));

        matched.Should().Be("failure:TEST");
    }

    [Fact]
    public async Task Result_MatchLastAsync_WithSuccess_ShouldCallOnSuccess()
    {
        var result = Result.Success();

        string matched = await result.MatchLastAsync(
            () => Task.FromResult("success"),
            error => Task.FromResult($"failure:{error.Code}"));

        matched.Should().Be("success");
    }

    [Fact]
    public async Task Result_MatchLastAsync_WithFailure_ShouldCallOnLastError()
    {
        var result = Result.Failure(TestError, AnotherError);

        string matched = await result.MatchLastAsync(
            () => Task.FromResult("success"),
            error => Task.FromResult($"failure:{error.Code}"));

        matched.Should().Be("failure:VAL");
    }

    #endregion

    #region Result Match Extensions

    [Fact]
    public async Task Result_Match_Task_WithSuccess_ShouldCallOnSuccess()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Success());
#pragma warning restore IDE0008

        string matched = await task.Match(
            () => "success",
            errors => $"failure:{errors.Length}");

        matched.Should().Be("success");
    }

    [Fact]
    public async Task Result_Match_Task_WithFailure_ShouldCallOnFailure()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Failure(TestError));
#pragma warning restore IDE0008

        string matched = await task.Match(
            () => "success",
            errors => $"failure:{errors.Length}");

        matched.Should().Be("failure:1");
    }

    [Fact]
    public async Task Result_MatchAsync_Task_WithSuccess_ShouldCallOnSuccess()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Success());
#pragma warning restore IDE0008

        string matched = await task.MatchAsync(
            () => Task.FromResult("success"),
            errors => Task.FromResult($"failure:{errors.Length}"));

        matched.Should().Be("success");
    }

    [Fact]
    public async Task Result_MatchAsync_Task_WithFailure_ShouldCallOnFailure()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Failure(TestError));
#pragma warning restore IDE0008

        string matched = await task.MatchAsync(
            () => Task.FromResult("success"),
            errors => Task.FromResult($"failure:{errors.Length}"));

        matched.Should().Be("failure:1");
    }

    [Fact]
    public async Task Result_MatchFirst_Task_WithSuccess_ShouldCallOnSuccess()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Success());
#pragma warning restore IDE0008

        string matched = await task.MatchFirst(
            () => "success",
            error => $"failure:{error.Code}");

        matched.Should().Be("success");
    }

    [Fact]
    public async Task Result_MatchFirst_Task_WithFailure_ShouldCallOnFirstError()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Failure(TestError));
#pragma warning restore IDE0008

        string matched = await task.MatchFirst(
            () => "success",
            error => $"failure:{error.Code}");

        matched.Should().Be("failure:TEST");
    }

    [Fact]
    public async Task Result_MatchFirstAsync_Task_WithSuccess_ShouldCallOnSuccess()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Success());
#pragma warning restore IDE0008

        string matched = await task.MatchFirstAsync(
            () => Task.FromResult("success"),
            error => Task.FromResult($"failure:{error.Code}"));

        matched.Should().Be("success");
    }

    [Fact]
    public async Task Result_MatchFirstAsync_Task_WithFailure_ShouldCallOnFirstError()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Failure(TestError));
#pragma warning restore IDE0008

        string matched = await task.MatchFirstAsync(
            () => Task.FromResult("success"),
            error => Task.FromResult($"failure:{error.Code}"));

        matched.Should().Be("failure:TEST");
    }

    [Fact]
    public async Task Result_MatchLast_Task_WithSuccess_ShouldCallOnSuccess()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Success());
#pragma warning restore IDE0008

        string matched = await task.MatchLast(
            () => "success",
            error => $"failure:{error.Code}");

        matched.Should().Be("success");
    }

    [Fact]
    public async Task Result_MatchLast_Task_WithFailure_ShouldCallOnLastError()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Failure(TestError, AnotherError));
#pragma warning restore IDE0008

        string matched = await task.MatchLast(
            () => "success",
            error => $"failure:{error.Code}");

        matched.Should().Be("failure:VAL");
    }

    [Fact]
    public async Task Result_MatchLastAsync_Task_WithSuccess_ShouldCallOnSuccess()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Success());
#pragma warning restore IDE0008

        string matched = await task.MatchLastAsync(
            () => Task.FromResult("success"),
            error => Task.FromResult($"failure:{error.Code}"),
            default);

        matched.Should().Be("success");
    }

    [Fact]
    public async Task Result_MatchLastAsync_Task_WithFailure_ShouldCallOnLastError()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Failure(TestError, AnotherError));
#pragma warning restore IDE0008

        string matched = await task.MatchLastAsync(
            () => Task.FromResult("success"),
            error => Task.FromResult($"failure:{error.Code}"),
            default);

        matched.Should().Be("failure:VAL");
    }

    #endregion

    #region Result<T>.Match

    [Fact]
    public void ResultT_Match_WithSuccess_ShouldCallOnSuccess()
    {
        var result = Result<int>.Success(42);

        string matched = result.Match(
            v => $"success:{v}",
            errors => $"failure:{errors.Length}");

        matched.Should().Be("success:42");
    }

    [Fact]
    public void ResultT_Match_WithFailure_ShouldCallOnFailure()
    {
        var result = Result<int>.Failure(TestError);

        string matched = result.Match(
            v => $"success:{v}",
            errors => $"failure:{errors.Length}");

        matched.Should().Be("failure:1");
    }

    [Fact]
    public async Task ResultT_MatchAsync_WithSuccess_ShouldCallOnSuccess()
    {
        var result = Result<int>.Success(42);

        string matched = await result.MatchAsync(
            v => Task.FromResult($"success:{v}"),
            errors => Task.FromResult($"failure:{errors.Length}"));

        matched.Should().Be("success:42");
    }

    [Fact]
    public async Task ResultT_MatchAsync_WithFailure_ShouldCallOnFailure()
    {
        var result = Result<int>.Failure(TestError);

        string matched = await result.MatchAsync(
            v => Task.FromResult($"success:{v}"),
            errors => Task.FromResult($"failure:{errors.Length}"));

        matched.Should().Be("failure:1");
    }

    [Fact]
    public void ResultT_MatchFirst_WithSuccess_ShouldCallOnSuccess()
    {
        var result = Result<int>.Success(42);

        string matched = result.MatchFirst(
            v => $"success:{v}",
            error => $"failure:{error.Code}");

        matched.Should().Be("success:42");
    }

    [Fact]
    public void ResultT_MatchFirst_WithFailure_ShouldCallOnFirstError()
    {
        var result = Result<int>.Failure(TestError);

        string matched = result.MatchFirst(
            v => $"success:{v}",
            error => $"failure:{error.Code}");

        matched.Should().Be("failure:TEST");
    }

    [Fact]
    public void ResultT_MatchLast_WithSuccess_ShouldCallOnSuccess()
    {
        var result = Result<int>.Success(42);

        string matched = result.MatchLast(
            v => $"success:{v}",
            error => $"failure:{error.Code}");

        matched.Should().Be("success:42");
    }

    [Fact]
    public void ResultT_MatchLast_WithFailure_ShouldCallOnLastError()
    {
        var result = Result<int>.Failure(TestError, AnotherError);

        string matched = result.MatchLast(
            v => $"success:{v}",
            error => $"failure:{error.Code}");

        matched.Should().Be("failure:VAL");
    }

    [Fact]
    public async Task ResultT_MatchFirstAsync_WithSuccess_ShouldCallOnSuccess()
    {
        var result = Result<int>.Success(42);

        string matched = await result.MatchFirstAsync(
            v => Task.FromResult($"success:{v}"),
            error => Task.FromResult($"failure:{error.Code}"));

        matched.Should().Be("success:42");
    }

    [Fact]
    public async Task ResultT_MatchFirstAsync_WithFailure_ShouldCallOnFirstError()
    {
        var result = Result<int>.Failure(TestError);

        string matched = await result.MatchFirstAsync(
            v => Task.FromResult($"success:{v}"),
            error => Task.FromResult($"failure:{error.Code}"));

        matched.Should().Be("failure:TEST");
    }

    [Fact]
    public async Task ResultT_MatchLastAsync_WithSuccess_ShouldCallOnSuccess()
    {
        var result = Result<int>.Success(42);

        string matched = await result.MatchLastAsync(
            v => Task.FromResult($"success:{v}"),
            error => Task.FromResult($"failure:{error.Code}"));

        matched.Should().Be("success:42");
    }

    [Fact]
    public async Task ResultT_MatchLastAsync_WithFailure_ShouldCallOnLastError()
    {
        var result = Result<int>.Failure(TestError, AnotherError);

        string matched = await result.MatchLastAsync(
            v => Task.FromResult($"success:{v}"),
            error => Task.FromResult($"failure:{error.Code}"));

        matched.Should().Be("failure:VAL");
    }

    #endregion

    #region Result<T> Match Extensions

    [Fact]
    public async Task ResultT_Match_Task_WithSuccess_ShouldCallOnSuccess()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Success(42));
#pragma warning restore IDE0008

        string matched = await task.Match(
            (int v) => $"success:{v}",
            errors => $"failure:{errors.Length}");

        matched.Should().Be("success:42");
    }

    [Fact]
    public async Task ResultT_Match_Task_WithFailure_ShouldCallOnFailure()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008

        string matched = await task.Match(
            (int v) => $"success:{v}",
            errors => $"failure:{errors.Length}");

        matched.Should().Be("failure:1");
    }

    [Fact]
    public async Task ResultT_MatchAsync_Task_WithSuccess_ShouldCallOnSuccess()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Success(42));
#pragma warning restore IDE0008

        string matched = await task.MatchAsync(
            (int v) => Task.FromResult($"success:{v}"),
            errors => Task.FromResult($"failure:{errors.Length}"));

        matched.Should().Be("success:42");
    }

    [Fact]
    public async Task ResultT_MatchAsync_Task_WithFailure_ShouldCallOnFailure()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008

        string matched = await task.MatchAsync(
            (int v) => Task.FromResult($"success:{v}"),
            errors => Task.FromResult($"failure:{errors.Length}"));

        matched.Should().Be("failure:1");
    }

    [Fact]
    public async Task ResultT_MatchFirst_Task_WithSuccess_ShouldCallOnSuccess()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Success(42));
#pragma warning restore IDE0008

        string matched = await task.MatchFirst(
            (int v) => $"success:{v}",
            error => $"failure:{error.Code}");

        matched.Should().Be("success:42");
    }

    [Fact]
    public async Task ResultT_MatchFirst_Task_WithFailure_ShouldCallOnFirstError()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008

        string matched = await task.MatchFirst(
            (int v) => $"success:{v}",
            error => $"failure:{error.Code}");

        matched.Should().Be("failure:TEST");
    }

    [Fact]
    public async Task ResultT_MatchFirstAsync_Task_WithSuccess_ShouldCallOnSuccess()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Success(42));
#pragma warning restore IDE0008

        string matched = await task.MatchFirstAsync(
            (int v) => Task.FromResult($"success:{v}"),
            error => Task.FromResult($"failure:{error.Code}"));

        matched.Should().Be("success:42");
    }

    [Fact]
    public async Task ResultT_MatchFirstAsync_Task_WithFailure_ShouldCallOnFirstError()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008

        string matched = await task.MatchFirstAsync(
            (int v) => Task.FromResult($"success:{v}"),
            error => Task.FromResult($"failure:{error.Code}"));

        matched.Should().Be("failure:TEST");
    }

    #endregion
}
