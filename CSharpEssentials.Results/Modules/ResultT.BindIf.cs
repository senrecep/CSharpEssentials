namespace CSharpEssentials.Results;

public readonly partial record struct Result<TValue>
{
    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public Result<TValue> BindIf(bool condition, Func<TValue, Result<TValue>> func) =>
         condition ? Bind(func) : this;

    public Result<TValue> BindIf(Func<bool> predicate, Func<TValue, Result<TValue>> func) =>
        predicate() ? Bind(func) : this;

    public Result<TValue> BindIf(Func<TValue, bool> predicate, Func<TValue, Result<TValue>> func) =>
        IsSuccess && predicate(Value) ? Bind(func) : this;
}
public static partial class ResultExtensions
{

}