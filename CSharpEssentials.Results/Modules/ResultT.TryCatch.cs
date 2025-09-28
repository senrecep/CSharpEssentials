using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result<TValue>
{
    /// <summary>
    /// Executes a function if the result is a success.
    /// </summary>
    /// <param name="func"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public Result TryCatch(Func<TValue, Result> func, Error? error = null)
    {
        try
        {
            return IsSuccess ? func(Value) : Errors;
        }
        catch (Exception ex)
        {
            return error ?? Error.Exception(ex);
        }
    }

    /// <summary>
    /// Executes a function if the result is a success.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="func"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public Result<TOut> TryCatch<TOut>(Func<TValue, Result<TOut>> func, Error? error = null)
    {
        try
        {
            return IsSuccess ? func(Value) : Errors;
        }
        catch (Exception ex)
        {
            return error ?? Error.Exception(ex);
        }
    }
}
public static partial class ResultExtensions
{

}