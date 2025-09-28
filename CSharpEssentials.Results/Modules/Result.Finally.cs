namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result
{
    /// <summary>
    /// Executes a function if the result is a success.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="func"></param>
    /// <returns></returns>
    public TOut Finally<TOut>(Func<Result, TOut> func) => func(this);
}
public static partial class ResultExtensions
{

}