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

}