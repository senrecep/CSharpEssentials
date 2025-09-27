namespace CSharpEssentials.Results;

public readonly partial record struct Result
{
    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public Result BindIf(bool condition, Func<Result> func) =>
        condition ? Bind(func) : this;

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public Result BindIf(Func<bool> predicate, Func<Result> func) =>
        predicate() ? Bind(func) : this;
}
public static partial class ResultExtensions
{

}