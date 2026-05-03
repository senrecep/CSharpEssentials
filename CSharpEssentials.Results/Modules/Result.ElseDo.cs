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

    public async Task<Result> ElseDoAsync(Func<Error[], Task> onFailure, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            await onFailure(Errors).WithCancellation(cancellationToken);
        return this;
    }

    public async Task<Result> ElseDoFirstAsync(Func<Error, Task> onFirstFailure, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            await onFirstFailure(FirstError).WithCancellation(cancellationToken);
        return this;
    }
}

public static partial class ResultExtensions
{
    public static async Task<Result> ElseDoAsync(this Task<Result> task, Func<Error[], Task> onFailure, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return await result.ElseDoAsync(onFailure, cancellationToken);
    }

    public static async Task<Result> ElseDoFirstAsync(this Task<Result> task, Func<Error, Task> onFirstFailure, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return await result.ElseDoFirstAsync(onFirstFailure, cancellationToken);
    }

    public static async ValueTask<Result> ElseDoAsync(this ValueTask<Result> task, Func<Error[], Task> onFailure, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return await result.ElseDoAsync(onFailure, cancellationToken);
    }

    public static async ValueTask<Result> ElseDoFirstAsync(this ValueTask<Result> task, Func<Error, Task> onFirstFailure, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return await result.ElseDoFirstAsync(onFirstFailure, cancellationToken);
    }
}
