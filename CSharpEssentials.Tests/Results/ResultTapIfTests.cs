using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultTapIfTests
{
    [Fact]
    public void TapIf_Should_ExecuteAction_When_Success_And_PredicateTrue()
    {
        bool called = false;
        Result<int> result = Result<int>.Success(42);

        result.TapIf(v => v > 10, v => called = true);

        called.Should().BeTrue();
    }

    [Fact]
    public void TapIf_Should_NotExecuteAction_When_Success_And_PredicateFalse()
    {
        bool called = false;
        Result<int> result = Result<int>.Success(5);

        result.TapIf(v => v > 10, v => called = true);

        called.Should().BeFalse();
    }

    [Fact]
    public void TapIf_Should_NotExecuteAction_When_Failure()
    {
        bool called = false;
        Result<int> result = Result<int>.Failure(CSharpEssentials.Errors.Error.Failure("E", "fail"));

        result.TapIf(v => v > 0, v => called = true);

        called.Should().BeFalse();
    }

    [Fact]
    public void TapIf_Should_ReturnSameResult()
    {
        Result<int> result = Result<int>.Success(42);
        Result<int> returned = result.TapIf(v => v > 0, v => { });

        returned.Should().Be(result);
    }

    [Fact]
    public async Task TapIfAsync_Bool_Should_ExecuteAction_When_Success_And_ConditionTrue()
    {
        bool called = false;
        Result<int> result = Result<int>.Success(42);

        await result.TapIfAsync(true, async v => { await Task.Yield(); called = true; });

        called.Should().BeTrue();
    }

    [Fact]
    public async Task TapIfAsync_Bool_Should_NotExecuteAction_When_Success_And_ConditionFalse()
    {
        bool called = false;
        Result<int> result = Result<int>.Success(42);

        await result.TapIfAsync(false, async v => { await Task.Yield(); called = true; });

        called.Should().BeFalse();
    }

    [Fact]
    public async Task TapIfAsync_Bool_Should_NotExecuteAction_When_Failure()
    {
        bool called = false;
        Result<int> result = Result<int>.Failure(CSharpEssentials.Errors.Error.Failure("E", "fail"));

        await result.TapIfAsync(true, async v => { await Task.Yield(); called = true; });

        called.Should().BeFalse();
    }

    [Fact]
    public async Task TapIfAsync_Predicate_Should_ExecuteAction_When_Success_And_PredicateTrue()
    {
        bool called = false;
        Result<int> result = Result<int>.Success(42);

        await result.TapIfAsync(v => v == 42, async v => { await Task.Yield(); called = true; });

        called.Should().BeTrue();
    }

    [Fact]
    public async Task TapIfAsync_Predicate_Should_NotExecuteAction_When_Success_And_PredicateFalse()
    {
        bool called = false;
        Result<int> result = Result<int>.Success(5);

        await result.TapIfAsync(v => v > 10, async v => { await Task.Yield(); called = true; });

        called.Should().BeFalse();
    }

    [Fact]
    public async Task TapIfAsync_Predicate_Should_NotExecuteAction_When_Failure()
    {
        bool called = false;
        Result<int> result = Result<int>.Failure(CSharpEssentials.Errors.Error.Failure("E", "fail"));

        await result.TapIfAsync(v => v > 0, async v => { await Task.Yield(); called = true; });

        called.Should().BeFalse();
    }

    [Fact]
    public async Task TapIfAsync_Task_Predicate_Should_ExecuteAction_When_Success_And_PredicateTrue()
    {
        bool called = false;
        Task<Result<int>> task = Task.FromResult(Result<int>.Success(42));

        await task.TapIfAsync(v => v > 10, v => called = true);

        called.Should().BeTrue();
    }

    [Fact]
    public async Task TapIfAsync_Task_Predicate_Should_NotExecuteAction_When_Success_And_PredicateFalse()
    {
        bool called = false;
        Task<Result<int>> task = Task.FromResult(Result<int>.Success(5));

        await task.TapIfAsync(v => v > 10, v => called = true);

        called.Should().BeFalse();
    }

    [Fact]
    public async Task TapIfAsync_Task_Bool_Should_ExecuteAction_When_Success_And_ConditionTrue()
    {
        bool called = false;
        Task<Result<int>> task = Task.FromResult(Result<int>.Success(42));

        await task.TapIfAsync(true, async v => { await Task.Yield(); called = true; });

        called.Should().BeTrue();
    }

    [Fact]
    public async Task TapIfAsync_Task_Func_Should_ExecuteAction_When_Success_And_PredicateTrue()
    {
        bool called = false;
        Task<Result<int>> task = Task.FromResult(Result<int>.Success(42));

        await task.TapIfAsync(v => v == 42, async v => { await Task.Yield(); called = true; });

        called.Should().BeTrue();
    }

    [Fact]
    public async Task TapIfAsync_ValueTask_Predicate_Should_ExecuteAction_When_Success_And_PredicateTrue()
    {
        bool called = false;
        ValueTask<Result<int>> task = new(Result<int>.Success(42));

        await task.TapIfAsync(v => v > 10, v => called = true);

        called.Should().BeTrue();
    }

    [Fact]
    public async Task TapIfAsync_ValueTask_Predicate_Should_NotExecuteAction_When_Success_And_PredicateFalse()
    {
        bool called = false;
        ValueTask<Result<int>> task = new(Result<int>.Success(5));

        await task.TapIfAsync(v => v > 10, v => called = true);

        called.Should().BeFalse();
    }

    [Fact]
    public async Task TapIfAsync_ValueTask_Bool_Should_ExecuteAction_When_Success_And_ConditionTrue()
    {
        bool called = false;
        ValueTask<Result<int>> task = new(Result<int>.Success(42));

        await task.TapIfAsync(true, async v => { await Task.Yield(); called = true; });

        called.Should().BeTrue();
    }

    [Fact]
    public async Task TapIfAsync_ValueTask_Func_Should_ExecuteAction_When_Success_And_PredicateTrue()
    {
        bool called = false;
        ValueTask<Result<int>> task = new(Result<int>.Success(42));

        await task.TapIfAsync(v => v == 42, async v => { await Task.Yield(); called = true; });

        called.Should().BeTrue();
    }
}
