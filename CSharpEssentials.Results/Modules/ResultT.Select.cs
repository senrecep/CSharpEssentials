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

}