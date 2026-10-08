using System.Runtime.CompilerServices;
using CSharpEssentials.Core;
namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result<TValue>
{
    /// <summary>
    /// Maps a function to the result.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="map"></param>
    /// <returns></returns>
    public Result<TOut> Map<TOut>(Func<TValue, TOut> map)
    {
        if (IsFailure)
            return _errors;
        return map(Value);
    }

    /// <summary>
    /// Maps a function to the result.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="map"></param>
    /// <returns></returns>
    public Result<TOut> Map<TOut>(Func<TValue, Result<TOut>> map)
    {
        if (IsFailure)
            return _errors;
        return map(Value);
    }

    /// <summary>
    /// Maps the success value with an async function. On failure the function is never called.
    /// </summary>
    /// <typeparam name="TOut">The type of the mapped value.</typeparam>
    /// <param name="map">Maps the success value.</param>
    /// <param name="cancellationToken">Checked before the function call and observed while awaiting it.</param>
    /// <returns>The mapped success, or a failure with the original errors.</returns>
    [OverloadResolutionPriority(1)]
    public Task<Result<TOut>> MapAsync<TOut>(Func<TValue, Task<TOut>> map, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return Result<TOut>.Failure(_errors).AsTask();
        return MapCoreAsync(Value, map, cancellationToken);
    }

    private static async Task<Result<TOut>> MapCoreAsync<TOut>(TValue value, Func<TValue, Task<TOut>> map, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await map(value).WithCancellation(cancellationToken).ConfigureAwait(false);
    }

#if NET9_0_OR_GREATER
    /// <summary>
    /// Maps the success value with an async function. On failure the function is never called.
    /// </summary>
    /// <typeparam name="TOut">The type of the mapped value.</typeparam>
    /// <param name="map">Maps the success value.</param>
    /// <param name="cancellationToken">Checked before the function call and observed while awaiting it.</param>
    /// <returns>The mapped success, or a failure with the original errors.</returns>
    public ValueTask<Result<TOut>> MapAsync<TOut>(Func<TValue, ValueTask<TOut>> map, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return Result<TOut>.Failure(_errors).AsValueTask();
        return MapCoreAsync(Value, map, cancellationToken);
    }

    private static async ValueTask<Result<TOut>> MapCoreAsync<TOut>(TValue value, Func<TValue, ValueTask<TOut>> map, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await map(value).WithCancellation(cancellationToken).ConfigureAwait(false);
    }
#endif
}

public static partial class ResultExtensions
{
    /// <summary>
    /// Maps a function to the result.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="task"></param>
    /// <param name="map"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<TOut>> MapAsync<TValue, TOut>(this Task<Result<TValue>> task, Func<TValue, TOut> map, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.Map(map);
    }

    /// <summary>
    /// Maps a function to the result.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="task"></param>
    /// <param name="map"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<TOut>> MapAsync<TValue, TOut>(this Task<Result<TValue>> task, Func<TValue, Task<TOut>> map, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        if (result.IsFailure)
            return result.ErrorsOrEmptyArray;
        return await map(result.Value).WithCancellation(cancellationToken);
    }

    /// <summary>
    /// Maps a function to the result.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="task"></param>
    /// <param name="map"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result<TOut>> MapAsync<TValue, TOut>(this ValueTask<Result<TValue>> task, Func<TValue, TOut> map, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.Map(map);
    }

    /// <summary>
    /// Awaits the ValueTask result and maps the success value with an async function. On failure the function is never called.
    /// </summary>
    /// <typeparam name="TValue">The type of the source value.</typeparam>
    /// <typeparam name="TOut">The type of the mapped value.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="map">Maps the success value.</param>
    /// <param name="cancellationToken">Observed while awaiting the source, checked before the function call and observed while awaiting it.</param>
    /// <returns>The mapped success, or a failure with the original errors.</returns>
    public static async ValueTask<Result<TOut>> MapAsync<TValue, TOut>(this ValueTask<Result<TValue>> task, Func<TValue, ValueTask<TOut>> map, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
            return result.ErrorsOrEmptyArray;
        cancellationToken.ThrowIfCancellationRequested();
        return await map(result.Value).WithCancellation(cancellationToken).ConfigureAwait(false);
    }
}
