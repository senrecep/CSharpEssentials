using System.Runtime.CompilerServices;
using CSharpEssentials.Core;
using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result
{
    public Result MapError(Func<Error[], Error[]> errorMapper)
    {
        if (IsSuccess)
            return this;
        return errorMapper(Errors);
    }

    public Result MapError(Func<Error, Error> errorMapper)
    {
        if (IsSuccess)
            return this;
        var mappedErrors = new Error[_errors.Length];
        for (int i = 0; i < _errors.Length; i++)
            mappedErrors[i] = errorMapper(_errors[i]);
        return mappedErrors;
    }

    /// <summary>
    /// Maps every error with an async mapper, one at a time and in order. On success the mapper is never called.
    /// </summary>
    /// <param name="errorMapper">Maps a single error. Called once per error.</param>
    /// <param name="cancellationToken">Checked before each mapper call. A running mapper is not abandoned; pass the token into the mapper if it must stop early.</param>
    /// <returns>The original success, or a failure with the mapped errors.</returns>
    [OverloadResolutionPriority(1)]
    public Task<Result> MapErrorAsync(Func<Error, Task<Error>> errorMapper, CancellationToken cancellationToken = default)
    {
        if (IsSuccess)
            return this.AsTask();
        return MapEachErrorAsync(_errors, errorMapper, cancellationToken);
    }

    /// <summary>
    /// Maps every error with an async mapper, one at a time and in order. On success the mapper is never called.
    /// </summary>
    /// <param name="errorMapper">Maps a single error. Called once per error.</param>
    /// <param name="cancellationToken">Checked before each mapper call. A running mapper is not abandoned; pass the token into the mapper if it must stop early.</param>
    /// <returns>The original success, or a failure with the mapped errors.</returns>
    public ValueTask<Result> MapErrorAsync(Func<Error, ValueTask<Error>> errorMapper, CancellationToken cancellationToken = default)
    {
        if (IsSuccess)
            return this.AsValueTask();
        return MapEachErrorAsync(_errors, errorMapper, cancellationToken);
    }

    /// <summary>
    /// Replaces the error array with the result of an async mapper. On success the mapper is never called.
    /// </summary>
    /// <param name="errorMapper">Maps the whole error array. Must return a non-empty array.</param>
    /// <param name="cancellationToken">Checked before the mapper call. A running mapper is not abandoned; pass the token into the mapper if it must stop early.</param>
    /// <returns>The original success, or a failure with the mapped errors.</returns>
    [OverloadResolutionPriority(1)]
    public Task<Result> MapErrorAsync(Func<Error[], Task<Error[]>> errorMapper, CancellationToken cancellationToken = default)
    {
        if (IsSuccess)
            return this.AsTask();
        return MapErrorsAsync(_errors, errorMapper, cancellationToken);
    }

    /// <summary>
    /// Replaces the error array with the result of an async mapper. On success the mapper is never called.
    /// </summary>
    /// <param name="errorMapper">Maps the whole error array. Must return a non-empty array.</param>
    /// <param name="cancellationToken">Checked before the mapper call. A running mapper is not abandoned; pass the token into the mapper if it must stop early.</param>
    /// <returns>The original success, or a failure with the mapped errors.</returns>
    public ValueTask<Result> MapErrorAsync(Func<Error[], ValueTask<Error[]>> errorMapper, CancellationToken cancellationToken = default)
    {
        if (IsSuccess)
            return this.AsValueTask();
        return MapErrorsAsync(_errors, errorMapper, cancellationToken);
    }

    private static async Task<Result> MapEachErrorAsync(Error[] errors, Func<Error, Task<Error>> errorMapper, CancellationToken cancellationToken)
    {
        var mappedErrors = new Error[errors.Length];
        for (int i = 0; i < errors.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            mappedErrors[i] = await errorMapper(errors[i]);
        }
        return mappedErrors;
    }

    private static async ValueTask<Result> MapEachErrorAsync(Error[] errors, Func<Error, ValueTask<Error>> errorMapper, CancellationToken cancellationToken)
    {
        var mappedErrors = new Error[errors.Length];
        for (int i = 0; i < errors.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            mappedErrors[i] = await errorMapper(errors[i]);
        }
        return mappedErrors;
    }

    private static async Task<Result> MapErrorsAsync(Error[] errors, Func<Error[], Task<Error[]>> errorMapper, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await errorMapper(errors);
    }

    private static async ValueTask<Result> MapErrorsAsync(Error[] errors, Func<Error[], ValueTask<Error[]>> errorMapper, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await errorMapper(errors);
    }
}

