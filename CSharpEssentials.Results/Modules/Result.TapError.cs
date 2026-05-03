using CSharpEssentials.Core;
using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result
{
    public Result TapError(Action<Error[]> onFailure)
    {
        if (IsFailure)
            onFailure(Errors);
        return this;
    }

    public Result TapErrorFirst(Action<Error> onFirstFailure)
    {
        if (IsFailure)
            onFirstFailure(FirstError);
        return this;
    }

    public Result TapErrorIf(bool condition, Action<Error[]> onFailure)
    {
        if (IsFailure && condition)
            onFailure(Errors);
        return this;
    }

    public Result TapErrorIf(Func<bool> condition, Action<Error[]> onFailure)
    {
        if (IsFailure && condition())
            onFailure(Errors);
        return this;
    }

    public async Task<Result> TapErrorAsync(Func<Error[], Task> onFailure, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            await onFailure(Errors).WithCancellation(cancellationToken);
        return this;
    }

    public async Task<Result> TapErrorFirstAsync(Func<Error, Task> onFirstFailure, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            await onFirstFailure(FirstError).WithCancellation(cancellationToken);
        return this;
    }
}

public static partial class ResultExtensions
{
    public static async Task<Result> TapErrorAsync(this Task<Result> task, Func<Error[], Task> onFailure, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return await result.TapErrorAsync(onFailure, cancellationToken);
    }

    public static async Task<Result> TapErrorFirstAsync(this Task<Result> task, Func<Error, Task> onFirstFailure, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return await result.TapErrorFirstAsync(onFirstFailure, cancellationToken);
    }

    public static async ValueTask<Result> TapErrorAsync(this ValueTask<Result> task, Func<Error[], Task> onFailure, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return await result.TapErrorAsync(onFailure, cancellationToken);
    }

    public static async ValueTask<Result> TapErrorFirstAsync(this ValueTask<Result> task, Func<Error, Task> onFirstFailure, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return await result.TapErrorFirstAsync(onFirstFailure, cancellationToken);
    }
}
