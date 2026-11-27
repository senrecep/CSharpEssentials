using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Results.Benchmarks;

/// <summary>
/// Typed twins of <see cref="BenchmarkSources"/>: the same success value (42) and a cached error in the last slot of each
/// arity, so a failure takes the longest dispatch. With <c>yield</c> false every awaitable is already completed; with
/// <c>yield</c> true it completes after <see cref="Task.Yield"/>.
/// </summary>
internal static class TypedSources
{
    public static readonly NotFoundError Missing = new("BENCH.NotFound", "Benchmark failure");
    public static readonly ConflictError Stale = new("BENCH.Conflict", "Benchmark failure");
    public static readonly ValidationError Invalid = new("BENCH.Invalid", "Benchmark failure");

    private static readonly Result<int, NotFoundError, ConflictError, ValidationError> SuccessResult = 42;
    private static readonly Result<int, NotFoundError, ConflictError, ValidationError> FailureResult = Invalid;
    private static readonly Task<Result<int, NotFoundError, ConflictError, ValidationError>> CompletedSuccess = Task.FromResult(SuccessResult);
    private static readonly Task<Result<int, NotFoundError, ConflictError, ValidationError>> CompletedFailure = Task.FromResult(FailureResult);

    public static Result<int, NotFoundError> One(bool succeed) => succeed ? 42 : Missing;

    public static Result<int, NotFoundError, ConflictError> Two(bool succeed) => succeed ? 42 : Stale;

    public static Result<int, NotFoundError, ConflictError, ValidationError> Three(bool succeed) => succeed ? SuccessResult : FailureResult;

    public static Task<Result<int, NotFoundError, ConflictError, ValidationError>> TaskOf(bool succeed, bool yield)
    {
        if (yield)
            return YieldResultTask(succeed);
        return succeed ? CompletedSuccess : CompletedFailure;
    }

    public static ValueTask<Result<int, NotFoundError, ConflictError, ValidationError>> ValueTaskOf(bool succeed, bool yield) =>
        yield ? YieldResultValueTask(succeed) : new ValueTask<Result<int, NotFoundError, ConflictError, ValidationError>>(Three(succeed));

    public static Task<Result<int, NotFoundError, ConflictError, ValidationError>> TaskResult(int value, bool yield) =>
        yield ? YieldResultOfValueTask(value) : Task.FromResult<Result<int, NotFoundError, ConflictError, ValidationError>>(value);

    public static ValueTask<Result<int, NotFoundError, ConflictError, ValidationError>> ValueTaskResult(int value, bool yield) =>
        yield ? YieldResultOfValueValueTask(value) : new ValueTask<Result<int, NotFoundError, ConflictError, ValidationError>>(value);

    public static Task<int> TaskValue(int value, bool yield) => yield ? YieldValue(value) : Task.FromResult(value);

    public static Task<TOut> Completed<TOut>(TOut value, bool yield) => yield ? YieldOf(value) : Task.FromResult(value);

    private static async Task<int> YieldValue(int value)
    {
        await Task.Yield();
        return value;
    }

    private static async Task<TOut> YieldOf<TOut>(TOut value)
    {
        await Task.Yield();
        return value;
    }

    private static async Task<Result<int, NotFoundError, ConflictError, ValidationError>> YieldResultTask(bool succeed)
    {
        await Task.Yield();
        return Three(succeed);
    }

    private static async ValueTask<Result<int, NotFoundError, ConflictError, ValidationError>> YieldResultValueTask(bool succeed)
    {
        await Task.Yield();
        return Three(succeed);
    }

    private static async Task<Result<int, NotFoundError, ConflictError, ValidationError>> YieldResultOfValueTask(int value)
    {
        await Task.Yield();
        return value;
    }

    private static async ValueTask<Result<int, NotFoundError, ConflictError, ValidationError>> YieldResultOfValueValueTask(int value)
    {
        await Task.Yield();
        return value;
    }
}
