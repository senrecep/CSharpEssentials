using System.Runtime.CompilerServices;
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

    [OverloadResolutionPriority(1)]
    public async Task<Result> TapErrorAsync(Func<Error[], Task> onFailure, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            await onFailure(Errors).WithCancellation(cancellationToken).ConfigureAwait(false);
        return this;
    }

    [OverloadResolutionPriority(1)]
    public async Task<Result> TapErrorFirstAsync(Func<Error, Task> onFirstFailure, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            await onFirstFailure(FirstError).WithCancellation(cancellationToken).ConfigureAwait(false);
        return this;
    }

#if NET9_0_OR_GREATER
    public async ValueTask<Result> TapErrorAsync(Func<Error[], ValueTask> onFailure, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            await onFailure(Errors).WithCancellation(cancellationToken).ConfigureAwait(false);
        return this;
    }

    public async ValueTask<Result> TapErrorFirstAsync(Func<Error, ValueTask> onFirstFailure, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            await onFirstFailure(FirstError).WithCancellation(cancellationToken).ConfigureAwait(false);
        return this;
    }
#endif
}

public static partial class ResultExtensions
{
    public static async Task<Result> TapErrorAsync(this Task<Result> task, Func<Error[], Task> onFailure, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.TapErrorAsync(onFailure, cancellationToken).ConfigureAwait(false);
    }

    public static async Task<Result> TapErrorFirstAsync(this Task<Result> task, Func<Error, Task> onFirstFailure, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.TapErrorFirstAsync(onFirstFailure, cancellationToken).ConfigureAwait(false);
    }

    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result> TapErrorAsync(this ValueTask<Result> task, Func<Error[], Task> onFailure, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.TapErrorAsync(onFailure, cancellationToken).ConfigureAwait(false);
    }

    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result> TapErrorFirstAsync(this ValueTask<Result> task, Func<Error, Task> onFirstFailure, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.TapErrorFirstAsync(onFirstFailure, cancellationToken).ConfigureAwait(false);
    }

#if NET9_0_OR_GREATER
    public static async ValueTask<Result> TapErrorAsync(this ValueTask<Result> task, Func<Error[], ValueTask> onFailure, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.TapErrorAsync(onFailure, cancellationToken).ConfigureAwait(false);
    }

    public static async ValueTask<Result> TapErrorFirstAsync(this ValueTask<Result> task, Func<Error, ValueTask> onFirstFailure, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.TapErrorFirstAsync(onFirstFailure, cancellationToken).ConfigureAwait(false);
    }
#endif
}
