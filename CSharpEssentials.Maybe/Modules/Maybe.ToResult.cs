using CSharpEssentials.Core;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Maybe;

public static partial class MaybeExtensions
{
    /// <summary>
    /// Converts the Maybe to a Result. Alias for ToMaybeResult.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybe"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public static Result<T> ToResult<T>(this Maybe<T> maybe, Error? error = null) =>
        maybe.ToMaybeResult(error);

    /// <summary>
    /// Converts the Maybe to a unit Result. Alias for ToMaybeUnitResult.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybe"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public static Result ToUnitResult<T>(this Maybe<T> maybe, Error? error = null) =>
        maybe.ToMaybeUnitResult(error);

    /// <summary>
    /// Converts the Maybe to a Result asynchronously.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="error"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<T>> ToResultAsync<T>(this Task<Maybe<T>> maybeTask, Error? error = null, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.ToResult(error);
    }

    /// <summary>
    /// Converts the Maybe to a Result asynchronously.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="error"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result<T>> ToResultAsync<T>(this ValueTask<Maybe<T>> maybeTask, Error? error = null, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.ToResult(error);
    }

    /// <summary>
    /// Converts the Maybe to a unit Result asynchronously.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="error"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result> ToUnitResultAsync<T>(this Task<Maybe<T>> maybeTask, Error? error = null, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.ToUnitResult(error);
    }

    /// <summary>
    /// Converts the Maybe to a unit Result asynchronously.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="error"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result> ToUnitResultAsync<T>(this ValueTask<Maybe<T>> maybeTask, Error? error = null, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.ToUnitResult(error);
    }
}
