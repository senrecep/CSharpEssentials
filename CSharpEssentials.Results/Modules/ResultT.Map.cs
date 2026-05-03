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
            return Errors;
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
            return Errors;
        return map(Value);
    }
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
            return result.Errors;
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

}
