using System.Runtime.CompilerServices;
using CSharpEssentials.Core;
namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result
{
    /// <summary>
    /// Maps a function to the result.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="map"></param>
    /// <returns></returns>
    public Result<TOut> Map<TOut>(Func<TOut> map)
    {
        if (IsFailure)
            return _errors;
        return map();
    }

    /// <summary>
    /// Maps a function to the result.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="map"></param>
    /// <returns></returns>
    public Result<TOut> Map<TOut>(Func<Result<TOut>> map)
    {
        if (IsFailure)
            return _errors;
        return map();
    }

    /// <summary>
    /// Maps the success to a value produced by an async function. On failure the function is never called.
    /// </summary>
    /// <typeparam name="TOut">The type of the mapped value.</typeparam>
    /// <param name="map">Produces the value on success.</param>
    /// <param name="cancellationToken">Checked before the function call and observed while awaiting it.</param>
    /// <returns>The mapped success, or a failure with the original errors.</returns>
    [OverloadResolutionPriority(1)]
    public Task<Result<TOut>> MapAsync<TOut>(Func<Task<TOut>> map, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return Result<TOut>.Failure(_errors).AsTask();
        return MapCoreAsync(map, cancellationToken);
    }

    private static async Task<Result<TOut>> MapCoreAsync<TOut>(Func<Task<TOut>> map, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await map().WithCancellation(cancellationToken).ConfigureAwait(false);
    }

#if NET9_0_OR_GREATER
    /// <summary>
    /// Maps the success to a value produced by an async function. On failure the function is never called.
    /// </summary>
    /// <typeparam name="TOut">The type of the mapped value.</typeparam>
    /// <param name="map">Produces the value on success.</param>
    /// <param name="cancellationToken">Checked before the function call and observed while awaiting it.</param>
    /// <returns>The mapped success, or a failure with the original errors.</returns>
    public ValueTask<Result<TOut>> MapAsync<TOut>(Func<ValueTask<TOut>> map, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return Result<TOut>.Failure(_errors).AsValueTask();
        return MapCoreAsync(map, cancellationToken);
    }

    private static async ValueTask<Result<TOut>> MapCoreAsync<TOut>(Func<ValueTask<TOut>> map, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await map().WithCancellation(cancellationToken).ConfigureAwait(false);
    }
#endif
}

public static partial class ResultExtensions
{
    /// <summary>
    /// Maps a function to the result.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="task"></param>
    /// <param name="map"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<TOut>> MapAsync<TOut>(this Task<Result> task, Func<TOut> map, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return result.Map(map);
    }

    /// <summary>
    /// Maps a function to the result.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="task"></param>
    /// <param name="map"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<TOut>> MapAsync<TOut>(this Task<Result> task, Func<Task<TOut>> map, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        if (result.IsFailure)
            return result.ErrorsOrEmptyArray;
        return await map().WithCancellation(cancellationToken);
    }

    /// <summary>
    /// Maps a function to the result.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="task"></param>
    /// <param name="map"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result<TOut>> MapAsync<TOut>(this ValueTask<Result> task, Func<TOut> map, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return result.Map(map);
    }

    /// <summary>
    /// Awaits the ValueTask result and maps the success to a value produced by an async function. On failure the function is never called.
    /// </summary>
    /// <typeparam name="TOut">The type of the mapped value.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="map">Produces the value on success.</param>
    /// <param name="cancellationToken">Observed while awaiting the source, checked before the function call and observed while awaiting it.</param>
    /// <returns>The mapped success, or a failure with the original errors.</returns>
    public static async ValueTask<Result<TOut>> MapAsync<TOut>(this ValueTask<Result> task, Func<ValueTask<TOut>> map, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
            return result.ErrorsOrEmptyArray;
        cancellationToken.ThrowIfCancellationRequested();
        return await map().WithCancellation(cancellationToken).ConfigureAwait(false);
    }
}
