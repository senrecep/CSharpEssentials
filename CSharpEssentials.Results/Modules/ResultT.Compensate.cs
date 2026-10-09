using System.Runtime.CompilerServices;
using CSharpEssentials.Core;
using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result<TValue>
{
    public Result<TValue> Compensate(Func<Error[], Result<TValue>> onFailure)
    {
        if (IsSuccess)
            return this;
        return onFailure(Errors);
    }

    public Result<TValue> CompensateFirst(Func<Error, Result<TValue>> onFirstFailure)
    {
        if (IsSuccess)
            return this;
        return onFirstFailure(FirstError);
    }

    [OverloadResolutionPriority(1)]
    public async Task<Result<TValue>> CompensateAsync(Func<Error[], Task<Result<TValue>>> onFailure, CancellationToken cancellationToken = default)
    {
        if (IsSuccess)
            return this;
        return await onFailure(Errors).WithCancellation(cancellationToken).ConfigureAwait(false);
    }

    [OverloadResolutionPriority(1)]
    public async Task<Result<TValue>> CompensateFirstAsync(Func<Error, Task<Result<TValue>>> onFirstFailure, CancellationToken cancellationToken = default)
    {
        if (IsSuccess)
            return this;
        return await onFirstFailure(FirstError).WithCancellation(cancellationToken).ConfigureAwait(false);
    }

#if NET9_0_OR_GREATER
    public async ValueTask<Result<TValue>> CompensateAsync(Func<Error[], ValueTask<Result<TValue>>> onFailure, CancellationToken cancellationToken = default)
    {
        if (IsSuccess)
            return this;
        return await onFailure(Errors).WithCancellation(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<Result<TValue>> CompensateFirstAsync(Func<Error, ValueTask<Result<TValue>>> onFirstFailure, CancellationToken cancellationToken = default)
    {
        if (IsSuccess)
            return this;
        return await onFirstFailure(FirstError).WithCancellation(cancellationToken).ConfigureAwait(false);
    }
#endif
}

public static partial class ResultExtensions
{
    public static async Task<Result<TValue>> CompensateAsync<TValue>(this Task<Result<TValue>> task, Func<Error[], Task<Result<TValue>>> onFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.CompensateAsync(onFailure, cancellationToken).ConfigureAwait(false);
    }

    public static async Task<Result<TValue>> CompensateFirstAsync<TValue>(this Task<Result<TValue>> task, Func<Error, Task<Result<TValue>>> onFirstFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.CompensateFirstAsync(onFirstFailure, cancellationToken).ConfigureAwait(false);
    }

    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result<TValue>> CompensateAsync<TValue>(this ValueTask<Result<TValue>> task, Func<Error[], Task<Result<TValue>>> onFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.CompensateAsync(onFailure, cancellationToken).ConfigureAwait(false);
    }

    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result<TValue>> CompensateFirstAsync<TValue>(this ValueTask<Result<TValue>> task, Func<Error, Task<Result<TValue>>> onFirstFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.CompensateFirstAsync(onFirstFailure, cancellationToken).ConfigureAwait(false);
    }

#if NET9_0_OR_GREATER
    public static async ValueTask<Result<TValue>> CompensateAsync<TValue>(this ValueTask<Result<TValue>> task, Func<Error[], ValueTask<Result<TValue>>> onFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.CompensateAsync(onFailure, cancellationToken).ConfigureAwait(false);
    }

    public static async ValueTask<Result<TValue>> CompensateFirstAsync<TValue>(this ValueTask<Result<TValue>> task, Func<Error, ValueTask<Result<TValue>>> onFirstFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.CompensateFirstAsync(onFirstFailure, cancellationToken).ConfigureAwait(false);
    }
#endif
}
