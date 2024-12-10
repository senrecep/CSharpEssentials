namespace CSharpEssentials;

public readonly partial record struct Result
{
    /// <summary>
    /// Executes a function if the result is a success.
    /// </summary>
    /// <param name="func"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public Result TryCatch(Func<Result> func, Error? error = null)
    {
        try
        {
            return IsSuccess ? func() : this;
        }
        catch (Exception ex)
        {
            return error ?? Error.Exception(ex);
        }
    }

    public Result<TOut> TryCatch<TOut>(Func<Result<TOut>> func, Error? error = null)
    {
        try
        {
            return IsSuccess ? func() : Errors;
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