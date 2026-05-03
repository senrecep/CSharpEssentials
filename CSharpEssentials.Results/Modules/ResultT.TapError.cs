using CSharpEssentials.Core;
using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result<TValue>
{
    public Result<TValue> TapError(Action<Error[]> onFailure)
    {
        if (IsFailure)
            onFailure(Errors);
        return this;
    }

    public Result<TValue> TapErrorFirst(Action<Error> onFirstFailure)
    {
        if (IsFailure)
            onFirstFailure(FirstError);
        return this;
    }

    public Result<TValue> TapErrorIf(bool condition, Action<Error[]> onFailure)
    {
        if (IsFailure && condition)
            onFailure(Errors);
        return this;
    }

    public Result<TValue> TapErrorIf(Func<bool> condition, Action<Error[]> onFailure)
    {
        if (IsFailure && condition())
            onFailure(Errors);
        return this;
    }

    public async Task<Result<TValue>> TapErrorAsync(Func<Error[], Task> onFailure, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            await onFailure(Errors).WithCancellation(cancellationToken);
        return this;
    }

    public async Task<Result<TValue>> TapErrorFirstAsync(Func<Error, Task> onFirstFailure, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            await onFirstFailure(FirstError).WithCancellation(cancellationToken);
        return this;
    }
}

public static partial class ResultExtensions
{
    public static async Task<Result<TValue>> TapErrorAsync<TValue>(this Task<Result<TValue>> task, Func<Error[], Task> onFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.TapErrorAsync(onFailure, cancellationToken);
    }

    public static async Task<Result<TValue>> TapErrorFirstAsync<TValue>(this Task<Result<TValue>> task, Func<Error, Task> onFirstFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.TapErrorFirstAsync(onFirstFailure, cancellationToken);
    }

    public static async ValueTask<Result<TValue>> TapErrorAsync<TValue>(this ValueTask<Result<TValue>> task, Func<Error[], Task> onFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.TapErrorAsync(onFailure, cancellationToken);
    }

    public static async ValueTask<Result<TValue>> TapErrorFirstAsync<TValue>(this ValueTask<Result<TValue>> task, Func<Error, Task> onFirstFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.TapErrorFirstAsync(onFirstFailure, cancellationToken);
    }
}
