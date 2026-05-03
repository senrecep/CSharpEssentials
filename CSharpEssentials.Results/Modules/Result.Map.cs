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
            return Errors;
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
            return Errors;
        return map();
    }
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
            return result.Errors;
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

}
