using CSharpEssentials.Errors;
using CSharpEssentials.Resilience;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Resilience;

public class ResilienceFuncExtensionsTests
{
    [Fact]
    public async Task ExecuteAsync_Should_Return_Success()
    {
        Func<Task> func = () => Task.CompletedTask;
        Result result = await func.ExecuteAsync();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_Generic_Should_Return_Value()
    {
        Func<Task<int>> func = () => Task.FromResult(42);
        Result<int> result = await func.ExecuteAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task ExecuteAsync_With_CancellationToken_Should_Work()
    {
        Func<CancellationToken, Task> func = _ => Task.CompletedTask;
        Result result = await func.ExecuteAsync();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_Generic_With_CancellationToken_Should_Work()
    {
        Func<CancellationToken, Task<int>> func = _ => Task.FromResult(42);
        Result<int> result = await func.ExecuteAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task ExecuteAsync_Result_Generic_With_CancellationToken_Should_Work()
    {
        Func<CancellationToken, Task<Result<int>>> func = _ => Task.FromResult(Result<int>.Success(42));
        Result<int> result = await func.ExecuteAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }
}
