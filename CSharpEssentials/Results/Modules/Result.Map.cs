namespace CSharpEssentials;

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

    public Result<TOut> Map<TOut>(Func<Result<TOut>> map)
    {
        if (IsFailure)
            return Errors;
        return map();
    }
}
public static partial class ResultExtensions
{

}