namespace CSharpEssentials.Results;

public readonly partial record struct Result<TValue>
{
    /// <summary>
    /// Executes a function if the result is a success.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public Result<TValue> Tap(Action<TValue> action)
    {
        if (IsSuccess)
            action(Value);
        return this;
    }

    /// <summary>
    /// Executes a function if the result is a success.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public Result<TValue> Tap(Action action)
    {
        if (IsSuccess)
            action();
        return this;
    }

    /// <summary>
    /// Executes a function if the result is a success.
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public Result<TValue> Tap(bool condition, Action<TValue> action)
    {
        if (IsSuccess && condition)
            action(Value);
        return this;
    }

    /// <summary>
    /// Executes a function if the result is a success.
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public Result<TValue> Tap(bool condition, Action action)
    {
        if (IsSuccess && condition)
            action();
        return this;
    }

    /// <summary>
    /// Executes a function if the result is a success.
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public Result<TValue> Tap(Func<bool> condition, Action<TValue> action)
    {
        if (IsSuccess && condition())
            action(Value);
        return this;
    }

    /// <summary>
    /// Executes a function if the result is a success.
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public Result<TValue> Tap(Func<bool> condition, Action action)
    {
        if (IsSuccess && condition())
            action();
        return this;
    }
}
public static partial class ResultExtensions
{

}