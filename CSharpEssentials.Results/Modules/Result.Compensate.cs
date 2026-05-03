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

    public async Task<Result> CompensateAsync(Func<Error[], Task<Result>> onFailure, CancellationToken cancellationToken = default)
    {
        if (IsSuccess)
            return this;
        return await onFailure(Errors).WithCancellation(cancellationToken);
    }

    public async Task<Result> CompensateFirstAsync(Func<Error, Task<Result>> onFirstFailure, CancellationToken cancellationToken = default)
    {
        if (IsSuccess)
            return this;
        return await onFirstFailure(FirstError).WithCancellation(cancellationToken);
    }
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

    public static async ValueTask<Result> CompensateAsync(this ValueTask<Result> task, Func<Error[], Task<Result>> onFailure, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return await result.CompensateAsync(onFailure, cancellationToken);
    }

    public static async ValueTask<Result> CompensateFirstAsync(this ValueTask<Result> task, Func<Error, Task<Result>> onFirstFailure, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return await result.CompensateFirstAsync(onFirstFailure, cancellationToken);
    }
}
