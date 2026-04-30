using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultModulesTests
{
    private static readonly Error TestError = Error.Failure("TEST", "Test error");

    #region Bind

    [Fact]
    public void Bind_WithSuccess_ShouldExecuteFunction()
    {
        var result = Result.Success();

        Result<int> bound = result.Bind(() => Result<int>.Success(42));

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be(42);
    }

    [Fact]
    public void Bind_WithFailure_ShouldNotExecuteFunction()
    {
        var result = Result.Failure(TestError);
        bool functionCalled = false;

        Result<int> bound = result.Bind(() =>
        {
            functionCalled = true;
            return Result<int>.Success(42);
        });

        bound.IsFailure.Should().BeTrue();
        functionCalled.Should().BeFalse();
    }

    [Fact]
    public void Bind_WithSuccess_ToResult_ShouldExecuteFunction()
    {
        var result = Result.Success();

        Result bound = result.Bind(() => Result.Success());

        bound.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task BindAsync_WithSuccess_ShouldExecuteFunction()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Success());
#pragma warning restore IDE0008

        Result bound = await task.BindAsync(Result.Success);

        bound.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task BindAsync_WithFailure_ShouldNotExecuteFunction()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result.Failure(TestError));
#pragma warning restore IDE0008
        bool functionCalled = false;

        Result bound = await task.BindAsync(() =>
        {
            functionCalled = true;
            return Result.Success();
        });

        bound.IsFailure.Should().BeTrue();
        functionCalled.Should().BeFalse();
    }

    #endregion

    #region Map

    [Fact]
    public void Map_WithSuccess_ShouldTransformValue()
    {
        var result = Result.Success();

        Result<int> mapped = result.Map(() => 42);

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(42);
    }

    [Fact]
    public void Map_WithFailure_ShouldNotTransform()
    {
        var result = Result.Failure(TestError);
        bool functionCalled = false;

        Result<int> mapped = result.Map(() =>
        {
            functionCalled = true;
            return 42;
        });

        mapped.IsFailure.Should().BeTrue();
        functionCalled.Should().BeFalse();
    }

    [Fact]
    public void Map_WithResultFunc_ShouldWork()
    {
        var result = Result.Success();

        Result<int> mapped = result.Map(() => Result<int>.Success(42));

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(42);
    }

    #endregion
}

