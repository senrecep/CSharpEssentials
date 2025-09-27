namespace CSharpEssentials.Results;

public readonly partial record struct Result
{
    /// <summary>
    /// Executes an action if the result is a success.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public Result Tap(Action action)
    {
        if (IsSuccess)
            action();
        return this;
    }

    /// <summary>
    /// Executes an action if the result is a success.
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public Result Tap(bool condition, Action action)
    {
        if (IsSuccess && condition)
            action();
        return this;
    }

    public Result Tap(Func<bool> condition, Action action)
    {
        if (IsSuccess && condition())
            action();
        return this;
    }
}
public static partial class ResultExtensions
{

}