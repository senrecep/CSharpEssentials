using CSharpEssentials.Core;
using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result<TValue>
{
    public Result<TValue> ThenEnsure(Func<TValue, Result<TValue>> validator)
    {
        if (IsFailure)
            return this;
        return validator(Value);
    }

    public Result<TValue> ThenEnsure(Func<TValue, Result> validator)
    {
        if (IsFailure)
            return this;
        Result result = validator(Value);
        return result.IsSuccess ? this : result.Errors.ToResult<TValue>();
    }

    public async Task<Result<TValue>> ThenEnsureAsync(Func<TValue, Task<Result<TValue>>> validator, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return this;
        return await validator(Value).WithCancellation(cancellationToken);
    }

    public async Task<Result<TValue>> ThenEnsureAsync(Func<TValue, Task<Result>> validator, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return this;
        Result result = await validator(Value).WithCancellation(cancellationToken);
        return result.IsSuccess ? this : result.Errors.ToResult<TValue>();
    }
}

public static partial class ResultExtensions
{
    public static async Task<Result<TValue>> ThenEnsureAsync<TValue>(this Task<Result<TValue>> task, Func<TValue, Task<Result<TValue>>> validator, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.ThenEnsureAsync(validator, cancellationToken);
    }

    public static async Task<Result<TValue>> ThenEnsureAsync<TValue>(this Task<Result<TValue>> task, Func<TValue, Task<Result>> validator, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.ThenEnsureAsync(validator, cancellationToken);
    }

    public static async ValueTask<Result<TValue>> ThenEnsureAsync<TValue>(this ValueTask<Result<TValue>> task, Func<TValue, Task<Result<TValue>>> validator, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.ThenEnsureAsync(validator, cancellationToken);
    }

    public static async ValueTask<Result<TValue>> ThenEnsureAsync<TValue>(this ValueTask<Result<TValue>> task, Func<TValue, Task<Result>> validator, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.ThenEnsureAsync(validator, cancellationToken);
    }
}
