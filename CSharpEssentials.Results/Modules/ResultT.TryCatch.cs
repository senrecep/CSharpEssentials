using CSharpEssentials.Errors;
using CSharpEssentials.Core;

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
    /// <summary>
    /// Executes a function if the result is a success.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="func"></param>
    /// <param name="error"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result> TryCatchAsync<TValue>(this Task<Result<TValue>> task, Func<TValue, Result> func, Error? error = null, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.TryCatch(func, error);
    }

    /// <summary>
    /// Executes a function if the result is a success.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="task"></param>
    /// <param name="func"></param>
    /// <param name="error"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<TOut>> TryCatchAsync<TValue, TOut>(this Task<Result<TValue>> task, Func<TValue, Result<TOut>> func, Error? error = null, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.TryCatch(func, error);
    }

    /// <summary>
    /// Executes a function if the result is a success.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="func"></param>
    /// <param name="error"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result> TryCatchAsync<TValue>(this ValueTask<Result<TValue>> task, Func<TValue, Result> func, Error? error = null, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.TryCatch(func, error);
    }

    /// <summary>
    /// Executes a function if the result is a success.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="task"></param>
    /// <param name="func"></param>
    /// <param name="error"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result<TOut>> TryCatchAsync<TValue, TOut>(this ValueTask<Result<TValue>> task, Func<TValue, Result<TOut>> func, Error? error = null, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.TryCatch(func, error);
    }
}
