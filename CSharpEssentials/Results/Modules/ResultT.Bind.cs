namespace CSharpEssentials;

public readonly partial record struct Result<TValue>
{
    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="func"></param>
    /// <returns></returns>
    public Result<TOut> Bind<TOut>(Func<TValue, Result<TOut>> func)
    {
        if (IsFailure)
            return Errors;
        return func(Value);
    }

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <param name="func"></param>
    /// <returns></returns>
    public Result Bind(Func<TValue, Result> func)
    {
        if (IsFailure)
            return Errors;
        return func(Value);
    }

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="func"></param>
    /// <returns></returns>
    public Task<Result<TOut>> Bind<TOut>(Func<TValue, Task<Result<TOut>>> func)
    {
        if (IsFailure)
            return Result<TOut>.Failure(Errors).AsTask();
        return func(Value);
    }

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <param name="func"></param>
    /// <returns></returns>
    public Task<Result> Bind(Func<TValue, Task<Result>> func)
    {
        if (IsFailure)
            return Result.Failure(Errors).AsTask();
        return func(Value);
    }

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="valueTask"></param>
    /// <returns></returns>
    public ValueTask<Result<TOut>> Bind<TOut>(Func<TValue, ValueTask<Result<TOut>>> valueTask)
    {
        if (IsFailure)
            return Result.Failure<TOut>(Errors).AsValueTask();
        return valueTask(Value);
    }


    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <param name="valueTask"></param>
    /// <returns></returns>
    public ValueTask<Result> Bind(Func<TValue, ValueTask<Result>> valueTask)
    {
        if (IsFailure)
            return Result.Failure(Errors).AsValueTask();
        return valueTask(Value);
    }
}


public static partial class ResultExtensions
{
    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="task"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<TOut>> BindAsync<TValue, TOut>(this Task<Result<TValue>> task, Func<TValue, Result<TOut>> func, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
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
    public static async Task<Result<TOut>> BindAsync<TOut>(this Task<Result> task, Func<Result<TOut>> func, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken);
        return result.Bind(func);
    }

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result> BindAsync<TValue>(this Task<Result<TValue>> task, Func<TValue, Result> func, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.Bind(func);
    }

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="task"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<TOut>> BindAsync<TValue, TOut>(this Task<Result<TValue>> task, Func<TValue, Task<Result<TOut>>> func, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.Bind(func).WithCancellation(cancellationToken);
    }

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result> BindAsync<TValue>(this Task<Result<TValue>> task, Func<TValue, Task<Result>> func, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.Bind(func).WithCancellation(cancellationToken);
    }

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="task"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result<TOut>> BindAsync<TValue, TOut>(this ValueTask<Result<TValue>> task, Func<TValue, Result<TOut>> func, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.Bind(func);
    }

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result> BindAsync<TValue>(this ValueTask<Result<TValue>> task, Func<TValue, Result> func, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.Bind(func);
    }

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="task"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result<TOut>> BindAsync<TValue, TOut>(this ValueTask<Result<TValue>> task, Func<TValue, ValueTask<Result<TOut>>> func, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.Bind(func).WithCancellation(cancellationToken);
    }

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result> BindAsync<TValue>(this ValueTask<Result<TValue>> task, Func<TValue, ValueTask<Result>> func, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.Bind(func).WithCancellation(cancellationToken);
    }
}