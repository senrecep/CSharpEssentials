using CSharpEssentials.Core;
using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result<TValue>
{
    public Result<TValue> ElseDo(Action<Error[]> onFailure)
    {
        if (IsFailure)
            onFailure(Errors);
        return this;
    }

    public Result<TValue> ElseDoFirst(Action<Error> onFirstFailure)
    {
        if (IsFailure)
            onFirstFailure(FirstError);
        return this;
    }

    public async Task<Result<TValue>> ElseDoAsync(Func<Error[], Task> onFailure, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            await onFailure(Errors).WithCancellation(cancellationToken);
        return this;
    }

    public async Task<Result<TValue>> ElseDoFirstAsync(Func<Error, Task> onFirstFailure, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            await onFirstFailure(FirstError).WithCancellation(cancellationToken);
        return this;
    }
}

public static partial class ResultExtensions
{
    public static async Task<Result<TValue>> ElseDoAsync<TValue>(this Task<Result<TValue>> task, Func<Error[], Task> onFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.ElseDoAsync(onFailure, cancellationToken);
    }

    public static async Task<Result<TValue>> ElseDoFirstAsync<TValue>(this Task<Result<TValue>> task, Func<Error, Task> onFirstFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.ElseDoFirstAsync(onFirstFailure, cancellationToken);
    }

    public static async ValueTask<Result<TValue>> ElseDoAsync<TValue>(this ValueTask<Result<TValue>> task, Func<Error[], Task> onFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.ElseDoAsync(onFailure, cancellationToken);
    }

    public static async ValueTask<Result<TValue>> ElseDoFirstAsync<TValue>(this ValueTask<Result<TValue>> task, Func<Error, Task> onFirstFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.ElseDoFirstAsync(onFirstFailure, cancellationToken);
    }
}
