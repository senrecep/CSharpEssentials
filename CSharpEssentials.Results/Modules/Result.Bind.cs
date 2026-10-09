using System.Runtime.CompilerServices;
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
            return _errors;
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
            return this;
        return func();
    }

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="func"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(1)]
    public Task<Result<TOut>> BindAsync<TOut>(Func<Task<Result<TOut>>> func)
    {
        if (IsFailure)
            return Result<TOut>.Failure(_errors).AsTask();
        return func();
    }

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <param name="func"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(1)]
    public Task<Result> BindAsync(Func<Task<Result>> func)
    {
        if (IsFailure)
            return this.AsTask();
        return func();
    }

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="valueTask"></param>
    /// <returns></returns>
    public ValueTask<Result<TOut>> BindAsync<TOut>(Func<ValueTask<Result<TOut>>> valueTask)
    {
        if (IsFailure)
            return Result<TOut>.Failure(_errors).AsValueTask();
        return valueTask();
    }

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <param name="valueTask"></param>
    /// <returns></returns>
    public ValueTask<Result> BindAsync(Func<ValueTask<Result>> valueTask)
    {
        if (IsFailure)
            return this.AsValueTask();
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
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
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
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
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
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.BindAsync(func).WithCancellation(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Awaits the Task result and binds an async function that produces a typed result. On failure the function is never called.
    /// </summary>
    /// <typeparam name="TOut">The value type of the produced result.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="func">Produces the next result on success.</param>
    /// <param name="cancellationToken">Observed while awaiting the source and the function.</param>
    /// <returns>The produced result, or a failure with the original errors.</returns>
    public static async Task<Result<TOut>> BindAsync<TOut>(this Task<Result> task, Func<Task<Result<TOut>>> func, CancellationToken cancellationToken = default)
    {
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.BindAsync(func).WithCancellation(cancellationToken).ConfigureAwait(false);
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
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
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
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
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
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.BindAsync(func).WithCancellation(cancellationToken).ConfigureAwait(false);
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
        Result result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.BindAsync(func).WithCancellation(cancellationToken).ConfigureAwait(false);
    }
}
