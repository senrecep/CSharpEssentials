using CSharpEssentials.Errors;
using CSharpEssentials.Maybe;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Maybe;

public class MaybeExtensionsTests
{
    [Fact]
    public void ToMaybeResult_WithValue_ShouldReturnSuccess()
    {
        Maybe<int> maybe = 42;

        Result<int> result = maybe.ToMaybeResult();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void ToMaybeResult_WithoutValue_ShouldReturnDefaultError()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Result<int> result = maybe.ToMaybeResult();

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Maybe.Result");
    }

    [Fact]
    public void ToMaybeResult_WithoutValue_WithCustomError_ShouldReturnCustomError()
    {
        Maybe<int> maybe = Maybe<int>.None;
        Error customError = TestData.Errors.NotFound;

        Result<int> result = maybe.ToMaybeResult(customError);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(customError);
    }

    [Fact]
    public void ToMaybeUnitResult_WithValue_ShouldReturnSuccess()
    {
        Maybe<int> maybe = 42;

        Result result = maybe.ToMaybeUnitResult();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ToMaybeUnitResult_WithoutValue_ShouldReturnDefaultError()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Result result = maybe.ToMaybeUnitResult();

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Maybe.Result");
    }

    [Fact]
    public async Task ToMaybeResult_WithTask_ShouldReturnSuccess()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(42));

        Result<int> result = await maybeTask.ToMaybeResult();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task ToMaybeResult_WithValueTask_ShouldReturnSuccess()
    {
        var maybeTask = new ValueTask<Maybe<int>>(42);

        Result<int> result = await maybeTask.ToMaybeResult();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task ToMaybeUnitResult_WithTask_ShouldReturnSuccess()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult<Maybe<int>>(42);

        Result result = await maybeTask.ToMaybeUnitResult();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ToMaybeUnitResult_WithValueTask_ShouldReturnSuccess()
    {
        var maybeTask = new ValueTask<Maybe<int>>(42);

        Result result = await maybeTask.ToMaybeUnitResult();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ToMaybeResult_WithTask_WithoutValue_ShouldReturnError()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.None);
        Error customError = TestData.Errors.NotFound;

        Result<int> result = await maybeTask.ToMaybeResult(customError);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(customError);
    }
}

