using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Results.Benchmarks;

/// <summary>
/// Sources and handlers shared by the async matrix benchmarks. With <c>yield</c> false every awaitable is already
/// completed (the synchronous fast path); with <c>yield</c> true it completes after <see cref="Task.Yield"/>.
/// </summary>
internal static class BenchmarkSources
{
    public static readonly Error Failure = Error.Failure("BENCH", "Benchmark failure");

    private static readonly Task<Result<int>> CompletedSuccess = Task.FromResult<Result<int>>(42);
    private static readonly Task<Result<int>> CompletedFailure = Task.FromResult<Result<int>>(Failure);

    public static Result<int> Plain(bool succeed) => succeed ? 42 : Failure;

    public static Task<Result<int>> TaskOf(bool succeed, bool yield)
    {
        if (yield)
            return YieldResultTask(succeed);
        return succeed ? CompletedSuccess : CompletedFailure;
    }

    public static ValueTask<Result<int>> ValueTaskOf(bool succeed, bool yield) =>
        yield ? YieldResultValueTask(succeed) : new ValueTask<Result<int>>(Plain(succeed));

    public static Task<int> TaskValue(int value, bool yield) =>
        yield ? YieldValueTask(value) : Task.FromResult(value);

    public static ValueTask<int> ValueTaskValue(int value, bool yield) =>
        yield ? YieldValueValueTask(value) : new ValueTask<int>(value);

    public static Task<Result<int>> TaskResult(int value, bool yield) =>
        yield ? YieldResultOfValueTask(value) : Task.FromResult<Result<int>>(value);

    public static ValueTask<Result<int>> ValueTaskResult(int value, bool yield) =>
        yield ? YieldResultOfValueValueTask(value) : new ValueTask<Result<int>>(value);

    public static Task<bool> TaskBool(bool value, bool yield) =>
        yield ? YieldBoolTask(value) : Task.FromResult(value);

    public static ValueTask<bool> ValueTaskBool(bool value, bool yield) =>
        yield ? YieldBoolValueTask(value) : new ValueTask<bool>(value);

    public static Task TaskWork(bool yield) => yield ? YieldTask() : Task.CompletedTask;

    public static ValueTask ValueTaskWork(bool yield) => yield ? YieldValueTask() : default;

    private static async Task<Result<int>> YieldResultTask(bool succeed)
    {
        await Task.Yield();
        return Plain(succeed);
    }

    private static async ValueTask<Result<int>> YieldResultValueTask(bool succeed)
    {
        await Task.Yield();
        return Plain(succeed);
    }

    private static async Task<int> YieldValueTask(int value)
    {
        await Task.Yield();
        return value;
    }

    private static async ValueTask<int> YieldValueValueTask(int value)
    {
        await Task.Yield();
        return value;
    }

    private static async Task<Result<int>> YieldResultOfValueTask(int value)
    {
        await Task.Yield();
        return value;
    }

    private static async ValueTask<Result<int>> YieldResultOfValueValueTask(int value)
    {
        await Task.Yield();
        return value;
    }

    private static async Task<bool> YieldBoolTask(bool value)
    {
        await Task.Yield();
        return value;
    }

    private static async ValueTask<bool> YieldBoolValueTask(bool value)
    {
        await Task.Yield();
        return value;
    }

    private static async Task YieldTask() => await Task.Yield();

    private static async ValueTask YieldValueTask() => await Task.Yield();
}
