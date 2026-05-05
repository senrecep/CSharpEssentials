using CSharpEssentials.Errors;
using CSharpEssentials.Maybe;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultAsMaybeTests
{
    private static readonly Error TestError = Error.Validation("Test.Code", "Test message");

    #region Result<T>.AsMaybe

    [Fact]
    public void ResultT_AsMaybe_WithSuccess_ShouldReturnSome()
    {
        var result = 42.ToResult();

        Maybe<int> maybe = result.AsMaybe();

        maybe.HasValue.Should().BeTrue();
        maybe.Value.Should().Be(42);
    }

    [Fact]
    public void ResultT_AsMaybe_WithFailure_ShouldReturnNone()
    {
        var result = Result<int>.Failure(TestError);

        Maybe<int> maybe = result.AsMaybe();

        maybe.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region AsMaybeAsync Extensions

    [Fact]
    public async Task ResultT_AsMaybeAsync_Task_WithSuccess_ShouldReturnSome()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(42.ToResult());
#pragma warning restore IDE0008

        Maybe<int> maybe = await task.AsMaybeAsync();

        maybe.HasValue.Should().BeTrue();
        maybe.Value.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_AsMaybeAsync_Task_WithFailure_ShouldReturnNone()
    {
#pragma warning disable IDE0008
        var task = Task.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008

        Maybe<int> maybe = await task.AsMaybeAsync();

        maybe.HasNoValue.Should().BeTrue();
    }

    [Fact]
    public async Task ResultT_AsMaybeAsync_ValueTask_WithSuccess_ShouldReturnSome()
    {
#pragma warning disable IDE0008
        var valueTask = ValueTask.FromResult(42.ToResult());
#pragma warning restore IDE0008

        Maybe<int> maybe = await valueTask.AsMaybeAsync();

        maybe.HasValue.Should().BeTrue();
        maybe.Value.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_AsMaybeAsync_ValueTask_WithFailure_ShouldReturnNone()
    {
#pragma warning disable IDE0008
        var valueTask = ValueTask.FromResult(Result<int>.Failure(TestError));
#pragma warning restore IDE0008

        Maybe<int> maybe = await valueTask.AsMaybeAsync();

        maybe.HasNoValue.Should().BeTrue();
    }

    #endregion
}
