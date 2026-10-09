using System.Runtime.CompilerServices;
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

    [OverloadResolutionPriority(1)]
    public async Task<Result<TValue>> ElseDoAsync(Func<Error[], Task> onFailure, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            await onFailure(Errors).WithCancellation(cancellationToken);
        return this;
    }

    [OverloadResolutionPriority(1)]
    public async Task<Result<TValue>> ElseDoFirstAsync(Func<Error, Task> onFirstFailure, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            await onFirstFailure(FirstError).WithCancellation(cancellationToken);
        return this;
    }

#if NET9_0_OR_GREATER
    public async ValueTask<Result<TValue>> ElseDoAsync(Func<Error[], ValueTask> onFailure, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            await onFailure(Errors).WithCancellation(cancellationToken).ConfigureAwait(false);
        return this;
    }

    public async ValueTask<Result<TValue>> ElseDoFirstAsync(Func<Error, ValueTask> onFirstFailure, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            await onFirstFailure(FirstError).WithCancellation(cancellationToken).ConfigureAwait(false);
        return this;
    }
#endif
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

    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result<TValue>> ElseDoAsync<TValue>(this ValueTask<Result<TValue>> task, Func<Error[], Task> onFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.ElseDoAsync(onFailure, cancellationToken);
    }

    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result<TValue>> ElseDoFirstAsync<TValue>(this ValueTask<Result<TValue>> task, Func<Error, Task> onFirstFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.ElseDoFirstAsync(onFirstFailure, cancellationToken);
    }

#if NET9_0_OR_GREATER
    public static async ValueTask<Result<TValue>> ElseDoAsync<TValue>(this ValueTask<Result<TValue>> task, Func<Error[], ValueTask> onFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.ElseDoAsync(onFailure, cancellationToken).ConfigureAwait(false);
    }

    public static async ValueTask<Result<TValue>> ElseDoFirstAsync<TValue>(this ValueTask<Result<TValue>> task, Func<Error, ValueTask> onFirstFailure, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.ElseDoFirstAsync(onFirstFailure, cancellationToken).ConfigureAwait(false);
    }
#endif
}
