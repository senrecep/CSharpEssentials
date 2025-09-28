using CSharpEssentials.Core;

namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result
{
    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="func"></param>
    /// <returns></returns>
    public Result<TOut> Bind<TOut>(Func<Result<TOut>> func)
    {
        if (IsFailure)
            return Errors;
        return func();
    }

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <param name="func"></param>
    /// <returns></returns>
    public Result Bind(Func<Result> func)
    {
        if (IsFailure)
            return Errors;
        return func();
    }

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="func"></param>
    /// <returns></returns>
    public Task<Result<TOut>> Bind<TOut>(Func<Task<Result<TOut>>> func)
    {
        if (IsFailure)
            return Result<TOut>.Failure(Errors).AsTask();
        return func();
    }

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <param name="func"></param>
    /// <returns></returns>
    public Task<Result> Bind(Func<Task<Result>> func)
    {
        if (IsFailure)
            return Failure(Errors).AsTask();
        return func();
    }

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="valueTask"></param>
    /// <returns></returns>
    public ValueTask<Result<TOut>> Bind<TOut>(Func<ValueTask<Result<TOut>>> valueTask)
    {
        if (IsFailure)
            return Failure<TOut>(Errors).AsValueTask();
        return valueTask();
    }

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <param name="valueTask"></param>
    /// <returns></returns>
    public ValueTask<Result> Bind(Func<ValueTask<Result>> valueTask)
    {
        if (IsFailure)
            return Failure(Errors).AsValueTask();
        return valueTask();
    }
}


public static partial class ResultExtensions
{
    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <param name="task"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result> BindAsync(this Task<Result> task, Func<Result> func, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return result.Bind(func);
    }

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <param name="task"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result> BindAsync(this Task<Result> task, Func<Task<Result>> func, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return await result.Bind(func).WithCancellation(cancellationToken);
    }

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="task"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result<TOut>> BindAsync<TOut>(this ValueTask<Result> task, Func<Result<TOut>> func, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return result.Bind(func);
    }

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <param name="task"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result> BindAsync(this ValueTask<Result> task, Func<Result> func, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return result.Bind(func);
    }

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="task"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result<TOut>> BindAsync<TOut>(this ValueTask<Result> task, Func<ValueTask<Result<TOut>>> func, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return await result.Bind(func).WithCancellation(cancellationToken);
    }

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <param name="task"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result> BindAsync(this ValueTask<Result> task, Func<ValueTask<Result>> func, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return await result.Bind(func).WithCancellation(cancellationToken);
    }
}