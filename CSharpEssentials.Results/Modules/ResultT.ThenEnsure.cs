using System.Runtime.CompilerServices;
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
        return result.IsSuccess ? this : result.ErrorsOrEmptyArray;
    }

    [OverloadResolutionPriority(1)]
    public async Task<Result<TValue>> ThenEnsureAsync(Func<TValue, Task<Result<TValue>>> validator, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return this;
        cancellationToken.ThrowIfCancellationRequested();
        return await validator(Value);
    }

    [OverloadResolutionPriority(1)]
    public async Task<Result<TValue>> ThenEnsureAsync(Func<TValue, Task<Result>> validator, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return this;
        cancellationToken.ThrowIfCancellationRequested();
        Result result = await validator(Value);
        return result.IsSuccess ? this : result.ErrorsOrEmptyArray;
    }

#if NET9_0_OR_GREATER
    public async ValueTask<Result<TValue>> ThenEnsureAsync(Func<TValue, ValueTask<Result<TValue>>> validator, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return this;
        cancellationToken.ThrowIfCancellationRequested();
        return await validator(Value).ConfigureAwait(false);
    }

    public async ValueTask<Result<TValue>> ThenEnsureAsync(Func<TValue, ValueTask<Result>> validator, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return this;
        cancellationToken.ThrowIfCancellationRequested();
        Result result = await validator(Value).ConfigureAwait(false);
        return result.IsSuccess ? this : result.ErrorsOrEmptyArray;
    }
#endif
}

public static partial class ResultExtensions
{
    public static async Task<Result<TValue>> ThenEnsureAsync<TValue>(this Task<Result<TValue>> task, Func<TValue, Task<Result<TValue>>> validator, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task;
        return await result.ThenEnsureAsync(validator, cancellationToken);
    }

    public static async Task<Result<TValue>> ThenEnsureAsync<TValue>(this Task<Result<TValue>> task, Func<TValue, Task<Result>> validator, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task;
        return await result.ThenEnsureAsync(validator, cancellationToken);
    }

    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result<TValue>> ThenEnsureAsync<TValue>(this ValueTask<Result<TValue>> task, Func<TValue, Task<Result<TValue>>> validator, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task;
        return await result.ThenEnsureAsync(validator, cancellationToken);
    }

    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result<TValue>> ThenEnsureAsync<TValue>(this ValueTask<Result<TValue>> task, Func<TValue, Task<Result>> validator, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task;
        return await result.ThenEnsureAsync(validator, cancellationToken);
    }

#if NET9_0_OR_GREATER
    public static async ValueTask<Result<TValue>> ThenEnsureAsync<TValue>(this ValueTask<Result<TValue>> task, Func<TValue, ValueTask<Result<TValue>>> validator, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.ConfigureAwait(false);
        return await result.ThenEnsureAsync(validator, cancellationToken).ConfigureAwait(false);
    }

    public static async ValueTask<Result<TValue>> ThenEnsureAsync<TValue>(this ValueTask<Result<TValue>> task, Func<TValue, ValueTask<Result>> validator, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.ConfigureAwait(false);
        return await result.ThenEnsureAsync(validator, cancellationToken).ConfigureAwait(false);
    }
#endif
}
