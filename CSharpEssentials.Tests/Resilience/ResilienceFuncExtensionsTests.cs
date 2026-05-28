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

    [Fact]
    public async Task ExecuteAsync_Should_Handle_Exception_In_Func_Task()
    {
        Func<Task> func = () => throw new InvalidOperationException("boom");
        Result result = await func.ExecuteAsync();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_Should_Handle_Exception_In_Func_Task_T()
    {
        Func<Task<int>> func = () => throw new InvalidOperationException("boom");
        Result<int> result = await func.ExecuteAsync();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_Should_Handle_Exception_In_Func_Task_Result_T()
    {
        Func<Task<Result<int>>> func = () => throw new InvalidOperationException("boom");
        Result<int> result = await func.ExecuteAsync();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_Should_Handle_Exception_In_Func_CT_Task()
    {
        Func<CancellationToken, Task> func = _ => throw new InvalidOperationException("boom");
        Result result = await func.ExecuteAsync();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_Should_Handle_Exception_In_Func_CT_Task_T()
    {
        Func<CancellationToken, Task<int>> func = _ => throw new InvalidOperationException("boom");
        Result<int> result = await func.ExecuteAsync();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_Should_Handle_Exception_In_Func_CT_Task_Result_T()
    {
        Func<CancellationToken, Task<Result<int>>> func = _ => throw new InvalidOperationException("boom");
        Result<int> result = await func.ExecuteAsync();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_Should_Handle_OperationCanceledException()
    {
        using CancellationTokenSource cts = new();

        Func<CancellationToken, Task> func = async ct =>
        {
            await Task.Delay(100, ct);
        };

        Result result = await func.ExecuteAsync(cts.Token);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_Generic_Should_Handle_OperationCanceledException()
    {
        using CancellationTokenSource cts = new();

        Func<CancellationToken, Task<int>> func = async ct =>
        {
            await Task.Delay(100, ct);
            return 42;
        };

        Result<int> result = await func.ExecuteAsync(cts.Token);

        result.IsSuccess.Should().BeTrue();
    }
}
