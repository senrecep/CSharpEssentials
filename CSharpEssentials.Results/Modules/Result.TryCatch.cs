using CSharpEssentials.Core;
using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

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
            return IsSuccess ? func() : Errors;
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
    /// <summary>
    /// Executes a function if the result is a success.
    /// </summary>
    /// <param name="task"></param>
    /// <param name="func"></param>
    /// <param name="error"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result> TryCatchAsync(this Task<Result> task, Func<Result> func, Error? error = null, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return result.TryCatch(func, error);
    }

    /// <summary>
    /// Executes a function if the result is a success.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="task"></param>
    /// <param name="func"></param>
    /// <param name="error"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<TOut>> TryCatchAsync<TOut>(this Task<Result> task, Func<Result<TOut>> func, Error? error = null, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return result.TryCatch(func, error);
    }

    /// <summary>
    /// Executes a function if the result is a success.
    /// </summary>
    /// <param name="task"></param>
    /// <param name="func"></param>
    /// <param name="error"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result> TryCatchAsync(this ValueTask<Result> task, Func<Result> func, Error? error = null, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return result.TryCatch(func, error);
    }

    /// <summary>
    /// Executes a function if the result is a success.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="task"></param>
    /// <param name="func"></param>
    /// <param name="error"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result<TOut>> TryCatchAsync<TOut>(this ValueTask<Result> task, Func<Result<TOut>> func, Error? error = null, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return result.TryCatch(func, error);
    }
}
