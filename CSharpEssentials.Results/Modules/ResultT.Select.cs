using CSharpEssentials.Core;
namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result<TValue>
{
    /// <summary>
    /// Maps a function to the result.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="selector"></param>
    /// <returns></returns>
    public Result<TOut> Select<TOut>(Func<TValue, TOut> selector)
    {
        return Map(selector);
    }

    /// <summary>
    /// Maps a function to the result.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="selector"></param>
    /// <returns></returns>
    public Result<TOut> Select<TOut>(Func<TValue, Result<TOut>> selector)
    {
        return Map(selector);
    }

    /// <summary>
    /// Maps a function to the result.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="selector"></param>
    /// <returns></returns>
    public Result<TOut> SelectMany<TOut>(Func<TValue, Result<TOut>> selector)
    {
        return Map(selector);
    }

    /// <summary>
    /// Maps a function to the result.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="selector"></param>
    /// <returns></returns>
    public Result<TOut> SelectMany<TOut>(Func<TValue, TOut> selector)
    {
        return Map(selector);
    }

    /// <summary>
    /// Maps a function to the result.
    /// </summary>
    /// <typeparam name="TIntermediate"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="selector"></param>
    /// <param name="projector"></param>
    /// <returns></returns>
    public Result<TOut> SelectMany<TIntermediate, TOut>(
        Func<TValue, Result<TIntermediate>> selector,
        Func<TValue, TIntermediate, TOut> projector)
    {
        TValue? value = Value;
        return
            Bind(selector)
            .Map(intermediate => projector(value, intermediate));
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
    /// <param name="selector"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<TOut>> SelectAsync<TValue, TOut>(this Task<Result<TValue>> task, Func<TValue, TOut> selector, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.Select(selector);
    }

    /// <summary>
    /// Maps a function to the result.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="task"></param>
    /// <param name="selector"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<TOut>> SelectAsync<TValue, TOut>(this Task<Result<TValue>> task, Func<TValue, Task<TOut>> selector, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        if (result.IsFailure)
            return result.Errors;
        return await selector(result.Value).WithCancellation(cancellationToken);
    }

    /// <summary>
    /// Maps a function to the result.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="task"></param>
    /// <param name="selector"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result<TOut>> SelectAsync<TValue, TOut>(this ValueTask<Result<TValue>> task, Func<TValue, TOut> selector, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.Select(selector);
    }

}
