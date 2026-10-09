using System.Runtime.CompilerServices;
using CSharpEssentials.Core;
using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result
{
    public Result Compensate(Func<Error[], Result> onFailure)
    {
        if (IsSuccess)
            return this;
        return onFailure(Errors);
    }

    public Result CompensateFirst(Func<Error, Result> onFirstFailure)
    {
        if (IsSuccess)
            return this;
        return onFirstFailure(FirstError);
    }

    [OverloadResolutionPriority(1)]
    public async Task<Result> CompensateAsync(Func<Error[], Task<Result>> onFailure, CancellationToken cancellationToken = default)
    {
        if (IsSuccess)
            return this;
        return await onFailure(Errors).WithCancellation(cancellationToken);
    }

    [OverloadResolutionPriority(1)]
    public async Task<Result> CompensateFirstAsync(Func<Error, Task<Result>> onFirstFailure, CancellationToken cancellationToken = default)
    {
        if (IsSuccess)
            return this;
        return await onFirstFailure(FirstError).WithCancellation(cancellationToken);
    }

#if NET9_0_OR_GREATER
    public async ValueTask<Result> CompensateAsync(Func<Error[], ValueTask<Result>> onFailure, CancellationToken cancellationToken = default)
    {
        if (IsSuccess)
            return this;
        return await onFailure(Errors).WithCancellation(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<Result> CompensateFirstAsync(Func<Error, ValueTask<Result>> onFirstFailure, CancellationToken cancellationToken = default)
    {
        if (IsSuccess)
            return this;
        return await onFirstFailure(FirstError).WithCancellation(cancellationToken).ConfigureAwait(false);
    }
#endif
}

public static partial class ResultExtensions
{
    public static async Task<Result> CompensateAsync(this Task<Result> task, Func<Error[], Task<Result>> onFailure, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return await result.CompensateAsync(onFailure, cancellationToken);
    }

    public static async Task<Result> CompensateFirstAsync(this Task<Result> task, Func<Error, Task<Result>> onFirstFailure, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return await result.CompensateFirstAsync(onFirstFailure, cancellationToken);
    }

    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result> CompensateAsync(this ValueTask<Result> task, Func<Error[], Task<Result>> onFailure, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return await result.CompensateAsync(onFailure, cancellationToken);
    }

    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result> CompensateFirstAsync(this ValueTask<Result> task, Func<Error, Task<Result>> onFirstFailure, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return await result.CompensateFirstAsync(onFirstFailure, cancellationToken);
    }

#if NET9_0_OR_GREATER
    public static async ValueTask<Result> CompensateAsync(this ValueTask<Result> task, Func<Error[], ValueTask<Result>> onFailure, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.CompensateAsync(onFailure, cancellationToken).ConfigureAwait(false);
    }

    public static async ValueTask<Result> CompensateFirstAsync(this ValueTask<Result> task, Func<Error, ValueTask<Result>> onFirstFailure, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.CompensateFirstAsync(onFirstFailure, cancellationToken).ConfigureAwait(false);
    }
#endif
}