public static partial class ResultExtensions
{
    /// <summary>
    /// Awaits the Task result and maps every error, in order, with a mapper. On success the mapper is never called.
    /// </summary>
    /// <param name="task">The pending result.</param>
    /// <param name="errorMapper">The error mapper.</param>
    /// <param name="cancellationToken">Cancels waiting for the source and is checked before each mapper call.</param>
    /// <returns>The original success, or a failure with the mapped errors.</returns>
    public static async Task<Result> MapErrorAsync(this Task<Result> task, Func<Error, Error> errorMapper, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return result.MapError(errorMapper);
    }

    /// <summary>
    /// Awaits the Task result and maps the error array with a mapper. On success the mapper is never called.
    /// </summary>
    /// <param name="task">The pending result.</param>
    /// <param name="errorMapper">The error mapper.</param>
    /// <param name="cancellationToken">Cancels waiting for the source and is checked before each mapper call.</param>
    /// <returns>The original success, or a failure with the mapped errors.</returns>
    public static async Task<Result> MapErrorAsync(this Task<Result> task, Func<Error[], Error[]> errorMapper, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return result.MapError(errorMapper);
    }

    /// <summary>
    /// Awaits the Task result and maps every error, in order, with a async mapper. On success the mapper is never called.
    /// </summary>
    /// <param name="task">The pending result.</param>
    /// <param name="errorMapper">The error mapper.</param>
    /// <param name="cancellationToken">Cancels waiting for the source and is checked before each mapper call.</param>
    /// <returns>The original success, or a failure with the mapped errors.</returns>
    public static async Task<Result> MapErrorAsync(this Task<Result> task, Func<Error, Task<Error>> errorMapper, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return await result.MapErrorAsync(errorMapper, cancellationToken);
    }

    /// <summary>
    /// Awaits the Task result and maps the error array with a async mapper. On success the mapper is never called.
    /// </summary>
    /// <param name="task">The pending result.</param>
    /// <param name="errorMapper">The error mapper.</param>
    /// <param name="cancellationToken">Cancels waiting for the source and is checked before each mapper call.</param>
    /// <returns>The original success, or a failure with the mapped errors.</returns>
    public static async Task<Result> MapErrorAsync(this Task<Result> task, Func<Error[], Task<Error[]>> errorMapper, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return await result.MapErrorAsync(errorMapper, cancellationToken);
    }

    /// <summary>
    /// Awaits the ValueTask result and maps every error, in order, with a mapper. On success the mapper is never called.
    /// </summary>
    /// <param name="task">The pending result.</param>
    /// <param name="errorMapper">The error mapper.</param>
    /// <param name="cancellationToken">Cancels waiting for the source and is checked before each mapper call.</param>
    /// <returns>The original success, or a failure with the mapped errors.</returns>
    public static async ValueTask<Result> MapErrorAsync(this ValueTask<Result> task, Func<Error, Error> errorMapper, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return result.MapError(errorMapper);
    }

    /// <summary>
    /// Awaits the ValueTask result and maps the error array with a mapper. On success the mapper is never called.
    /// </summary>
    /// <param name="task">The pending result.</param>
    /// <param name="errorMapper">The error mapper.</param>
    /// <param name="cancellationToken">Cancels waiting for the source and is checked before each mapper call.</param>
    /// <returns>The original success, or a failure with the mapped errors.</returns>
    public static async ValueTask<Result> MapErrorAsync(this ValueTask<Result> task, Func<Error[], Error[]> errorMapper, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return result.MapError(errorMapper);
    }

    /// <summary>
    /// Awaits the ValueTask result and maps every error, in order, with a async mapper. On success the mapper is never called.
    /// </summary>
    /// <param name="task">The pending result.</param>
    /// <param name="errorMapper">The error mapper.</param>
    /// <param name="cancellationToken">Cancels waiting for the source and is checked before each mapper call.</param>
    /// <returns>The original success, or a failure with the mapped errors.</returns>
    public static async ValueTask<Result> MapErrorAsync(this ValueTask<Result> task, Func<Error, ValueTask<Error>> errorMapper, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return await result.MapErrorAsync(errorMapper, cancellationToken);
    }

    /// <summary>
    /// Awaits the ValueTask result and maps the error array with a async mapper. On success the mapper is never called.
    /// </summary>
    /// <param name="task">The pending result.</param>
    /// <param name="errorMapper">The error mapper.</param>
    /// <param name="cancellationToken">Cancels waiting for the source and is checked before each mapper call.</param>
    /// <returns>The original success, or a failure with the mapped errors.</returns>
    public static async ValueTask<Result> MapErrorAsync(this ValueTask<Result> task, Func<Error[], ValueTask<Error[]>> errorMapper, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return await result.MapErrorAsync(errorMapper, cancellationToken);
    }
}
