using System.Runtime.CompilerServices;
using CSharpEssentials.Core;
using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result
{
    public Result ElseDo(Action<Error[]> onFailure)
    {
        if (IsFailure)
            onFailure(Errors);
        return this;
    }

    public Result ElseDoFirst(Action<Error> onFirstFailure)
    {
        if (IsFailure)
            onFirstFailure(FirstError);
        return this;
    }

    [OverloadResolutionPriority(1)]
    public async Task<Result> ElseDoAsync(Func<Error[], Task> onFailure, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            await onFailure(Errors).WithCancellation(cancellationToken).ConfigureAwait(false);
        return this;
    }

    [OverloadResolutionPriority(1)]
    public async Task<Result> ElseDoFirstAsync(Func<Error, Task> onFirstFailure, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            await onFirstFailure(FirstError).WithCancellation(cancellationToken).ConfigureAwait(false);
        return this;
    }

#if NET9_0_OR_GREATER
    public async ValueTask<Result> ElseDoAsync(Func<Error[], ValueTask> onFailure, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            await onFailure(Errors).WithCancellation(cancellationToken).ConfigureAwait(false);
        return this;
    }

    public async ValueTask<Result> ElseDoFirstAsync(Func<Error, ValueTask> onFirstFailure, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            await onFirstFailure(FirstError).WithCancellation(cancellationToken).ConfigureAwait(false);
        return this;
    }
#endif
}

public static partial class ResultExtensions
{
    public static async Task<Result> ElseDoAsync(this Task<Result> task, Func<Error[], Task> onFailure, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.ElseDoAsync(onFailure, cancellationToken).ConfigureAwait(false);
    }

    public static async Task<Result> ElseDoFirstAsync(this Task<Result> task, Func<Error, Task> onFirstFailure, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.ElseDoFirstAsync(onFirstFailure, cancellationToken).ConfigureAwait(false);
    }

    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result> ElseDoAsync(this ValueTask<Result> task, Func<Error[], Task> onFailure, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.ElseDoAsync(onFailure, cancellationToken).ConfigureAwait(false);
    }

    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result> ElseDoFirstAsync(this ValueTask<Result> task, Func<Error, Task> onFirstFailure, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.ElseDoFirstAsync(onFirstFailure, cancellationToken).ConfigureAwait(false);
    }

#if NET9_0_OR_GREATER
    public static async ValueTask<Result> ElseDoAsync(this ValueTask<Result> task, Func<Error[], ValueTask> onFailure, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.ElseDoAsync(onFailure, cancellationToken).ConfigureAwait(false);
    }

    public static async ValueTask<Result> ElseDoFirstAsync(this ValueTask<Result> task, Func<Error, ValueTask> onFirstFailure, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.ElseDoFirstAsync(onFirstFailure, cancellationToken).ConfigureAwait(false);
    }
#endif
}
